namespace NodeDock
{
    partial class DownloadVersionForm
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
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstVersions
            // 
            this.lstVersions.BackColor = System.Drawing.Color.White;
            this.lstVersions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstVersions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lstVersions.FormattingEnabled = true;
            this.lstVersions.ItemHeight = 12;
            this.lstVersions.Location = new System.Drawing.Point(20, 60);
            this.lstVersions.Name = "lstVersions";
            this.lstVersions.Size = new System.Drawing.Size(340, 218);
            this.lstVersions.TabIndex = 2;
            // 
            // btnDownload
            // 
            this.btnDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.Location = new System.Drawing.Point(204, 320);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 30);
            this.btnDownload.TabIndex = 3;
            this.btnDownload.Text = "下载";
            this.btnDownload.UseVisualStyleBackColor = false;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnCancel.Location = new System.Drawing.Point(285, 320);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(20, 298);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(340, 15);
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblStatus.Location = new System.Drawing.Point(20, 280);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(340, 15);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "准备就绪";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.Color.White;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.txtSearch.Location = new System.Drawing.Point(20, 30);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(340, 21);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(20, 15);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(101, 12);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "搜索版本 (如 18):";
            // 
            // DownloadVersionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.ClientSize = new System.Drawing.Size(380, 370);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.lstVersions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DownloadVersionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "下载 Node.js 版本";
            this.Load += new System.EventHandler(this.DownloadVersionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ListBox lstVersions;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
    }
}
