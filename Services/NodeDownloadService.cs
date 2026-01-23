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
        private const string VersionListUrl = "https://nodejs.org/dist/index.json";
        private const string DownloadUrlTemplate = "https://nodejs.org/dist/{0}/node-{0}-win-x64.zip";

        public async Task<List<RemoteNodeVersion>> GetAvailableVersionsAsync()
        {
            try
            {
                string json = await _httpClient.GetStringAsync(VersionListUrl);
                var versions = JsonConvert.DeserializeObject<List<RemoteNodeVersion>>(json);
                return versions.Where(v => v.Files.Contains("win-x64-zip")).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("获取 Node.js 版本列表失败: " + ex.Message);
            }
        }

        public async Task DownloadAndExtractAsync(string version, Action<int> onProgress)
        {
            if (!version.StartsWith("v")) version = "v" + version;
            string url = string.Format(DownloadUrlTemplate, version);
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

                // 2. 解压
                if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
                
                string extractTempPath = Path.Combine(runtimesDir, $"temp_{version}");
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
