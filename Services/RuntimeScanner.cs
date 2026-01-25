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
        
        /// <summary>
        /// 运行时目录的磁盘占用大小（字节）
        /// </summary>
        public long SizeBytes { get; set; }
        
        /// <summary>
        /// 是否为默认版本
        /// </summary>
        public bool IsDefault { get; set; }
        
        /// <summary>
        /// 格式化的大小显示
        /// </summary>
        public string SizeDisplay
        {
            get
            {
                if (SizeBytes < 1024) return $"{SizeBytes} B";
                if (SizeBytes < 1024 * 1024) return $"{SizeBytes / 1024.0:F1} KB";
                if (SizeBytes < 1024 * 1024 * 1024) return $"{SizeBytes / (1024.0 * 1024):F1} MB";
                return $"{SizeBytes / (1024.0 * 1024 * 1024):F2} GB";
            }
        }

        public override string ToString()
        {
            return $"{Name}        {SizeDisplay}";
        }
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
        /// 获取 runtimes 目录路径
        /// </summary>
        public string RuntimesDirectory => _runtimesDir;

        /// <summary>
        /// 扫描 runtimes 目录下的所有 Node 版本
        /// </summary>
        public List<NodeRuntimeInfo> Scan()
        {
            var results = new List<NodeRuntimeInfo>();

            if (!Directory.Exists(_runtimesDir)) return results;
            
            var defaultVersion = ConfigService.Instance.Settings.DefaultNodeVersion;

            var subDirs = Directory.GetDirectories(_runtimesDir);
            foreach (var dir in subDirs)
            {
                string nodeExe = Path.Combine(dir, "node.exe");
                if (File.Exists(nodeExe))
                {
                    var name = Path.GetFileName(dir);
                    results.Add(new NodeRuntimeInfo
                    {
                        Name = name,
                        ExePath = nodeExe,
                        SizeBytes = CalculateDirectorySize(dir),
                        IsDefault = name == defaultVersion
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
        
        /// <summary>
        /// 删除指定版本的 Node.js 运行时
        /// </summary>
        /// <param name="versionName">版本目录名称</param>
        /// <returns>是否成功删除</returns>
        public bool DeleteVersion(string versionName)
        {
            if (string.IsNullOrEmpty(versionName)) return false;
            
            string versionDir = Path.Combine(_runtimesDir, versionName);
            if (!Directory.Exists(versionDir)) return false;
            
            try
            {
                Directory.Delete(versionDir, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// 计算目录的总大小
        /// </summary>
        private long CalculateDirectorySize(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return 0;
            
            long size = 0;
            try
            {
                // 递归计算所有文件大小
                foreach (var file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        size += new FileInfo(file).Length;
                    }
                    catch { /* 忽略无法访问的文件 */ }
                }
            }
            catch { /* 忽略无法访问的目录 */ }
            
            return size;
        }
    }
}
