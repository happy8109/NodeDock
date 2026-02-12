using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NodeDock.Models;
using NodeDock.Utils;

namespace NodeDock.Services
{
    public class NodeProcessWorker : IDisposable
    {
        private Process _process;
        private readonly AppItem _app;
        private readonly string _nodeExePath;
        private JobObject _jobObject;
        
        // 进程守护相关
        private bool _isManualStop = false;  // 是否为用户主动停止
        private List<DateTime> _restartHistory = new List<DateTime>();  // 重启历史记录
        
        // 进程监控相关
        private long _lastCpuTime = 0;
        private DateTime _lastSampleTime = DateTime.MinValue;
        
        // 事件定义
        public event Action<string, string> OutputReceived; // (appId, message)
        public event Action<string, AppStatus> StatusChanged; // (appId, status)

        public NodeProcessWorker(AppItem app, string nodeExePath)
        {
            _app = app;
            _nodeExePath = nodeExePath;
        }

        public AppStatus Status => _app.Status;
        public AppItem App => _app;

        public void Start()
        {
            if (_process != null && !_process.HasExited) return;
            
            // 重置手动停止标识
            _isManualStop = false;

            if (string.IsNullOrEmpty(_nodeExePath) || !File.Exists(_nodeExePath))
            {
                OnStatusChanged(AppStatus.Error);
                OnOutputReceived($"错误：未找到 Node 执行程序路径 \"{_nodeExePath}\"");
                return;
            }

            try
            {
                OnStatusChanged(AppStatus.Starting);

                // Windows 10+：创建作业对象以管理进程树
                // Windows 7：跳过 JobObject（不支持嵌套 Job）
                if (!Win7CompatibilityUtil.IsWindows7OrLower)
                {
                    _jobObject?.Dispose();
                    _jobObject = new JobObject();
                }

                _process = new Process();
                string nodeDir = Path.GetDirectoryName(_nodeExePath);
                
                // 智能处理入口脚本：如果用户习惯性输入了 "node app.js"，自动剥离前面的 "node "
                string entry = _app.EntryScript?.Trim() ?? "";
                if (entry.StartsWith("node ", StringComparison.OrdinalIgnoreCase))
                {
                    entry = entry.Substring(5).Trim();
                }

                bool isNpm = entry.StartsWith("npm ", StringComparison.OrdinalIgnoreCase) || 
                           entry.Equals("npm", StringComparison.OrdinalIgnoreCase);

                if (isNpm)
                {
                    // 处理 npm 命令
                    string npmPath = Path.Combine(nodeDir, "npm.cmd");
                    if (!File.Exists(npmPath))
                    {
                        npmPath = "npm.cmd"; // 回退
                    }

                    _process.StartInfo.FileName = npmPath;
                    
                    // 提取 npm 后面的参数
                    string npmArgs = entry.Length > 3 ? entry.Substring(4).Trim() : "";
                    _process.StartInfo.Arguments = string.IsNullOrEmpty(_app.Arguments) 
                        ? npmArgs 
                        : $"{npmArgs} {_app.Arguments}";
                }
                else
                {
                    // 处理普通 node 执行
                    _process.StartInfo.FileName = _nodeExePath;
                    string args = string.IsNullOrEmpty(_app.Arguments) ? "" : _app.Arguments + " ";
                    _process.StartInfo.Arguments = args + $"\"{entry}\"";
                }

                // 环境隔离
                if (_process.StartInfo.EnvironmentVariables.ContainsKey("PATH"))
                {
                    _process.StartInfo.EnvironmentVariables["PATH"] = nodeDir + ";" + _process.StartInfo.EnvironmentVariables["PATH"];
                }
                else
                {
                    _process.StartInfo.EnvironmentVariables["PATH"] = nodeDir;
                }
                
                _process.StartInfo.WorkingDirectory = _app.WorkingDirectory;
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                _process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                // 注入 PATH 环境变量，确保 npm 和其他工具可用
                string runtimeDir = Path.GetDirectoryName(_nodeExePath);
                if (Directory.Exists(runtimeDir))
                {
                    string oldPath = Environment.GetEnvironmentVariable("PATH");
                    _process.StartInfo.EnvironmentVariables["PATH"] = runtimeDir + ";" + oldPath;
                }

                // 绑定输出流
                _process.OutputDataReceived += (s, e) => { if (e.Data != null) OnOutputReceived(e.Data); };
                _process.ErrorDataReceived += (s, e) => { if (e.Data != null) OnOutputReceived("[ERROR] " + e.Data); };

                // 绑定退出事件
                _process.EnableRaisingEvents = true;
                _process.Exited += OnProcessExited;

                if (_process.Start())
                {
                    // Windows 10+：将进程绑定到 JobObject
                    if (_jobObject != null)
                    {
                        try
                        {
                            _jobObject.AddProcess(_process.Handle);
                        }
                        catch (Exception ex)
                        {
                            OnOutputReceived($"警告：无法建立 Job 绑定 ({ex.Message})，子进程清理可能失效。");
                        }
                    }

                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();
                    _app.StartTime = DateTime.Now;
                    OnStatusChanged(AppStatus.Running);
                }
                else
                {
                    OnStatusChanged(AppStatus.Error);
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged(AppStatus.Error);
                OnOutputReceived($"启动失败：{ex.Message}");
            }
        }
        
        /// <summary>
        /// 进程退出事件处理
        /// </summary>
        private void OnProcessExited(object sender, EventArgs e)
        {
            _app.StartTime = null;
            
            // 获取退出码
            int exitCode = 0;
            try
            {
                exitCode = _process?.ExitCode ?? 0;
            }
            catch { }
            
            // 判断是否需要自动重启
            if (_isManualStop)
            {
                // 用户主动停止，不重启
                OnStatusChanged(AppStatus.Stopped);
                OnOutputReceived("--- 程序已停止 ---");
            }
            else if (exitCode == 0)
            {
                // 正常退出，不重启
                OnStatusChanged(AppStatus.Stopped);
                OnOutputReceived("--- 程序正常退出 ---");
            }
            else if (_app.EnableAutoRestart)
            {
                // 异常退出且开启了自动重启
                OnOutputReceived($"--- 程序异常退出 (ExitCode: {exitCode}) ---");
                TryAutoRestart();
            }
            else
            {
                // 异常退出但未开启自动重启
                if (_app.Status != AppStatus.Error) OnStatusChanged(AppStatus.Stopped);
                OnOutputReceived($"--- 程序异常退出 (ExitCode: {exitCode}) ---");
            }
        }
        
        /// <summary>
        /// 尝试自动重启
        /// </summary>
        private void TryAutoRestart()
        {
            // 清理过期的重启记录（超过时间窗口）
            var windowStart = DateTime.Now.AddMinutes(-_app.RestartWindowMinutes);
            _restartHistory.RemoveAll(t => t < windowStart);
            
            // 检查重启次数是否已达上限
            if (_restartHistory.Count >= _app.MaxRestartAttempts)
            {
                OnStatusChanged(AppStatus.Error);
                OnOutputReceived($"*** 已达最大重启次数 ({_app.MaxRestartAttempts} 次/{_app.RestartWindowMinutes} 分钟)，停止自动重启 ***");
                return;
            }
            
            // 记录本次重启
            _restartHistory.Add(DateTime.Now);
            int attempt = _restartHistory.Count;
            
            OnStatusChanged(AppStatus.Restarting);
            OnOutputReceived($"*** 将在 {_app.RestartDelaySeconds} 秒后自动重启 (第 {attempt}/{_app.MaxRestartAttempts} 次) ***");
            
            // 延迟后重启
            Task.Delay(_app.RestartDelaySeconds * 1000).ContinueWith(_ =>
            {
                // 再次检查是否被手动停止
                if (_isManualStop)
                {
                    OnStatusChanged(AppStatus.Stopped);
                    OnOutputReceived("*** 自动重启已取消（用户手动停止）***");
                    return;
                }
                
                OnOutputReceived("*** 正在自动重启... ***");
                Start();
            });
        }

        public void Stop()
        {
            // 标记为手动停止
            _isManualStop = true;
            
            try
            {
                if (_jobObject != null)
                {
                    // ===== Windows 10+ 路径：通过 JobObject 终止进程树 =====
                    _jobObject.Terminate(1);
                    _jobObject.Dispose();
                    _jobObject = null;
                }
                else if (_process != null && !_process.HasExited)
                {
                    // ===== Windows 7 路径：通过 taskkill 终止进程树 =====
                    KillProcessTree(_process.Id);
                }
            }
            catch (Exception ex)
            {
                OnOutputReceived($"停止进程失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 使用 taskkill /F /T 终止进程及其所有子进程（Windows 7 兼容方式）
        /// </summary>
        private void KillProcessTree(int pid)
        {
            try
            {
                var taskkill = new Process();
                taskkill.StartInfo.FileName = "taskkill";
                taskkill.StartInfo.Arguments = $"/F /T /PID {pid}";
                taskkill.StartInfo.CreateNoWindow = true;
                taskkill.StartInfo.UseShellExecute = false;
                taskkill.StartInfo.RedirectStandardOutput = true;
                taskkill.StartInfo.RedirectStandardError = true;
                taskkill.Start();
                taskkill.WaitForExit(5000);
            }
            catch (Exception ex)
            {
                OnOutputReceived($"taskkill 失败：{ex.Message}");
                
                // 最终兜底：直接 Kill 主进程
                try
                {
                    if (_process != null && !_process.HasExited)
                    {
                        _process.Kill();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 更新资源占用信息（CPU和内存）
        /// </summary>
        public void UpdateResourceUsage()
        {
            if (_app.Status != AppStatus.Running)
            {
                _app.CpuUsage = 0;
                _app.MemoryUsage = 0;
                _lastCpuTime = 0;
                return;
            }

            try
            {
                if (_jobObject != null)
                {
                    // ===== Windows 10+ 路径：通过 JobObject 获取进程树资源 =====
                    long currentCpuTime = _jobObject.GetCpuTime();
                    DateTime currentSampleTime = DateTime.Now;

                    if (_lastSampleTime != DateTime.MinValue && _lastCpuTime > 0)
                    {
                        long cpuTimeDelta = currentCpuTime - _lastCpuTime;
                        double timeDelta = (currentSampleTime - _lastSampleTime).TotalMilliseconds * 10000;

                        if (timeDelta > 0)
                        {
                            float usage = (float)((cpuTimeDelta / timeDelta) * 100.0 / Environment.ProcessorCount);
                            _app.CpuUsage = Math.Max(0, Math.Min(100, usage));
                        }
                    }

                    _lastCpuTime = currentCpuTime;
                    _lastSampleTime = currentSampleTime;

                    long totalMemory = 0;
                    var pids = _jobObject.GetProcessIds();
                    foreach (int pid in pids)
                    {
                        try
                        {
                            using (var p = Process.GetProcessById(pid))
                            {
                                totalMemory += p.WorkingSet64;
                            }
                        }
                        catch { /* 进程可能已退出 */ }
                    }
                    _app.MemoryUsage = totalMemory;
                }
                else if (_process != null && !_process.HasExited)
                {
                    // ===== Windows 7 路径：仅监控主进程 =====
                    DateTime currentSampleTime = DateTime.Now;
                    long currentCpuTime = (long)(_process.TotalProcessorTime.TotalMilliseconds * 10000);

                    if (_lastSampleTime != DateTime.MinValue && _lastCpuTime > 0)
                    {
                        long cpuTimeDelta = currentCpuTime - _lastCpuTime;
                        double timeDelta = (currentSampleTime - _lastSampleTime).TotalMilliseconds * 10000;

                        if (timeDelta > 0)
                        {
                            float usage = (float)((cpuTimeDelta / timeDelta) * 100.0 / Environment.ProcessorCount);
                            _app.CpuUsage = Math.Max(0, Math.Min(100, usage));
                        }
                    }

                    _lastCpuTime = currentCpuTime;
                    _lastSampleTime = currentSampleTime;
                    _app.MemoryUsage = _process.WorkingSet64;
                }
                else
                {
                    _app.CpuUsage = 0;
                    _app.MemoryUsage = 0;
                }
            }
            catch
            {
                _app.CpuUsage = 0;
                _app.MemoryUsage = 0;
            }
        }

        private void OnOutputReceived(string message) => OutputReceived?.Invoke(_app.Id, message);
        private void OnStatusChanged(AppStatus status)
        {
            _app.Status = status;
            StatusChanged?.Invoke(_app.Id, status);
        }

        public void Dispose()
        {
            _isManualStop = true;  // 防止 Dispose 时触发重启
            Stop();
            _process?.Dispose();
            _jobObject?.Dispose();
        }
    }
}
