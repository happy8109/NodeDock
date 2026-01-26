using System;
using System.Collections.Generic;
using System.Linq;
using NodeDock.Models;

namespace NodeDock.Services
{
    public class ManagerService
    {
        private static ManagerService _instance;
        public static ManagerService Instance => _instance ?? (_instance = new ManagerService());

        private readonly Dictionary<string, NodeProcessWorker> _workers = new Dictionary<string, NodeProcessWorker>();
        private readonly RuntimeScanner _scanner = new RuntimeScanner();
        private readonly System.Timers.Timer _monitoringTimer;

        public event Action<string, string> GlobalOutputReceived;
        public event Action<string, AppStatus> GlobalStatusChanged;
        public event Action GlobalResourceUpdated;

        private ManagerService() 
        {
            _monitoringTimer = new System.Timers.Timer(2000);
            _monitoringTimer.AutoReset = true;
            _monitoringTimer.Elapsed += (s, e) => UpdateAllResources();
            _monitoringTimer.Start();
        }

        private void UpdateAllResources()
        {
            foreach (var worker in _workers.Values)
            {
                worker.UpdateResourceUsage();
            }
            GlobalResourceUpdated?.Invoke();
        }

        /// <summary>
        /// 初始化所有 Worker
        /// </summary>
        public void Initialize()
        {
            var settings = ConfigService.Instance.Settings;
            var availableRuntimes = _scanner.Scan();

            foreach (var app in settings.AppList)
            {
                CreateWorker(app, availableRuntimes);
            }
            
            // 自动启动标记了 AutoStart 的应用
            foreach (var app in settings.AppList.Where(a => a.AutoStart))
            {
                StartApp(app.Id);
            }
        }

        private void CreateWorker(AppItem app, List<NodeRuntimeInfo> runtimes)
        {
            if (_workers.ContainsKey(app.Id)) return;

            // 寻找匹配的 Node 版本路径
            var runtime = runtimes.FirstOrDefault(r => r.Name == app.NodeVersion) 
                          ?? runtimes.FirstOrDefault(r => r.Name == ConfigService.Instance.Settings.DefaultNodeVersion)
                          ?? runtimes.FirstOrDefault();

            var worker = new NodeProcessWorker(app, runtime?.ExePath);
            worker.OutputReceived += (id, msg) => GlobalOutputReceived?.Invoke(id, msg);
            worker.StatusChanged += (id, status) => GlobalStatusChanged?.Invoke(id, status);

            _workers[app.Id] = worker;
        }

        public void StartApp(string appId)
        {
            if (_workers.TryGetValue(appId, out var worker))
            {
                worker.Start();
            }
        }

        public void StopApp(string appId)
        {
            if (_workers.TryGetValue(appId, out var worker))
            {
                worker.Stop();
            }
        }

        public void StartAll()
        {
            foreach (var worker in _workers.Values)
            {
                worker.Start();
            }
        }

        public void StopAll()
        {
            foreach (var worker in _workers.Values)
            {
                worker.Stop();
            }
        }

        public void RefreshWorkers()
        {
            // 在配置变更后调用，同步 AppList 与 Workers
            var settings = ConfigService.Instance.Settings;
            var runtimes = _scanner.Scan();

            // 停止并移除不再存在的 app
            var currentIds = settings.AppList.Select(a => a.Id).ToList();
            var idsToRemove = _workers.Keys.Where(id => !currentIds.Contains(id)).ToList();
            foreach (var id in idsToRemove)
            {
                _workers[id].Stop();
                _workers[id].Dispose();
                _workers.Remove(id);
            }

            // 添加新 app
            foreach (var app in settings.AppList)
            {
                if (!_workers.ContainsKey(app.Id))
                {
                    CreateWorker(app, runtimes);
                }
            }
        }

        public List<NodeRuntimeInfo> GetRuntimes() => _scanner.Scan();

        public void OpenTerminal(string appId)
        {
            var app = ConfigService.Instance.Settings.AppList.FirstOrDefault(a => a.Id == appId);
            if (app == null) return;

            var runtimes = _scanner.Scan();
            var runtime = runtimes.FirstOrDefault(r => r.Name == app.NodeVersion)
                          ?? runtimes.FirstOrDefault(r => r.Name == ConfigService.Instance.Settings.DefaultNodeVersion)
                          ?? runtimes.FirstOrDefault();

            if (runtime == null) return;

            string runtimeDir = System.IO.Path.GetDirectoryName(runtime.ExePath);
            string oldPath = Environment.GetEnvironmentVariable("PATH");

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = app.WorkingDirectory,
                UseShellExecute = false
            };
            // 注入环境路径
            startInfo.EnvironmentVariables["PATH"] = runtimeDir + ";" + oldPath;

            System.Diagnostics.Process.Start(startInfo);
        }
        
        /// <summary>
        /// 获取使用指定 Node.js 版本的应用列表
        /// </summary>
        /// <param name="versionName">版本名称</param>
        /// <returns>使用该版本的应用列表</returns>
        public List<AppItem> GetAppsUsingVersion(string versionName)
        {
            var settings = ConfigService.Instance.Settings;
            
            // 直接配置了该版本的应用
            var directUsers = settings.AppList
                .Where(a => a.NodeVersion == versionName)
                .ToList();
            
            // 如果该版本是默认版本，还需要检查未指定版本的应用
            if (settings.DefaultNodeVersion == versionName)
            {
                var defaultUsers = settings.AppList
                    .Where(a => string.IsNullOrEmpty(a.NodeVersion))
                    .ToList();
                directUsers.AddRange(defaultUsers);
            }
            
            return directUsers;
        }
    }
}
