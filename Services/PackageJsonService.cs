using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace NodeDock.Services
{
    /// <summary>
    /// 负责解析项目 package.json 文件，提取 Node.js 版本需求
    /// </summary>
    public class PackageJsonService
    {
        /// <summary>
        /// 从指定目录读取 package.json 并提取 engines.node 版本需求
        /// </summary>
        /// <param name="workingDirectory">项目工作目录</param>
        /// <returns>版本需求字符串，如 ">=18.0.0"，若无则返回 null</returns>
        public static string GetRequiredNodeVersion(string workingDirectory)
        {
            if (string.IsNullOrEmpty(workingDirectory)) return null;

            var packageJsonPath = Path.Combine(workingDirectory, "package.json");
            if (!File.Exists(packageJsonPath)) return null;

            try
            {
                var content = File.ReadAllText(packageJsonPath);
                var json = JObject.Parse(content);
                var engines = json["engines"] as JObject;
                if (engines == null) return null;

                var nodeVersion = engines["node"]?.ToString();
                return nodeVersion;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取系统环境中安装的 Node.js 版本
        /// </summary>
        /// <returns>版本号，如 "v20.10.0"，若未安装则返回 null</returns>
        public static string GetSystemNodeVersion()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "node",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    if (process == null) return null;
                    
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit(3000);
                    
                    // 输出格式为 "v20.10.0"
                    if (output.StartsWith("v") && output.Contains("."))
                    {
                        return output;
                    }
                }
            }
            catch
            {
                // Node.js 未安装或不在 PATH 中
            }
            
            return null;
        }

        /// <summary>
        /// 检查指定的 Node.js 版本是否满足版本需求
        /// </summary>
        /// <param name="nodeVersion">Node.js 版本，如 "v20.10.0"</param>
        /// <param name="requirement">版本需求，如 ">=18.0.0" 或 "^20.0.0"</param>
        /// <returns>是否满足需求</returns>
        public static bool IsVersionSatisfied(string nodeVersion, string requirement)
        {
            if (string.IsNullOrEmpty(nodeVersion) || string.IsNullOrEmpty(requirement))
                return false;

            try
            {
                // 移除版本号的 'v' 前缀
                var cleanVersion = nodeVersion.TrimStart('v');
                var version = ParseVersion(cleanVersion);
                if (version == null) return false;

                // 解析版本需求表达式 (支持多个条件用空格分隔)
                var conditions = requirement.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var condition in conditions)
                {
                    if (!EvaluateCondition(version, condition.Trim()))
                        return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从版本需求中提取推荐的主版本号
        /// </summary>
        public static int? GetRecommendedMajorVersion(string requirement)
        {
            if (string.IsNullOrEmpty(requirement)) return null;

            try
            {
                // 尝试匹配常见格式: ^18, ~18, >=18, 18.x, 18
                var match = Regex.Match(requirement, @"[\^~>=<]*(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int major))
                {
                    return major;
                }
            }
            catch { }
            
            return null;
        }

        private static Version ParseVersion(string versionString)
        {
            // 只取主版本号部分 (移除 -alpha, -beta 等后缀)
            var cleanVersion = Regex.Replace(versionString, @"-.*$", "");
            
            // 补全版本号为三段式
            var parts = cleanVersion.Split('.');
            int major = parts.Length > 0 && int.TryParse(parts[0], out int m) ? m : 0;
            int minor = parts.Length > 1 && int.TryParse(parts[1], out int n) ? n : 0;
            int patch = parts.Length > 2 && int.TryParse(parts[2], out int p) ? p : 0;

            return new Version(major, minor, patch);
        }

        private static bool EvaluateCondition(Version version, string condition)
        {
            if (string.IsNullOrEmpty(condition)) return true;

            // 处理 x 和 * 通配符: 18.x, 18.*, 18
            if (condition.Contains("x") || condition.Contains("*") || Regex.IsMatch(condition, @"^\d+$"))
            {
                var match = Regex.Match(condition, @"^(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int major))
                {
                    return version.Major == major;
                }
                return true;
            }

            // 解析操作符和版本号
            var opMatch = Regex.Match(condition, @"^(>=|<=|>|<|=|\^|~)?(.+)$");
            if (!opMatch.Success) return false;

            var op = opMatch.Groups[1].Value;
            var reqVersionStr = opMatch.Groups[2].Value.TrimStart('v');
            var reqVersion = ParseVersion(reqVersionStr);
            if (reqVersion == null) return false;

            switch (op)
            {
                case ">=":
                    return version >= reqVersion;
                case ">":
                    return version > reqVersion;
                case "<=":
                    return version <= reqVersion;
                case "<":
                    return version < reqVersion;
                case "=":
                case "":
                    // 精确匹配或无操作符时匹配主版本
                    if (string.IsNullOrEmpty(op) && reqVersionStr.Split('.').Length == 1)
                        return version.Major == reqVersion.Major;
                    return version == reqVersion;
                case "^":
                    // ^18.0.0 表示 >=18.0.0 且 <19.0.0
                    return version >= reqVersion && version.Major == reqVersion.Major;
                case "~":
                    // ~18.1.0 表示 >=18.1.0 且 <18.2.0
                    return version >= reqVersion && 
                           version.Major == reqVersion.Major && 
                           version.Minor == reqVersion.Minor;
                default:
                    return false;
            }
        }
    }
}
