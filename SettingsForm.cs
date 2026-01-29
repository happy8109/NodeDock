using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using NodeDock.Models;
using NodeDock.Services;

namespace NodeDock
{
    public partial class SettingsForm : Form
    {
        private readonly RuntimeScanner _scanner = new RuntimeScanner();
        private const string AutoStartRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "NodeDock";
        
        // 用于防止初始化时触发事件
        private bool _isLoading = true;

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            
            LoadVersionList();
            UpdateDefaultLabel();
            LoadMirrorSetting();
            LoadAutoStartSetting();
            LoadWin7CompatibilitySetting();
            
            _isLoading = false;
        }

        /// <summary>
        /// 加载已安装版本列表
        /// </summary>
        private void LoadVersionList()
        {
            lstVersions.Items.Clear();
            
            var runtimes = _scanner.Scan();
            
            if (runtimes.Count == 0)
            {
                lstVersions.Items.Add("(暂未安装任何版本)");
                btnSetDefault.Enabled = false;
                btnDelete.Enabled = false;
                return;
            }
            
            btnSetDefault.Enabled = true;
            btnDelete.Enabled = true;
            
            foreach (var runtime in runtimes)
            {
                lstVersions.Items.Add(runtime);
            }
            
            // 默认选中第一项
            if (lstVersions.Items.Count > 0)
            {
                lstVersions.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 更新默认版本标签
        /// </summary>
        private void UpdateDefaultLabel()
        {
            var defaultVersion = ConfigService.Instance.Settings.DefaultNodeVersion;
            
            if (string.IsNullOrEmpty(defaultVersion))
            {
                lblDefault.Text = "当前默认版本: 未设置";
            }
            else
            {
                lblDefault.Text = $"当前默认版本: {defaultVersion}";
            }
        }
        
        /// <summary>
        /// 加载镜像源设置
        /// </summary>
        private void LoadMirrorSetting()
        {
            var currentMirror = ConfigService.Instance.Settings.MirrorSource ?? MirrorSources.Official;
            
            if (currentMirror == MirrorSources.Taobao)
            {
                rbMirrorTaobao.Checked = true;
            }
            else
            {
                rbMirrorOfficial.Checked = true;
            }
        }
        
        /// <summary>
        /// 加载开机自启动设置
        /// </summary>
        private void LoadAutoStartSetting()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(AutoStartRegistryKey, false))
                {
                    var value = key?.GetValue(AppName);
                    chkAutoStart.Checked = value != null;
                }
            }
            catch
            {
                chkAutoStart.Checked = false;
            }
        }

        /// <summary>
        /// 加载 Windows 7 兼容性设置
        /// </summary>
        private void LoadWin7CompatibilitySetting()
        {
            var os = Environment.OSVersion.Version;
            // Windows 7 版本号为 6.1
            bool isWin7OrLower = os.Major < 6 || (os.Major == 6 && os.Minor <= 1);

            if (!isWin7OrLower)
            {
                chkWin7Compatibility.Enabled = false;
                chkWin7Compatibility.Checked = false;
                //chkWin7Compatibility.Text += " (当前系统无需开启)";
                
                // 如果之前强行开启了，也关掉并保存
                if (ConfigService.Instance.Settings.EnableNodeWin7Compatibility)
                {
                    ConfigService.Instance.Settings.EnableNodeWin7Compatibility = false;
                    ConfigService.Instance.Save();
                }
            }
            else
            {
                chkWin7Compatibility.Checked = ConfigService.Instance.Settings.EnableNodeWin7Compatibility;
            }
        }

        /// <summary>
        /// 设为默认按钮点击
        /// </summary>
        private void btnSetDefault_Click(object sender, EventArgs e)
        {
            if (lstVersions.SelectedItem is NodeRuntimeInfo runtime)
            {
                ConfigService.Instance.Settings.DefaultNodeVersion = runtime.Name;
                ConfigService.Instance.Save();
                
                LoadVersionList();
                UpdateDefaultLabel();
                
                MessageBox.Show($"已将 {runtime.Name} 设为默认版本。", "设置成功", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 删除版本按钮点击
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!(lstVersions.SelectedItem is NodeRuntimeInfo runtime))
            {
                return;
            }
            
            // 检查是否有应用正在使用该版本
            var appsUsing = ManagerService.Instance.GetAppsUsingVersion(runtime.Name);
            
            if (appsUsing.Count > 0)
            {
                var appNames = string.Join("\n", appsUsing.Select(a => $"  • {a.Name}"));
                var result = MessageBox.Show(
                    $"以下应用正在使用 {runtime.Name}：\n\n{appNames}\n\n" +
                    "删除后这些应用可能无法正常运行。是否继续删除？",
                    "警告",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }
            else
            {
                var result = MessageBox.Show(
                    $"确定要删除 {runtime.Name} ({runtime.SizeDisplay}) 吗？\n此操作无法撤销。",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }
            
            // 执行删除
            bool success = _scanner.DeleteVersion(runtime.Name);
            
            if (success)
            {
                // 如果删除的是默认版本，清空默认设置
                if (ConfigService.Instance.Settings.DefaultNodeVersion == runtime.Name)
                {
                    ConfigService.Instance.Settings.DefaultNodeVersion = null;
                    ConfigService.Instance.Save();
                }
                
                LoadVersionList();
                UpdateDefaultLabel();
                
                MessageBox.Show($"{runtime.Name} 已删除。", "删除成功", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    $"删除 {runtime.Name} 失败。\n可能有程序正在使用该目录，请确保所有相关进程已关闭后重试。",
                    "删除失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 镜像源选择变更
        /// </summary>
        private void rbMirror_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            
            if (rbMirrorTaobao.Checked)
            {
                ConfigService.Instance.Settings.MirrorSource = MirrorSources.Taobao;
            }
            else
            {
                ConfigService.Instance.Settings.MirrorSource = MirrorSources.Official;
            }
            
            ConfigService.Instance.Save();
        }
        
        /// <summary>
        /// 开机自启动选项变更
        /// </summary>
        private void chkAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(AutoStartRegistryKey, true))
                {
                    if (key == null) return;
                    
                    if (chkAutoStart.Checked)
                    {
                        // 添加到启动项
                        string exePath = Application.ExecutablePath;
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                    else
                    {
                        // 从启动项移除
                        key.DeleteValue(AppName, false);
                    }
                }
                
                // 同步到配置
                ConfigService.Instance.Settings.StartAtLogin = chkAutoStart.Checked;
                ConfigService.Instance.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机启动失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 恢复复选框状态
                _isLoading = true;
                chkAutoStart.Checked = !chkAutoStart.Checked;
                _isLoading = false;
            }
        }

        /// <summary>
        /// Windows 7 兼容性选项变更
        /// </summary>
        private void chkWin7Compatibility_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ConfigService.Instance.Settings.EnableNodeWin7Compatibility = chkWin7Compatibility.Checked;
            ConfigService.Instance.Save();
        }
        
        /// <summary>
        /// 下载新版本按钮点击
        /// </summary>
        private void btnDownload_Click(object sender, EventArgs e)
        {
            using (var form = new DownloadVersionForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // 下载成功后刷新版本列表
                    LoadVersionList();
                    UpdateDefaultLabel();
                }
            }
        }

        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
