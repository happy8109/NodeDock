namespace NodeDock
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.btnAddApp = new System.Windows.Forms.Button();
            this.btnStopAll = new System.Windows.Forms.Button();
            this.btnStartAll = new System.Windows.Forms.Button();
            this.lblLogo = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.lvApps = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlAppToolbar = new System.Windows.Forms.Panel();
            this.btnEditApp = new System.Windows.Forms.Button();
            this.btnRemoveApp = new System.Windows.Forms.Button();
            this.btnStartApp = new System.Windows.Forms.Button();
            this.btnStopApp = new System.Windows.Forms.Button();
            this.btnOpenTerminal = new System.Windows.Forms.Button();
            this.pnlLog = new System.Windows.Forms.Panel();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.lblLogTitle = new System.Windows.Forms.Label();
            this.pnlAppToolbar = new System.Windows.Forms.Panel();
            this.btnStopApp = new System.Windows.Forms.Button();
            this.btnStartApp = new System.Windows.Forms.Button();
            this.btnRemoveApp = new System.Windows.Forms.Button();
            this.btnEditApp = new System.Windows.Forms.Button();
            this.pnlSidebar.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlLog.SuspendLayout();
            this.pnlAppToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.pnlSidebar.Controls.Add(this.btnAddApp);
            this.pnlSidebar.Controls.Add(this.btnStopAll);
            this.pnlSidebar.Controls.Add(this.btnStartAll);
            this.pnlSidebar.Controls.Add(this.lblLogo);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(200, 600);
            this.pnlSidebar.TabIndex = 0;
            // 
            // btnAddApp
            // 
            this.btnAddApp.BackColor = System.Drawing.Color.White;
            this.btnAddApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddApp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnAddApp.Location = new System.Drawing.Point(20, 220);
            this.btnAddApp.Name = "btnAddApp";
            this.btnAddApp.Size = new System.Drawing.Size(160, 40);
            this.btnAddApp.TabIndex = 3;
            this.btnAddApp.Text = "添加应用";
            this.btnAddApp.UseVisualStyleBackColor = false;
            this.btnAddApp.Click += new System.EventHandler(this.btnAddApp_Click);
            // 
            // btnStopAll
            // 
            this.btnStopAll.BackColor = System.Drawing.Color.White;
            this.btnStopAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnStopAll.Location = new System.Drawing.Point(20, 150);
            this.btnStopAll.Name = "btnStopAll";
            this.btnStopAll.Size = new System.Drawing.Size(160, 40);
            this.btnStopAll.TabIndex = 2;
            this.btnStopAll.Text = "停止全部";
            this.btnStopAll.UseVisualStyleBackColor = false;
            this.btnStopAll.Click += new System.EventHandler(this.btnStopAll_Click);
            // 
            // btnStartAll
            // 
            this.btnStartAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnStartAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartAll.ForeColor = System.Drawing.Color.White;
            this.btnStartAll.Location = new System.Drawing.Point(20, 100);
            this.btnStartAll.Name = "btnStartAll";
            this.btnStartAll.Size = new System.Drawing.Size(160, 40);
            this.btnStartAll.TabIndex = 1;
            this.btnStartAll.Text = "启动全部";
            this.btnStartAll.UseVisualStyleBackColor = false;
            this.btnStartAll.Click += new System.EventHandler(this.btnStartAll_Click);
            // 
            // lblLogo
            // 
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblLogo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.lblLogo.Location = new System.Drawing.Point(0, 20);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(200, 50);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Text = "NodeDock";
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.lvApps);
            this.pnlMain.Controls.Add(this.pnlLog);
            this.pnlMain.Controls.Add(this.pnlAppToolbar);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(200, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMain.Size = new System.Drawing.Size(700, 600);
            this.pnlMain.TabIndex = 1;
            // 
            // lvApps
            // 
            this.lvApps.BackColor = System.Drawing.Color.White;
            this.lvApps.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvApps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colStatus,
            this.colVersion,
            this.colPath});
            this.lvApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvApps.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.lvApps.FullRowSelect = true;
            this.lvApps.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvApps.HideSelection = false;
            this.lvApps.Location = new System.Drawing.Point(10, 60);
            this.lvApps.MultiSelect = false;
            this.lvApps.Name = "lvApps";
            this.lvApps.Size = new System.Drawing.Size(680, 330);
            this.lvApps.TabIndex = 0;
            this.lvApps.UseCompatibleStateImageBehavior = false;
            this.lvApps.View = System.Windows.Forms.View.Details;
            this.lvApps.SelectedIndexChanged += new System.EventHandler(this.lvApps_SelectedIndexChanged);
            // 
            // colName
            // 
            this.colName.Text = "应用名称";
            this.colName.Width = 150;
            // 
            // colStatus
            // 
            this.colStatus.Text = "状态";
            this.colStatus.Width = 100;
            // 
            // colVersion
            // 
            this.colVersion.Text = "Node 版本";
            this.colVersion.Width = 120;
            // 
            // colPath
            // 
            this.colPath.Text = "工作目录";
            this.colPath.Width = 300;
            // 
            // pnlLog
            // 
            this.pnlLog.Controls.Add(this.txtLog);
            this.pnlLog.Controls.Add(this.lblLogTitle);
            this.pnlLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLog.Location = new System.Drawing.Point(10, 390);
            this.pnlLog.Name = "pnlLog";
            this.pnlLog.Size = new System.Drawing.Size(680, 200);
            this.pnlLog.TabIndex = 2;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.White;
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.txtLog.Location = new System.Drawing.Point(0, 20);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(680, 180);
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "";
            // 
            // lblLogTitle
            // 
            this.lblLogTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLogTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLogTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.lblLogTitle.Location = new System.Drawing.Point(0, 0);
            this.lblLogTitle.Name = "lblLogTitle";
            this.lblLogTitle.Size = new System.Drawing.Size(680, 20);
            this.lblLogTitle.TabIndex = 1;
            this.lblLogTitle.Text = "实时日志预览 (仅显示当前选中项)";
            this.lblLogTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAppToolbar
            // 
            this.pnlAppToolbar.Controls.Add(this.btnOpenTerminal);
            this.pnlAppToolbar.Controls.Add(this.btnStopApp);
            this.pnlAppToolbar.Controls.Add(this.btnStartApp);
            this.pnlAppToolbar.Controls.Add(this.btnRemoveApp);
            this.pnlAppToolbar.Controls.Add(this.btnEditApp);
            this.pnlAppToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAppToolbar.Location = new System.Drawing.Point(10, 10);
            this.pnlAppToolbar.Name = "pnlAppToolbar";
            this.pnlAppToolbar.Size = new System.Drawing.Size(680, 50);
            this.pnlAppToolbar.TabIndex = 1;
            // 
            // btnStopApp
            // 
            this.btnStopApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopApp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnStopApp.Location = new System.Drawing.Point(600, 10);
            this.btnStopApp.Name = "btnStopApp";
            this.btnStopApp.Size = new System.Drawing.Size(80, 30);
            this.btnStopApp.TabIndex = 3;
            this.btnStopApp.Text = "停止";
            this.btnStopApp.UseVisualStyleBackColor = true;
            this.btnStopApp.Click += new System.EventHandler(this.btnStopApp_Click);
            // 
            // btnStartApp
            // 
            this.btnStartApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartApp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this.btnStartApp.Location = new System.Drawing.Point(510, 10);
            this.btnStartApp.Name = "btnStartApp";
            this.btnStartApp.Size = new System.Drawing.Size(80, 30);
            this.btnStartApp.TabIndex = 2;
            this.btnStartApp.Text = "启动";
            this.btnStartApp.UseVisualStyleBackColor = true;
            this.btnStartApp.Click += new System.EventHandler(this.btnStartApp_Click);
            // 
            // btnRemoveApp
            // 
            this.btnRemoveApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveApp.Location = new System.Drawing.Point(90, 10);
            this.btnRemoveApp.Name = "btnRemoveApp";
            this.btnRemoveApp.Size = new System.Drawing.Size(80, 30);
            this.btnRemoveApp.TabIndex = 1;
            this.btnRemoveApp.Text = "删除";
            this.btnRemoveApp.UseVisualStyleBackColor = true;
            this.btnRemoveApp.Click += new System.EventHandler(this.btnRemoveApp_Click);
            // 
            // btnEditApp
            // 
            this.btnEditApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditApp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnEditApp.Location = new System.Drawing.Point(0, 10);
            this.btnEditApp.Name = "btnEditApp";
            this.btnEditApp.Size = new System.Drawing.Size(80, 30);
            this.btnEditApp.TabIndex = 0;
            this.btnEditApp.Text = "编辑";
            this.btnEditApp.UseVisualStyleBackColor = true;
            this.btnEditApp.Click += new System.EventHandler(this.btnEditApp_Click);
            // 
            // btnOpenTerminal
            // 
            this.btnOpenTerminal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenTerminal.Location = new System.Drawing.Point(180, 10);
            this.btnOpenTerminal.Name = "btnOpenTerminal";
            this.btnOpenTerminal.Size = new System.Drawing.Size(80, 30);
            this.btnOpenTerminal.TabIndex = 4;
            this.btnOpenTerminal.Text = "终端";
            this.btnOpenTerminal.UseVisualStyleBackColor = true;
            this.btnOpenTerminal.Click += new System.EventHandler(this.btnOpenTerminal_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlSidebar);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NodeDock - Node.js 应用管理";
            this.pnlSidebar.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlLog.ResumeLayout(false);
            this.pnlAppToolbar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Button btnStartAll;
        private System.Windows.Forms.Button btnStopAll;
        private System.Windows.Forms.Button btnAddApp;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.ListView lvApps;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colVersion;
        private System.Windows.Forms.ColumnHeader colPath;
        private System.Windows.Forms.Panel pnlAppToolbar;
        private System.Windows.Forms.Button btnEditApp;
        private System.Windows.Forms.Button btnRemoveApp;
        private System.Windows.Forms.Button btnStartApp;
        private System.Windows.Forms.Button btnStopApp;
        private System.Windows.Forms.Button btnOpenTerminal;
        private System.Windows.Forms.Panel pnlLog;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Label lblLogTitle;
    }
}
