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

        public string DownloadedVersion { get; private set; }

        public DownloadVersionForm()
        {
            InitializeComponent();
        }

        private async void DownloadVersionForm_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "正在获取可用版本列表...";
            try
            {
                _allVersions = await _downloadService.GetAvailableVersionsAsync();
                UpdateList("");
                lblStatus.Text = "列表拉取成功";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "列表获取失败";
            }
        }

        private void UpdateList(string filter)
        {
            lstVersions.Items.Clear();
            var filtered = _allVersions
                .Where(v => v.IsLts && (string.IsNullOrEmpty(filter) || v.Version.Contains(filter)))
                .OrderByDescending(v => v.Version)
                .ToList();

            foreach (var v in filtered)
            {
                lstVersions.Items.Add(v);
            }
            
            if (lstVersions.Items.Count > 0) lstVersions.SelectedIndex = 0;
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
