using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NodeDock.Services;

namespace NodeDock
{
    public partial class MainForm : Form
    {
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _trayMenu;
        private ContextMenuStrip _appContextMenu;
        
        // 日志缓存：每个应用独立存储日志历史
        private readonly Dictionary<string, StringBuilder> _logCache = new Dictionary<string, StringBuilder>();
        // 日志标签页映射：AppId -> TabPage
        private readonly Dictionary<string, TabPage> _logTabs = new Dictionary<string, TabPage>();
        private const int MaxLogLines = 500;
        private bool _isLoadingList = false;

        // 按钮区域定义
        private const int BtnWidth = 18;
        private const int BtnHeight = 18;
        private const int BtnSpacing = 4;

        public MainForm()
        {
            InitializeComponent();
            SetupTray();
            SetupAppContextMenu();
            
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

        private void SetupAppContextMenu()
        {
            _appContextMenu = new ContextMenuStrip();
            _appContextMenu.Items.Add("编辑", null, (s, e) => ContextMenuAction_Edit());
            _appContextMenu.Items.Add("删除", null, (s, e) => ContextMenuAction_Delete());
            _appContextMenu.Items.Add("-");
            _appContextMenu.Items.Add("打开终端", null, (s, e) => ContextMenuAction_Terminal());
            
            dgvApps.ContextMenuStrip = _appContextMenu;
        }

        private Models.AppItem GetSelectedApp()
        {
            if (dgvApps.SelectedRows.Count > 0)
                return dgvApps.SelectedRows[0].Tag as Models.AppItem;
            return null;
        }

        private void ContextMenuAction_Edit()
        {
            var app = GetSelectedApp();
            if (app != null)
            {
                string appId = app.Id;
                using (var form = new AddAppForm(app))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        ConfigService.Instance.Save();
                        ManagerService.Instance.RefreshWorkers();
                        LoadAppList();
                        SelectAppById(appId);
                    }
                }
            }
        }

        private void ContextMenuAction_Delete()
        {
            var app = GetSelectedApp();
            if (app != null && MessageBox.Show($"确定要删除应用 '{app.Name}' 吗？", "确认删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ManagerService.Instance.StopApp(app.Id);
                ConfigService.Instance.Settings.AppList.Remove(app);
                ConfigService.Instance.Save();
                ManagerService.Instance.RefreshWorkers();
                _logCache.Remove(app.Id);
                LoadAppList();
            }
        }

        private void ContextMenuAction_Terminal()
        {
            var app = GetSelectedApp();
            if (app != null) ManagerService.Instance.OpenTerminal(app.Id);
        }

        private void AppendLog(string id, string msg)
        {
            string logLine = $"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}";
            
            // 写入缓存
            if (!_logCache.ContainsKey(id))
            {
                _logCache[id] = new StringBuilder();
            }
            _logCache[id].Append(logLine);
            
            // 限制缓存大小（按字符数粗略限制）
            if (_logCache[id].Length > 50000)
            {
                string content = _logCache[id].ToString();
                int cutIndex = content.IndexOf(Environment.NewLine, content.Length / 2);
                if (cutIndex > 0)
                {
                    _logCache[id] = new StringBuilder(content.Substring(cutIndex + Environment.NewLine.Length));
                }
            }
            
            // 获取或创建对应的日志标签页
            var tabPage = GetOrCreateLogTab(id);
            if (tabPage != null && tabPage.Controls.Count > 0)
            {
                var txtLog = tabPage.Controls[0] as RichTextBox;
                if (txtLog != null)
                {
                    if (txtLog.Lines.Length > MaxLogLines)
                    {
                        txtLog.Select(0, txtLog.GetFirstCharIndexFromLine(txtLog.Lines.Length - MaxLogLines));
                        txtLog.ReadOnly = false;
                        txtLog.SelectedText = "";
                        txtLog.ReadOnly = true;
                    }
                    txtLog.AppendText(logLine);
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                }
            }
        }

        /// <summary>
        /// 获取或创建应用的日志标签页
        /// </summary>
        private TabPage GetOrCreateLogTab(string appId)
        {
            if (_logTabs.ContainsKey(appId))
            {
                return _logTabs[appId];
            }

            // 查找应用名称
            var app = ConfigService.Instance.Settings.AppList.Find(a => a.Id == appId);
            if (app == null) return null;

            return CreateLogTab(appId, app.Name);
        }

        /// <summary>
        /// 创建日志标签页
        /// </summary>
        private TabPage CreateLogTab(string appId, string appName)
        {
            var tabPage = new TabPage(appName)
            {
                Tag = appId
            };

            var txtLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9F),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                BorderStyle = BorderStyle.None
            };

