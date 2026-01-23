using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        
        // 事件定义
        public event Action<string, string> OutputReceived; // (appId, message)
        public event Action<string, AppStatus> StatusChanged; // (appId, status)

        public NodeProcessWorker(AppItem app, string nodeExePath)
        {
            _app = app;
            _nodeExePath = nodeExePath;
        }

        public void Start()
        {
            if (_process != null && !_process.HasExited) return;

            if (string.IsNullOrEmpty(_nodeExePath) || !File.Exists(_nodeExePath))
            {
                OnStatusChanged(AppStatus.Error);
                OnOutputReceived($"错误：未找到 Node 执行程序路径 \"{_nodeExePath}\"");
                return;
            }

            try
            {
                OnStatusChanged(AppStatus.Starting);

                // 创建作业对象以实现管理
                _jobObject?.Dispose();
                _jobObject = new JobObject();

                _process = new Process();
                string nodeDir = Path.GetDirectoryName(_nodeExePath);
                
                // 智能识别启动类型
                string entry = _app.EntryScript?.Trim() ?? "";
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
                    _process.StartInfo.Arguments = args + $"\"{_app.EntryScript}\"";
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
                _process.Exited += (s, e) => 
                {
                    _app.StartTime = null;
                    if (_app.Status != AppStatus.Error) OnStatusChanged(AppStatus.Stopped);
                    OnOutputReceived("--- 程序已退出 ---");
                };

                if (_process.Start())
                {
                    // 将进程添加到作业对象中
                    try
                    {
                        _jobObject.AddProcess(_process.Handle);
                    }
                    catch (Exception ex)
                    {
                        OnOutputReceived($"警告：无法建立 Job 绑定 ({ex.Message})，子进程清理可能失效。");
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

        public void Stop()
        {
            try
            {
                if (_jobObject != null)
                {
                    // 释放 JobObject 会导致所有关联进程（包括子进程）被 Windows 自动终止
                    _jobObject.Dispose();
                    _jobObject = null;
                }
                else if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                }
            }
            catch (Exception ex)
            {
                OnOutputReceived($"停止进程失败：{ex.Message}");
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
            Stop();
            _process?.Dispose();
            _jobObject?.Dispose();
        }
    }
}

