using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace NodeDock.Utils
{
    public static class PortDetector
    {
        /// <summary>
        /// 探测指定目录下的 Node.js 应用可能使用的端口
        /// </summary>
        public static List<int> DetectPorts(string workingDirectory)
        {
            var ports = new HashSet<int>();
            
            if (string.IsNullOrEmpty(workingDirectory) || !Directory.Exists(workingDirectory))
                return new List<int>();

            // 1. 扫描环境变量文件 (.env)
            DetectFromEnv(workingDirectory, ports);

            // 2. 递归扫描所有配置文件 (.json, .yml, .yaml)
            // 排除 node_modules 目录
            DetectFromConfigFiles(workingDirectory, ports);

            // 3. 扫描 package.json (scripts)
            DetectFromPackageJson(workingDirectory, ports);

            // 4. 扫描源码文件中的特征 (如 .listen(3000))
            DetectFromCode(workingDirectory, ports);

            var result = new List<int>(ports);
            result.Sort();
            return result;
        }

        private static void DetectFromEnv(string dir, HashSet<int> ports)
        {
            string envPath = Path.Combine(dir, ".env");
            if (File.Exists(envPath))
            {
                try
                {
                    var lines = File.ReadAllLines(envPath);
                    foreach (var line in lines)
                    {
                        // 匹配 PORT=3000, SERVER_PORT=3000 等
                        var match = Regex.Match(line, @"^\s*(?:PORT|SERVER_PORT|APP_PORT|WEB_PORT)\s*=\s*(?<port>\d+)", RegexOptions.IgnoreCase);
                        if (match.Success && int.TryParse(match.Groups["port"].Value, out int p))
                        {
                            if (p > 0 && p < 65536) ports.Add(p);
                        }
                    }
                }
                catch { }
            }
        }

        private static void DetectFromConfigFiles(string dir, HashSet<int> ports)
        {
            try
            {
                // 获取所有可能的配置文件
                var files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    // 排除 node_modules 和 .git
                    if (file.Contains("\\node_modules\\") || file.Contains("\\.git\\"))
                        continue;

                    string ext = Path.GetExtension(file).ToLower();
                    if (ext == ".json" || ext == ".yml" || ext == ".yaml")
                    {
                        // 排除 package.json (在单独方法处理) 和 package-lock.json
                        string filename = Path.GetFileName(file).ToLower();
                        if (filename == "package.json" || filename == "package-lock.json")
                            continue;

                        try
                        {
                            string content = File.ReadAllText(file);
                            // 通用启发式正则：匹配包含 "port" 的键且值为数字
                            // 兼容 JSON: "port": 3000
                            // 兼容 YAML: port: 3000
                            var matches = Regex.Matches(content, @"(?:""?[\w-]*port\w*""?)\s*[:=]\s*(?<port>\d+)", RegexOptions.IgnoreCase);
                            foreach (Match match in matches)
                            {
                                if (int.TryParse(match.Groups["port"].Value, out int p))
                                {
                                    if (p > 0 && p < 65536) ports.Add(p);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private static void DetectFromPackageJson(string dir, HashSet<int> ports)
        {
            string pkgPath = Path.Combine(dir, "package.json");
            if (File.Exists(pkgPath))
            {
                try
                {
                    string content = File.ReadAllText(pkgPath);
                    // 1. 匹配 --port 3000 或 PORT=3000 在脚本中
                    var matches = Regex.Matches(content, @"(?:--port|PORT=)\s*(?<port>\d+)", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        if (int.TryParse(match.Groups["port"].Value, out int p))
                        {
                            if (p > 0 && p < 65536) ports.Add(p);
                        }
                    }

                    // 2. 匹配 JSON 中的端口字段 (针对某些直接在 package.json 里写配置的应用)
                    var jsonMatches = Regex.Matches(content, @"""[\w-]*port\w*""\s*:\s*(?<port>\d+)", RegexOptions.IgnoreCase);
                    foreach (Match match in jsonMatches)
                    {
                        if (int.TryParse(match.Groups["port"].Value, out int p))
                        {
                            if (p > 0 && p < 65536) ports.Add(p);
                        }
                    }
                }
                catch { }
            }
        }

        private static void DetectFromCode(string dir, HashSet<int> ports)
        {
            // 扫描常见入口文件及其扩展
            string[] extensions = { ".js", ".ts", ".mjs", ".cjs" };
            string[] entryFiles = { "index", "app", "main", "server", "start" };

            try
            {
                foreach (var entryBase in entryFiles)
                {
                    foreach (var ext in extensions)
                    {
                        string path = Path.Combine(dir, entryBase + ext);
                        if (File.Exists(path))
                        {
                            ScanCodeFile(path, ports);
                        }
                    }
                }
                
                // 也可以扫描根目录下的所有脚本文件，但为了性能暂时只扫常见入口
            }
            catch { }
        }

        private static void ScanCodeFile(string path, HashSet<int> ports)
        {
            try
            {
                string content = File.ReadAllText(path);
                // 1. 匹配 .listen(3000)
                var matches = Regex.Matches(content, @".listen\(\s*(?<port>\d+)", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (int.TryParse(match.Groups["port"].Value, out int p))
                    {
                        if (p > 0 && p < 65536) ports.Add(p);
                    }
                }

                // 2. 匹配端口常量定义 const port = 3000;
                var constMatches = Regex.Matches(content, @"(?:const|let|var)\s+\w*port\w*\s*=\s*(?<port>\d+)", RegexOptions.IgnoreCase);
                foreach (Match match in constMatches)
                {
                    if (int.TryParse(match.Groups["port"].Value, out int p))
                    {
                        if (p > 0 && p < 65536) ports.Add(p);
                    }
                }
            }
            catch { }
        }
    }
}