            // 如果缓存中有历史日志，恢复显示
            if (_logCache.ContainsKey(appId))
            {
                txtLog.Text = _logCache[appId].ToString();
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();
            }

            tabPage.Controls.Add(txtLog);
            tabLogs.TabPages.Add(tabPage);
            _logTabs[appId] = tabPage;

            // 自动切换到新标签页
            tabLogs.SelectedTab = tabPage;

            return tabPage;
        }

        /// <summary>
        /// 更新日志标签页标题（用于显示状态）
        /// </summary>
        private void UpdateLogTabTitle(string appId, Models.AppStatus status)
        {
            if (!_logTabs.ContainsKey(appId)) return;

            var app = ConfigService.Instance.Settings.AppList.Find(a => a.Id == appId);
            if (app == null) return;

            var tabPage = _logTabs[appId];
            switch (status)
            {
                case Models.AppStatus.Running:
                    tabPage.Text = app.Name;
                    break;
                case Models.AppStatus.Stopped:
                    tabPage.Text = $"{app.Name} (已停止)";
                    break;
                case Models.AppStatus.Error:
                    tabPage.Text = $"{app.Name} (异常)";
                    break;
                default:
                    tabPage.Text = app.Name;
                    break;
            }
        }

        private void LoadAppList()
        {
            _isLoadingList = true;
            dgvApps.Rows.Clear();
            int index = 1;
            foreach (var app in ConfigService.Instance.Settings.AppList)
            {
                int rowIndex = dgvApps.Rows.Add(index++, app.Name, app.Status.ToString(), app.NodeVersion, app.WorkingDirectory, "");
                dgvApps.Rows[rowIndex].Tag = app;
                UpdateRowStyle(dgvApps.Rows[rowIndex], app.Status);
            }
            _isLoadingList = false;
        }

        private void SelectAppById(string appId)
        {
            foreach (DataGridViewRow row in dgvApps.Rows)
            {
                var app = row.Tag as Models.AppItem;
                if (app != null && app.Id == appId)
                {
                    row.Selected = true;
                    dgvApps.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        private void UpdateAppListUI(string id, Models.AppStatus status)
        {
            foreach (DataGridViewRow row in dgvApps.Rows)
            {
                var app = row.Tag as Models.AppItem;
                if (app != null && app.Id == id)
                {
                    row.Cells["colStatus"].Value = status.ToString();
                    UpdateRowStyle(row, status);
                    dgvApps.InvalidateRow(row.Index); // 触发重绘操作按钮
                    
                    // 同步更新日志标签页标题
                    UpdateLogTabTitle(id, status);
                    break;
                }
            }
        }

        private void UpdateRowStyle(DataGridViewRow row, Models.AppStatus status)
        {
            Color statusColor;
            switch (status)
            {
                case Models.AppStatus.Running:
                    statusColor = Color.FromArgb(22, 163, 74);
                    break;
                case Models.AppStatus.Error:
                    statusColor = Color.FromArgb(220, 38, 38);
                    break;
                case Models.AppStatus.Starting:
                    statusColor = Color.FromArgb(37, 99, 235);
                    break;
                default:
                    statusColor = Color.FromArgb(75, 85, 99);
                    break;
            }
            row.Cells["colStatus"].Style.ForeColor = statusColor;
        }

        private void dgvApps_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            if (dgvApps.Columns[e.ColumnIndex].Name != "colAction") return;

            var app = dgvApps.Rows[e.RowIndex].Tag as Models.AppItem;
            if (app == null) return;

            e.PaintBackground(e.ClipBounds, true);

            bool isRunning = app.Status == Models.AppStatus.Running || app.Status == Models.AppStatus.Starting;

            // 计算按钮位置
            int totalWidth = BtnWidth * 2 + BtnSpacing;
            int startX = e.CellBounds.X + (e.CellBounds.Width - totalWidth) / 2;
            int startY = e.CellBounds.Y + (e.CellBounds.Height - BtnHeight) / 2;

            Rectangle startBtnRect = new Rectangle(startX, startY, BtnWidth, BtnHeight);
            Rectangle stopBtnRect = new Rectangle(startX + BtnWidth + BtnSpacing, startY, BtnWidth, BtnHeight);

            // 绘制按钮背景（统一淡灰色，不代表状态）
            using (var brush = new SolidBrush(Color.FromArgb(243, 244, 246)))
            {
                e.Graphics.FillRectangle(brush, startBtnRect);
                e.Graphics.FillRectangle(brush, stopBtnRect);
            }

            using (var font = new Font("Segoe UI Symbol", 6, FontStyle.Bold))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                // 绘制启动按钮图标（可启动时为绿色，否则为灰色）
                using (var startBrush = new SolidBrush(!isRunning ? Color.FromArgb(34, 197, 94) : Color.FromArgb(156, 163, 175)))
                {
                    e.Graphics.DrawString("▶", font, startBrush, startBtnRect, sf);
                }

                // 绘制停止按钮图标（可停止时为红色，否则为灰色）
                using (var stopBrush = new SolidBrush(isRunning ? Color.FromArgb(239, 68, 68) : Color.FromArgb(156, 163, 175)))
                {
                    e.Graphics.DrawString("■", font, stopBrush, stopBtnRect, sf);
                }
            }

            e.Handled = true;
        }

        private void dgvApps_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvApps.Columns[e.ColumnIndex].Name != "colAction") return;

            var app = dgvApps.Rows[e.RowIndex].Tag as Models.AppItem;
            if (app == null) return;

            // 计算点击位置对应的按钮
            var cellRect = dgvApps.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            var mousePos = dgvApps.PointToClient(MousePosition);
            int relativeX = mousePos.X - cellRect.X;

            int totalWidth = BtnWidth * 2 + BtnSpacing;
            int startX = (cellRect.Width - totalWidth) / 2;

            bool isRunning = app.Status == Models.AppStatus.Running || app.Status == Models.AppStatus.Starting;

            // 判断点击的是哪个按钮
            if (relativeX >= startX && relativeX <= startX + BtnWidth)
            {
                // 点击了启动按钮
                if (!isRunning)
                {
                    ManagerService.Instance.StartApp(app.Id);
                }
            }
            else if (relativeX >= startX + BtnWidth + BtnSpacing && relativeX <= startX + BtnWidth * 2 + BtnSpacing)
            {
                // 点击了停止按钮
                if (isRunning)
                {
                    ManagerService.Instance.StopApp(app.Id);
                }
            }
        }

        private void dgvApps_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoadingList) return;
            
            // 选中应用时自动切换到对应的日志标签页
            var app = GetSelectedApp();
            if (app != null && _logTabs.ContainsKey(app.Id))
            {
                tabLogs.SelectedTab = _logTabs[app.Id];
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

        private void btnSetting_Click(object sender, EventArgs e)
        {
            // TODO: 打开系统设置窗口
            MessageBox.Show("系统设置功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Icon = SystemIcons.Application,
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
    }
}
