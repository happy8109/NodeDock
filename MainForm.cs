using System;
using System.Drawing;
using System.Windows.Forms;
using NodeDock.Services;

namespace NodeDock
{
    public partial class MainForm : Form
    {
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _trayMenu;

        public MainForm()
        {
            InitializeComponent();
            SetupTray();
            
            // 初始化服务
            ManagerService.Instance.Initialize();
            
            // 绑定全局事件
            ManagerService.Instance.GlobalStatusChanged += (id, status) => {
                if (this.InvokeRequired)
                    this.Invoke(new Action(() => UpdateAppListUI(id, status)));
                else
                    UpdateAppListUI(id, status);
            };

            ManagerService.Instance.GlobalOutputReceived += (id, msg) => {
                if (this.InvokeRequired)
                    this.Invoke(new Action(() => AppendLog(id, msg)));
                else
                    AppendLog(id, msg);
            };

            LoadAppList();
        }

        private void AppendLog(string id, string msg)
        {
            if (lvApps.SelectedItems.Count > 0)
            {
                var selectedApp = lvApps.SelectedItems[0].Tag as Models.AppItem;
                if (selectedApp != null && selectedApp.Id == id)
                {
                    if (txtLog.Lines.Length > 500) // 限制显示行数
                    {
                        txtLog.Select(0, txtLog.GetFirstCharIndexFromLine(txtLog.Lines.Length - 500));
                        txtLog.ReadOnly = false;
                        txtLog.SelectedText = "";
                        txtLog.ReadOnly = true;
                    }
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                }
            }
        }

        private void LoadAppList()
        {
            lvApps.Items.Clear();
            foreach (var app in ConfigService.Instance.Settings.AppList)
            {
                var item = new ListViewItem(app.Name);
                item.Tag = app;
                item.SubItems.Add(app.Status.ToString());
                item.SubItems.Add(app.NodeVersion);
                item.SubItems.Add(app.WorkingDirectory);
                lvApps.Items.Add(item);
            }
        }

        private void UpdateAppListUI(string id, Models.AppStatus status)
        {
            foreach (ListViewItem item in lvApps.Items)
            {
                var app = item.Tag as Models.AppItem;
                if (app != null && app.Id == id)
                {
                    item.SubItems[1].Text = status.ToString();
                    // 根据状态改变颜色
                    switch (status)
                    {
                        case Models.AppStatus.Running: item.ForeColor = Color.FromArgb(140, 200, 75); break;
                        case Models.AppStatus.Error: item.ForeColor = Color.Salmon; break;
                        case Models.AppStatus.Stopped: item.ForeColor = Color.White; break;
                        case Models.AppStatus.Starting: item.ForeColor = Color.SkyBlue; break;
                    }
                    break;
                }
            }
        }

        private void btnStartAll_Click(object sender, EventArgs e) => ManagerService.Instance.StartAll();
        private void btnStopAll_Click(object sender, EventArgs e) => ManagerService.Instance.StopAll();

        private void btnAddApp_Click(object sender, EventArgs e)
        {
            using (var form = new AddAppForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    ConfigService.Instance.Settings.AppList.Add(form.AppInfo);
                    ConfigService.Instance.Save();
                    ManagerService.Instance.RefreshWorkers();
                    LoadAppList();
                }
            }
        }

        private void btnEditApp_Click(object sender, EventArgs e)
        {
            if (lvApps.SelectedItems.Count > 0)
            {
                var app = lvApps.SelectedItems[0].Tag as Models.AppItem;
                using (var form = new AddAppForm(app))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        ConfigService.Instance.Save();
                        ManagerService.Instance.RefreshWorkers();
                        LoadAppList();
                    }
                }
            }
        }

        private void btnRemoveApp_Click(object sender, EventArgs e)
        {
            if (lvApps.SelectedItems.Count > 0)
            {
                var app = lvApps.SelectedItems[0].Tag as Models.AppItem;
                if (MessageBox.Show($"确定要删除应用 '{app.Name}' 吗？", "确认删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ManagerService.Instance.StopApp(app.Id);
                    ConfigService.Instance.Settings.AppList.Remove(app);
                    ConfigService.Instance.Save();
                    ManagerService.Instance.RefreshWorkers();
                    LoadAppList();
                }
            }
        }

        private void btnStartApp_Click(object sender, EventArgs e)
        {
            if (lvApps.SelectedItems.Count > 0)
            {
                var app = lvApps.SelectedItems[0].Tag as Models.AppItem;
                ManagerService.Instance.StartApp(app.Id);
            }
        }

        private void btnStopApp_Click(object sender, EventArgs e)
        {
            if (lvApps.SelectedItems.Count > 0)
            {
                var app = lvApps.SelectedItems[0].Tag as Models.AppItem;
                ManagerService.Instance.StopApp(app.Id);
            }
        }

        private void btnOpenTerminal_Click(object sender, EventArgs e)
        {
            if (lvApps.SelectedItems.Count > 0)
            {
                var app = lvApps.SelectedItems[0].Tag as Models.AppItem;
                ManagerService.Instance.OpenTerminal(app.Id);
            }
        }

        private void lvApps_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = lvApps.SelectedItems.Count > 0;
            btnEditApp.Enabled = hasSelection;
            btnRemoveApp.Enabled = hasSelection;
            btnStartApp.Enabled = hasSelection;
            btnStopApp.Enabled = hasSelection;
            btnOpenTerminal.Enabled = hasSelection;

            txtLog.Clear();
            if (hasSelection)
            {
                var app = lvApps.SelectedItems[0].Tag as Models.AppItem;
                lblLogTitle.Text = $"实时日志预览 - {app.Name}";
            }
            else
            {
                lblLogTitle.Text = "实时日志预览 (仅显示当前选中项)";
            }
        }

        private void SetupTray()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("显示主界面", null, (s, e) => ShowMainForm());
            _trayMenu.Items.Add("-");
            _trayMenu.Items.Add("全部启动", null, (s, e) => ManagerService.Instance.StartAll());
            _trayMenu.Items.Add("全部停止", null, (s, e) => ManagerService.Instance.StopAll());
            _trayMenu.Items.Add("-");
            _trayMenu.Items.Add("退出 NodeDock", null, (s, e) => ExitApp());

            _notifyIcon = new NotifyIcon
            {
                Text = "NodeDock - Node.js 应用管理",
                Icon = SystemIcons.Application, // 临时使用系统图标
                ContextMenuStrip = _trayMenu,
                Visible = true
            };
            _notifyIcon.DoubleClick += (s, e) => ShowMainForm();
        }

        private void ShowMainForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void ExitApp()
        {
            _notifyIcon.Visible = false;
            ManagerService.Instance.StopAll();
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && ConfigService.Instance.Settings.MinimizeToTray)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                ManagerService.Instance.StopAll();
            }
        }

        private void UpdateAppListUI(string id, Models.AppStatus status)
        {
            // TODO: 更新 UI 列表项的状态颜色
        }
    }
}
