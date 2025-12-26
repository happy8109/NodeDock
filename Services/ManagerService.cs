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

        public event Action<string, string> GlobalOutputReceived;
        public event Action<string, AppStatus> GlobalStatusChanged;

        private ManagerService() { }

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
    }
}
