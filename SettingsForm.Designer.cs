namespace NodeDock
{
    partial class SettingsForm
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
            this.lstVersions = new System.Windows.Forms.ListBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDefault = new System.Windows.Forms.Label();
            this.btnSetDefault = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpMirror = new System.Windows.Forms.GroupBox();
            this.rbMirrorTaobao = new System.Windows.Forms.RadioButton();
            this.rbMirrorOfficial = new System.Windows.Forms.RadioButton();
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.chkWin7Compatibility = new System.Windows.Forms.CheckBox();
            this.btnDownload = new System.Windows.Forms.Button();
            this.grpMirror.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(160, 17);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "已安装的 Node.js 版本:";
            // 
            // lstVersions
            // 
            this.lstVersions.BackColor = System.Drawing.Color.White;
            this.lstVersions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstVersions.Font = new System.Drawing.Font("Consolas", 10F);
            this.lstVersions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lstVersions.FormattingEnabled = true;
            this.lstVersions.ItemHeight = 15;
            this.lstVersions.Location = new System.Drawing.Point(20, 40);
            this.lstVersions.Name = "lstVersions";
            this.lstVersions.Size = new System.Drawing.Size(340, 137);
            this.lstVersions.TabIndex = 1;
            // 
            // lblDefault
            // 
            this.lblDefault.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblDefault.Location = new System.Drawing.Point(20, 185);
            this.lblDefault.Name = "lblDefault";
            this.lblDefault.Size = new System.Drawing.Size(340, 20);
            this.lblDefault.TabIndex = 2;
            this.lblDefault.Text = "当前默认版本: 未设置";
            // 
            // btnSetDefault
            // 
            this.btnSetDefault.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnSetDefault.FlatAppearance.BorderSize = 0;
            this.btnSetDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetDefault.ForeColor = System.Drawing.Color.White;
            this.btnSetDefault.Location = new System.Drawing.Point(188, 208);
            this.btnSetDefault.Name = "btnSetDefault";
            this.btnSetDefault.Size = new System.Drawing.Size(85, 28);
            this.btnSetDefault.TabIndex = 3;
            this.btnSetDefault.Text = "设为默认";
            this.btnSetDefault.UseVisualStyleBackColor = false;
            this.btnSetDefault.Click += new System.EventHandler(this.btnSetDefault_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this.btnDownload.FlatAppearance.BorderSize = 0;
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.Location = new System.Drawing.Point(20, 208);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(100, 28);
            this.btnDownload.TabIndex = 8;
            this.btnDownload.Text = "下载新版本...";
            this.btnDownload.UseVisualStyleBackColor = false;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(279, 208);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(81, 28);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "删除版本";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // grpMirror
            // 
            this.grpMirror.Controls.Add(this.rbMirrorTaobao);
            this.grpMirror.Controls.Add(this.rbMirrorOfficial);
            this.grpMirror.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.grpMirror.Location = new System.Drawing.Point(20, 245);
            this.grpMirror.Name = "grpMirror";
            this.grpMirror.Size = new System.Drawing.Size(340, 50);
            this.grpMirror.TabIndex = 5;
            this.grpMirror.TabStop = false;
            this.grpMirror.Text = "下载镜像源";
            // 
            // rbMirrorTaobao
            // 
            this.rbMirrorTaobao.AutoSize = true;
            this.rbMirrorTaobao.Location = new System.Drawing.Point(180, 22);
            this.rbMirrorTaobao.Name = "rbMirrorTaobao";
            this.rbMirrorTaobao.Size = new System.Drawing.Size(131, 16);
            this.rbMirrorTaobao.TabIndex = 1;
            this.rbMirrorTaobao.Text = "淘宝镜像 (国内推荐)";
            this.rbMirrorTaobao.UseVisualStyleBackColor = true;
            this.rbMirrorTaobao.CheckedChanged += new System.EventHandler(this.rbMirror_CheckedChanged);
            // 
            // rbMirrorOfficial
            // 
            this.rbMirrorOfficial.AutoSize = true;
            this.rbMirrorOfficial.Checked = true;
            this.rbMirrorOfficial.Location = new System.Drawing.Point(15, 22);
            this.rbMirrorOfficial.Name = "rbMirrorOfficial";
            this.rbMirrorOfficial.Size = new System.Drawing.Size(143, 16);
            this.rbMirrorOfficial.TabIndex = 0;
            this.rbMirrorOfficial.TabStop = true;
            this.rbMirrorOfficial.Text = "官方源 (nodejs.org)";
            this.rbMirrorOfficial.UseVisualStyleBackColor = true;
            this.rbMirrorOfficial.CheckedChanged += new System.EventHandler(this.rbMirror_CheckedChanged);
            // 
            // chkAutoStart
            // 
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.chkAutoStart.Location = new System.Drawing.Point(20, 310);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new System.Drawing.Size(168, 16);
            this.chkAutoStart.TabIndex = 6;
            this.chkAutoStart.Text = "开机自动启动 NodeDock";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            this.chkAutoStart.CheckedChanged += new System.EventHandler(this.chkAutoStart_CheckedChanged);
            // 
            // chkWin7Compatibility
            // 
            this.chkWin7Compatibility.AutoSize = true;
            this.chkWin7Compatibility.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.chkWin7Compatibility.Location = new System.Drawing.Point(20, 335);
            this.chkWin7Compatibility.Name = "chkWin7Compatibility";
            this.chkWin7Compatibility.Size = new System.Drawing.Size(222, 16);
            this.chkWin7Compatibility.TabIndex = 9;
            this.chkWin7Compatibility.Text = "开启 Windows 7 兼容模式 (跳过检测)";
            this.chkWin7Compatibility.UseVisualStyleBackColor = true;
            this.chkWin7Compatibility.CheckedChanged += new System.EventHandler(this.chkWin7Compatibility_CheckedChanged);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnClose.Location = new System.Drawing.Point(275, 330);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 28);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.ClientSize = new System.Drawing.Size(380, 375);
            this.Controls.Add(this.chkWin7Compatibility);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.chkAutoStart);
            this.Controls.Add(this.grpMirror);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnSetDefault);
            this.Controls.Add(this.lblDefault);
            this.Controls.Add(this.lstVersions);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "系统设置";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.grpMirror.ResumeLayout(false);
            this.grpMirror.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ListBox lstVersions;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDefault;
        private System.Windows.Forms.Button btnSetDefault;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox grpMirror;
        private System.Windows.Forms.RadioButton rbMirrorTaobao;
        private System.Windows.Forms.RadioButton rbMirrorOfficial;
        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.CheckBox chkWin7Compatibility;
        private System.Windows.Forms.Button btnDownload;
    }
}
