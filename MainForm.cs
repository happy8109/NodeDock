using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
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
        // 日志标签按钮映射：AppId -> Button
        private readonly Dictionary<string, Button> _logTabButtons = new Dictionary<string, Button>();
        // 当前选中的应用ID
        private string _currentLogAppId = null;
        private const int MaxLogLines = 500;
        private bool _isLoadingList = false;

        // 按钮区域定义
        private const int BtnWidth = 18;
        private const int BtnHeight = 18;
        private const int BtnSpacing = 4;

        public MainForm()
        {
            InitializeComponent();
            
            try {
                this.Icon = new Icon("Resources\\app_icon.ico");
            } catch { }
            
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

            ManagerService.Instance.GlobalResourceUpdated += () => {
                if (this.InvokeRequired)
                    this.Invoke(new Action(() => UpdateResourceUI()));
                else
                    UpdateResourceUI();
            };

            LoadAppList();

            // 取消按钮点击后的焦点边框效果
            btnStartAll.Enter += (s, e) => pnlSidebar.Focus();
            btnStopAll.Enter += (s, e) => pnlSidebar.Focus();
            btnAddApp.Enter += (s, e) => pnlSidebar.Focus();
            btnSetting.Enter += (s, e) => pnlSidebar.Focus();
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
                RemoveLogTab(app.Id);
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
            
            // 确保标签按钮存在
            EnsureLogTabButton(id);
            
            // 如果当前显示的是该应用的日志，直接追加
            if (_currentLogAppId == id)
            {
                if (txtLogContent.Lines.Length > MaxLogLines)
                {
                    txtLogContent.Select(0, txtLogContent.GetFirstCharIndexFromLine(txtLogContent.Lines.Length - MaxLogLines));
                    txtLogContent.ReadOnly = false;
                    txtLogContent.SelectedText = "";
                    txtLogContent.ReadOnly = true;
                }
                txtLogContent.AppendText(logLine);
                txtLogContent.SelectionStart = txtLogContent.Text.Length;
                txtLogContent.ScrollToCaret();
            }
        }

        /// <summary>
        /// 确保应用的日志标签按钮存在
        /// </summary>
        private void EnsureLogTabButton(string appId)
        {
            if (_logTabButtons.ContainsKey(appId)) return;

            var app = ConfigService.Instance.Settings.AppList.Find(a => a.Id == appId);
            if (app == null) return;

            CreateLogTabButton(appId, app);
        }

        /// <summary>
        /// 创建日志标签按钮
        /// </summary>
        private void CreateLogTabButton(string appId, Models.AppItem app)
        {
            var btn = new Button
            {
                Text = $"     {app.Name}",  // 前面留空给彩色圆点
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Margin = new Padding(2, 0, 2, 0),
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(243, 244, 246);
            btn.Click += LogTabButton_Click;

            _logTabButtons[appId] = btn;
            pnlLogTabs.Controls.Add(btn);

            // 设置状态颜色
            UpdateLogTabButtonStyle(appId, app.Status);

            // 如果是第一个标签，自动选中
            if (_currentLogAppId == null)
            {
                SwitchToLogTab(appId);
            }
        }

        /// <summary>
        /// 移除日志标签按钮
        /// </summary>
        private void RemoveLogTab(string appId)
        {
            if (_logTabButtons.ContainsKey(appId))
            {
                pnlLogTabs.Controls.Remove(_logTabButtons[appId]);
                _logTabButtons[appId].Dispose();
                _logTabButtons.Remove(appId);
            }

            // 如果移除的是当前显示的，切换到其他标签
            if (_currentLogAppId == appId)
            {
                _currentLogAppId = null;
                txtLogContent.Clear();
                
                // 切换到第一个可用的标签
                foreach (var kvp in _logTabButtons)
                {
                    SwitchToLogTab(kvp.Key);
                    break;
                }
            }
        }

        /// <summary>
        /// 点击日志标签按钮
        /// </summary>
        private void LogTabButton_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            // 从 _logTabButtons 字典中查找对应的 appId
            foreach (var kvp in _logTabButtons)
            {
                if (kvp.Value == btn)
                {
                    SwitchToLogTab(kvp.Key);
                    break;
                }
            }
        }

        /// <summary>
        /// 切换到指定应用的日志
        /// </summary>
        private void SwitchToLogTab(string appId)
        {
            _currentLogAppId = appId;

            // 更新按钮样式
            foreach (var kvp in _logTabButtons)
            {
                bool isSelected = (kvp.Key == appId);
                kvp.Value.BackColor = isSelected ? Color.White : Color.Transparent;
                kvp.Value.Font = new Font("Segoe UI", 9F, isSelected ? FontStyle.Bold : FontStyle.Regular);
                // 选中的按钮禁用悬停效果
                kvp.Value.FlatAppearance.MouseOverBackColor = isSelected ? Color.White : Color.FromArgb(243, 244, 246);
            }

            // 显示日志内容
            if (_logCache.ContainsKey(appId))
            {
                txtLogContent.Text = _logCache[appId].ToString();
                txtLogContent.SelectionStart = txtLogContent.Text.Length;
                txtLogContent.ScrollToCaret();
            }
            else
            {
                txtLogContent.Clear();
            }
        }

        /// <summary>
        /// 更新日志标签按钮样式（状态颜色）
        /// </summary>
        private void UpdateLogTabButtonStyle(string appId, Models.AppStatus status)
        {
            if (!_logTabButtons.ContainsKey(appId)) return;

            var app = ConfigService.Instance.Settings.AppList.Find(a => a.Id == appId);
            if (app == null) return;

            var btn = _logTabButtons[appId];
            Color statusColor;
            
            switch (status)
            {
                case Models.AppStatus.Running:
                    statusColor = Color.FromArgb(34, 197, 94);  // 绿色
                    break;
                case Models.AppStatus.Stopped:
                    statusColor = Color.FromArgb(156, 163, 175);  // 灰色
                    break;
                case Models.AppStatus.Error:
                    statusColor = Color.FromArgb(245, 158, 11);  // 橙色
                    break;
                case Models.AppStatus.Restarting:
                    statusColor = Color.FromArgb(59, 130, 246);  // 蓝色
                    break;
                default:
                    statusColor = Color.FromArgb(156, 163, 175);
                    break;
            }

            // 更新按钮文字（前面留空给彩色圆点）
            btn.Text = $"     {app.Name}";
            btn.ForeColor = _currentLogAppId == appId ? Color.FromArgb(17, 24, 39) : Color.FromArgb(107, 114, 128);
            
            // 通过 Paint 事件绘制彩色圆点
            btn.Paint -= LogTabButton_Paint;
            btn.Paint += LogTabButton_Paint;
            btn.Tag = statusColor;  // Tag 存储颜色
            btn.Invalidate();
        }

        /// <summary>
        /// 绘制日志标签按钮（带彩色圆点）
        /// </summary>
        private void LogTabButton_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is Color statusColor)
            {
                // 绘制彩色圆点
                using (var brush = new SolidBrush(statusColor))
                {
                    e.Graphics.FillEllipse(brush, 8, (btn.Height - 8) / 2, 8, 8);
                }
            }
        }

        private void LoadAppList()
        {
            _isLoadingList = true;
            dgvApps.Rows.Clear();
            int index = 1;
            foreach (var app in ConfigService.Instance.Settings.AppList)
            {
                string ports = app.DetectedPorts != null && app.DetectedPorts.Count > 0 ? string.Join(",", app.DetectedPorts) : "";
                
                bool hasConflict = app.ConflictPorts != null && app.ConflictPorts.Count > 0;
                string statusText = hasConflict ? $"! {string.Join(",", app.ConflictPorts)}" : app.Status.ToString();

                int rowIndex = dgvApps.Rows.Add(index++, app.Name, statusText, "", ports, app.NodeVersion, app.WorkingDirectory, "");
                dgvApps.Rows[rowIndex].Tag = app;
                UpdateRowStyle(dgvApps.Rows[rowIndex], app.Status, hasConflict);
            }
            _isLoadingList = false;
        }

        private void UpdateResourceUI()
        {
            float totalCpu = 0;
            long totalMemory = 0;

            foreach (DataGridViewRow row in dgvApps.Rows)
            {
                var app = row.Tag as Models.AppItem;
                if (app != null && app.Status == Models.AppStatus.Running)
                {
                    totalCpu += app.CpuUsage;
                    totalMemory += app.MemoryUsage;

                    string resourceText = $"{app.CpuUsage:F1}%|{app.MemoryUsage / 1024 / 1024}MB";
                    row.Cells["colResource"].Value = resourceText;
                }
                else
                {
                    row.Cells["colResource"].Value = "";
                }
            }

            lblResourceStatus.Text = $"总体资源占用: {totalCpu:F1}% | {totalMemory / 1024 / 1024}MB";
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
                    bool hasConflict = app.ConflictPorts != null && app.ConflictPorts.Count > 0;
                    string statusText = hasConflict ? $"! {string.Join(",", app.ConflictPorts)}" : status.ToString();
                    
                    row.Cells["colStatus"].Value = statusText;
                    
                    // 更新端口显示
                    string ports = app.DetectedPorts != null && app.DetectedPorts.Count > 0 ? string.Join(",", app.DetectedPorts) : "";
                    row.Cells["colPort"].Value = ports;

                    UpdateRowStyle(row, status, hasConflict);
                    dgvApps.InvalidateRow(row.Index); // 触发重绘操作按钮
                    
                    // 同步更新日志标签按钮样式
                    UpdateLogTabButtonStyle(id, status);
                    break;
                }
            }
        }

        private void UpdateRowStyle(DataGridViewRow row, Models.AppStatus status, bool hasConflict = false)
        {
            Color statusColor;
            if (hasConflict)
            {
                statusColor = Color.FromArgb(220, 38, 38);  // 红色，表示严重冲突
            }
            else
            {
                switch (status)
                {
                    case Models.AppStatus.Running:
                        statusColor = Color.FromArgb(22, 163, 74);  // 绿色
                        break;
                    case Models.AppStatus.Error:
                        statusColor = Color.FromArgb(220, 38, 38);  // 红色
                        break;
                    case Models.AppStatus.Starting:
                        statusColor = Color.FromArgb(37, 99, 235);  // 蓝色
                        break;
                    case Models.AppStatus.Restarting:
                        statusColor = Color.FromArgb(245, 158, 11); // 橙色
                        break;
                    default:
                        statusColor = Color.FromArgb(75, 85, 99);   // 灰色
                        break;
                }
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

            var app = dgvApps.Rows[e.RowIndex].Tag as Models.AppItem;
            if (app == null) return;

            // 处理路径链接点击
            if (dgvApps.Columns[e.ColumnIndex].Name == "colPath")
            {
                if (!string.IsNullOrEmpty(app.WorkingDirectory))
                {
                    try
                    {
                        Process.Start("explorer.exe", $"\"{app.WorkingDirectory}\"");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"无法打开目录: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return;
            }

            if (dgvApps.Columns[e.ColumnIndex].Name != "colAction") return;

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
            
            // 选中应用时自动切换到对应的日志
            var app = GetSelectedApp();
            if (app != null && _logTabButtons.ContainsKey(app.Id))
            {
                SwitchToLogTab(app.Id);
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
            using (var form = new SettingsForm())
            {
                form.ShowDialog(this);
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
                Icon = this.Icon ?? SystemIcons.Application,
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

        /// <summary>
        /// 处理 Windows 消息，用于单例模式激活窗口
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Program.WM_SHOWME)
            {
                ShowMainForm();
            }
            base.WndProc(ref m);
        }
    }
}
