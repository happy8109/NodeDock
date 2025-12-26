using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using NodeDock.Models;

namespace NodeDock.Services
{
    public class NodeProcessWorker : IDisposable
    {
        private Process _process;
        private readonly AppItem _app;
        private readonly string _nodeExePath;
        
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

                _process = new Process();
                _process.StartInfo.FileName = _nodeExePath;
                
                // 拼接参数: [Arguments] [EntryScript]
                string args = string.IsNullOrEmpty(_app.Arguments) ? "" : _app.Arguments + " ";
                _process.StartInfo.Arguments = args + $"\"{_app.EntryScript}\"";
                
                _process.StartInfo.WorkingDirectory = _app.WorkingDirectory;
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                _process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                // 绑定输出流
                _process.OutputDataReceived += (s, e) => { if (e.Data != null) OnOutputReceived(e.Data); };
                _process.ErrorDataReceived += (s, e) => { if (e.Data != null) OnOutputReceived("[ERROR] " + e.Data); };

                // 绑定退出事件
                _process.EnableRaisingEvents = true;
                _process.Exited += (s, e) => 
                {
                    _app.StartTime = null;
                    OnStatusChanged(AppStatus.Stopped);
                    OnOutputReceived("--- 程序已退出 ---");
                };

                if (_process.Start())
                {
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
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                    // 对于复杂的 Node 应用（如开启了子进程），可能需要更深层的 Process Tree Kill
                    // 但基础实现先使用 Kill
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
        }
    }
}
