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
                        txtWorkDir.Text = Path.GetDirectoryName(ofd.FileName);
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
            using (var form = new DownloadVersionForm())
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
