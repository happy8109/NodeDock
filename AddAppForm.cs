using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NodeDock.Models;
using NodeDock.Services;

namespace NodeDock
{
    public partial class AddAppForm : Form
    {
        public AppItem AppInfo { get; private set; }
        
        /// <summary>
        /// 从 package.json 探测到的 Node.js 版本需求
        /// </summary>
        private string _detectedNodeVersion;


        public AddAppForm(AppItem app = null)
        {
            InitializeComponent();
            PopulateRuntimes();

            if (app != null)
            {
                AppInfo = app;
                Text = "编辑应用 - " + app.Name;
                txtName.Text = app.Name;
                txtWorkDir.Text = app.WorkingDirectory;
                txtScript.Text = app.EntryScript;
                txtArgs.Text = app.Arguments;
                chkAutoStart.Checked = app.AutoStart;
                cmbVersion.SelectedItem = cmbVersion.Items.Cast<NodeRuntimeInfo>().FirstOrDefault(r => r.Name == app.NodeVersion);
                
                // 编辑模式下也探测版本需求
                DetectNodeVersion(app.WorkingDirectory);
            }
            else
            {
                AppInfo = new AppItem();
                Text = "添加新应用";
            }
        }

        private void PopulateRuntimes()
        {
            var runtimes = ManagerService.Instance.GetRuntimes();
            cmbVersion.DisplayMember = "Name";
            foreach (var r in runtimes)
            {
                cmbVersion.Items.Add(r);
            }
            if (cmbVersion.Items.Count > 0) cmbVersion.SelectedIndex = 0;
        }

        /// <summary>
        /// 探测工作目录下 package.json 中的 Node.js 版本需求
        /// </summary>
        private void DetectNodeVersion(string workingDirectory)
        {
            _detectedNodeVersion = PackageJsonService.GetRequiredNodeVersion(workingDirectory);
            
            if (!string.IsNullOrEmpty(_detectedNodeVersion))
            {
                // 更新下载按钮提示
                btnDownloadNode.Text = "下载 ⭐";
                toolTip.SetToolTip(btnDownloadNode, $"检测到项目需要: {_detectedNodeVersion}");
                
                // 尝试自动选择匹配的已安装版本
                AutoSelectMatchingVersion();
            }
            else
            {
                btnDownloadNode.Text = "下载...";
                toolTip.SetToolTip(btnDownloadNode, "下载新的 Node.js 版本");
            }
        }

        /// <summary>
        /// 自动选择符合版本需求的已安装 Node 版本
        /// </summary>
        private void AutoSelectMatchingVersion()
        {
            if (string.IsNullOrEmpty(_detectedNodeVersion)) return;

            foreach (var item in cmbVersion.Items)
            {
                if (item is NodeRuntimeInfo runtime)
                {
                    // 检查运行时版本是否满足需求
                    if (PackageJsonService.IsVersionSatisfied(runtime.Name, _detectedNodeVersion))
                    {
                        cmbVersion.SelectedItem = runtime;
                        return;
                    }
                }
            }
        }

        private void btnBrowseDir_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtWorkDir.Text = fbd.SelectedPath;
                    if (string.IsNullOrEmpty(txtName.Text))
                    {
                        txtName.Text = Path.GetFileName(fbd.SelectedPath);
                    }
                    
                    // 探测版本需求
                    DetectNodeVersion(fbd.SelectedPath);
                }
            }
        }

        private void btnBrowseScript_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "JavaScript Files (*.js)|*.js|All Files (*.*)|*.*";
                if (!string.IsNullOrEmpty(txtWorkDir.Text)) ofd.InitialDirectory = txtWorkDir.Text;
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtScript.Text = ofd.FileName;
                    if (string.IsNullOrEmpty(txtWorkDir.Text))
                    {
                        var dir = Path.GetDirectoryName(ofd.FileName);
                        txtWorkDir.Text = dir;
                        
                        // 探测版本需求
                        DetectNodeVersion(dir);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtWorkDir.Text) || string.IsNullOrEmpty(txtScript.Text))
            {
                MessageBox.Show("请完整填写必要信息。");
                return;
            }

            AppInfo.Name = txtName.Text;
            AppInfo.WorkingDirectory = txtWorkDir.Text;
            AppInfo.EntryScript = txtScript.Text;
            AppInfo.Arguments = txtArgs.Text;
            AppInfo.AutoStart = chkAutoStart.Checked;
            AppInfo.NodeVersion = (cmbVersion.SelectedItem as NodeRuntimeInfo)?.Name;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnDownloadNode_Click(object sender, EventArgs e)
        {
            // 传递探测到的版本需求给下载表单
            using (var form = new DownloadVersionForm(_detectedNodeVersion))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    PopulateRuntimes();
                    // 自动选中刚下载的版本
                    if (!string.IsNullOrEmpty(form.DownloadedVersion))
                    {
                        cmbVersion.SelectedItem = cmbVersion.Items.Cast<NodeRuntimeInfo>()
                            .FirstOrDefault(r => r.Name == form.DownloadedVersion);
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

