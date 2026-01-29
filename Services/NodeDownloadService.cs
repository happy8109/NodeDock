using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodeDock.Models;

namespace NodeDock.Services
{
    public class NodeDownloadService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        /// <summary>
        /// 获取当前配置的镜像源基础 URL
        /// </summary>
        private string BaseUrl => ConfigService.Instance.Settings.MirrorSource ?? MirrorSources.Official;
        
        /// <summary>
        /// 版本列表 URL (追加 index.json)
        /// </summary>
        private string VersionListUrl => BaseUrl.TrimEnd('/') + "/index.json";
        
        /// <summary>
        /// 下载 URL 模板 ({0} 为版本号，如 v20.0.0)
        /// </summary>
        private string GetDownloadUrl(string version) => 
            $"{BaseUrl.TrimEnd('/')}/{version}/node-{version}-win-x64.zip";

        public async Task<List<RemoteNodeVersion>> GetAvailableVersionsAsync()
        {
            try
            {
                string json = await _httpClient.GetStringAsync(VersionListUrl);
                var versions = JsonConvert.DeserializeObject<List<RemoteNodeVersion>>(json);
                return versions.Where(v => 
                    v.Files.Contains("win-x64-zip") && 
                    (
                        (v.Lts != null && GetMajorVersion(v.Version) >= 16) || // v16+ 的 LTS 版本
                        v.Version == "v13.14.0" ||                          // 或者是 v13.14.0 (Win7 最后支持版本)
                        v.Version == "v18.16.1"                             // 或者是 v18.16.1 (用户要求)
                    )
                ).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("获取 Node.js 版本列表失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 从版本字符串提取主版本号，如 "v16.20.0" -> 16
        /// </summary>
        private int GetMajorVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return 0;
            version = version.TrimStart('v');
            var parts = version.Split('.');
            if (parts.Length > 0 && int.TryParse(parts[0], out int major))
            {
                return major;
            }
            return 0;
        }

        public async Task DownloadAndExtractAsync(string version, Action<int> onProgress)
        {
            if (!version.StartsWith("v")) version = "v" + version;
            string url = GetDownloadUrl(version);
            string runtimesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes");
            string targetDir = Path.Combine(runtimesDir, version);
            string tempZipPath = Path.Combine(runtimesDir, $"{version}.zip");

            if (!Directory.Exists(runtimesDir)) Directory.CreateDirectory(runtimesDir);

            try
            {
                // 1. 下载
                using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        var totalRead = 0L;
                        int read;

                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;
                            if (canReportProgress)
                            {
                                int progress = (int)((double)totalRead / totalBytes * 100);
                                onProgress?.Invoke(progress);
                            }
                        }
                    }
                }

                // 2. 解压 - 在runtimes目录下创建临时目录，避免跨卷移动问题
                // Directory.Move 无法在不同磁盘分区之间移动，必须确保临时目录和目标目录在同一分区
                if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
                
                // 使用runtimes目录下的临时文件夹，确保与目标在同一分区
                string extractTempPath = Path.Combine(runtimesDir, $"_temp_{Guid.NewGuid():N}".Substring(0, 16));
                if (Directory.Exists(extractTempPath)) Directory.Delete(extractTempPath, true);

                ZipFile.ExtractToDirectory(tempZipPath, extractTempPath);

                // node-vXX.XX.XX-win-x64 目录下的内容才是我们要的
                string innerDir = Directory.GetDirectories(extractTempPath).FirstOrDefault();
                if (innerDir != null)
                {
                    Directory.Move(innerDir, targetDir);
                }
                
                // 清理
                if (Directory.Exists(extractTempPath)) Directory.Delete(extractTempPath, true);
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
                throw new Exception($"下载或安装 Node.js {version} 失败: {ex.Message}");
            }
        }
    }
}
