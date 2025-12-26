using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NodeDock.Services
{
    public class NodeRuntimeInfo
    {
        public string Name { get; set; }
        public string ExePath { get; set; }
        public bool IsValid => File.Exists(ExePath);
    }

    public class RuntimeScanner
    {
        private readonly string _runtimesDir;

        public RuntimeScanner()
        {
            _runtimesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes");
            if (!Directory.Exists(_runtimesDir)) Directory.CreateDirectory(_runtimesDir);
        }

        /// <summary>
        /// 扫描 runtimes 目录下的所有 Node 版本
        /// </summary>
        public List<NodeRuntimeInfo> Scan()
        {
            var results = new List<NodeRuntimeInfo>();

            if (!Directory.Exists(_runtimesDir)) return results;

            var subDirs = Directory.GetDirectories(_runtimesDir);
            foreach (var dir in subDirs)
            {
                string nodeExe = Path.Combine(dir, "node.exe");
                if (File.Exists(nodeExe))
                {
                    results.Add(new NodeRuntimeInfo
                    {
                        Name = Path.GetFileName(dir),
                        ExePath = nodeExe
                    });
                }
            }

            return results.OrderByDescending(r => r.Name).ToList();
        }

        /// <summary>
        /// 根据名称获取特定的 Node 路径
        /// </summary>
        public string GetExePath(string versionName)
        {
            if (string.IsNullOrEmpty(versionName)) return null;
            string path = Path.Combine(_runtimesDir, versionName, "node.exe");
            return File.Exists(path) ? path : null;
        }
    }
}
