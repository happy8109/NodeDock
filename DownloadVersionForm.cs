using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NodeDock.Models;
using NodeDock.Services;

namespace NodeDock
{
    public partial class DownloadVersionForm : Form
    {
        private readonly NodeDownloadService _downloadService = new NodeDownloadService();
        private List<RemoteNodeVersion> _allVersions = new List<RemoteNodeVersion>();
        private bool _isDownloading = false;
        
        /// <summary>
        /// 项目要求的 Node.js 版本需求（来自 package.json 的 engines.node）
        /// </summary>
        private readonly string _requiredVersion;

        public string DownloadedVersion { get; private set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public DownloadVersionForm() : this(null)
        {
        }

        /// <summary>
        /// 带版本需求的构造函数
        /// </summary>
        /// <param name="requiredVersion">项目要求的 Node.js 版本，如 ">=18.0.0"</param>
        public DownloadVersionForm(string requiredVersion)
        {
            InitializeComponent();
            _requiredVersion = requiredVersion;
        }

        private async void DownloadVersionForm_Load(object sender, EventArgs e)
        {
            // 显示版本需求信息
            if (!string.IsNullOrEmpty(_requiredVersion))
            {
                lblStatus.Text = $"正在获取版本列表... (项目要求: {_requiredVersion})";
            }
            else
            {
                lblStatus.Text = "正在获取可用版本列表...";
            }
            
            try
            {
                _allVersions = await _downloadService.GetAvailableVersionsAsync();
                
                // 标记系统环境版本
                MarkSystemVersion();
                
                // 标记推荐版本
                MarkRecommendedVersions();
                
                UpdateList("");
                
                if (!string.IsNullOrEmpty(_requiredVersion))
                {
                    var recommendedCount = _allVersions.Count(v => v.IsRecommended && v.IsLts);
                    lblStatus.Text = $"已找到 {recommendedCount} 个符合项目要求的推荐版本";
                }
                else
                {
                    lblStatus.Text = "列表拉取成功";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "列表获取失败";
            }
        }

        /// <summary>
        /// 标记系统环境中安装的 Node.js 版本
        /// </summary>
        private void MarkSystemVersion()
        {
            var systemVersion = PackageJsonService.GetSystemNodeVersion();
            if (string.IsNullOrEmpty(systemVersion)) return;

            foreach (var version in _allVersions)
            {
                if (version.Version == systemVersion)
                {
                    version.IsSystemVersion = true;
                    break;
                }
            }
        }


        /// <summary>
        /// 根据项目版本需求标记推荐版本
        /// 策略：只推荐满足条件的版本中，每个主版本的最新版本
        /// </summary>
        private void MarkRecommendedVersions()
        {
            if (string.IsNullOrEmpty(_requiredVersion)) return;

            // 先找出所有满足条件的 LTS 版本
            var satisfiedVersions = _allVersions
                .Where(v => v.IsLts && PackageJsonService.IsVersionSatisfied(v.Version, _requiredVersion))
                .ToList();

            if (satisfiedVersions.Count == 0) return;

            // 按主版本号分组，每组只取最新的一个
            var recommendedByMajor = satisfiedVersions
                .GroupBy(v => GetMajorVersion(v.Version))
                .Select(g => g.OrderByDescending(v => v.Version).First())
                .ToList();

            // 标记推荐版本
            foreach (var version in recommendedByMajor)
            {
                version.IsRecommended = true;
            }
        }

        /// <summary>
        /// 从版本号中提取主版本号
        /// </summary>
        private int GetMajorVersion(string version)
        {
            var cleanVersion = version.TrimStart('v');
            var parts = cleanVersion.Split('.');
            if (parts.Length > 0 && int.TryParse(parts[0], out int major))
            {
                return major;
            }
            return 0;
        }


        private void UpdateList(string filter)
        {
            lstVersions.Items.Clear();
            
            var filtered = _allVersions
                .Where(v => v.IsLts && (string.IsNullOrEmpty(filter) || v.Version.Contains(filter)))
                .OrderByDescending(v => v.IsRecommended) // 推荐版本优先
                .ThenByDescending(v => v.Version)
                .ToList();

            foreach (var v in filtered)
            {
                lstVersions.Items.Add(v);
            }
            
            // 自动选中第一个推荐版本，若无则选中第一个
            var firstRecommended = filtered.FirstOrDefault(v => v.IsRecommended);
            if (firstRecommended != null)
            {
                lstVersions.SelectedItem = firstRecommended;
            }
            else if (lstVersions.Items.Count > 0)
            {
                lstVersions.SelectedIndex = 0;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            UpdateList(txtSearch.Text);
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (lstVersions.SelectedItem is RemoteNodeVersion version)
            {
                await StartDownload(version.Version);
            }
        }

        private async System.Threading.Tasks.Task StartDownload(string version)
        {
            SetUIState(true);
            try
            {
                lblStatus.Text = $"正在下载 {version}...";
                progressBar.Value = 0;
                
                await _downloadService.DownloadAndExtractAsync(version, (progress) => {
                    this.Invoke(new Action(() => {
                        progressBar.Value = progress;
                        lblStatus.Text = $"正在下载 {version}: {progress}%";
                    }));
                });

                DownloadedVersion = version;
                lblStatus.Text = "下载并安装成功！";
                MessageBox.Show($"{version} 已成功安装到 runtimes 目录。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "下载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "安装失败";
            }
            finally
            {
                SetUIState(false);
            }
        }

        private void SetUIState(bool isDownloading)
        {
            _isDownloading = isDownloading;
            btnDownload.Enabled = !isDownloading;
            lstVersions.Enabled = !isDownloading;
            txtSearch.Enabled = !isDownloading;
            progressBar.Visible = isDownloading;
            btnCancel.Text = isDownloading ? "后台运行" : "取消";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_isDownloading)
            {
                this.Hide(); // 这里其实并没有真正的取消逻辑，只是隐藏窗口
                return;
            }
            this.Close();
        }
    }
}

