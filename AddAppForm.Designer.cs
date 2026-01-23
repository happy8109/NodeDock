namespace NodeDock
{
    partial class AddAppForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtWorkDir = new System.Windows.Forms.TextBox();
            this.btnBrowseDir = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtScript = new System.Windows.Forms.TextBox();
            this.btnBrowseScript = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbVersion = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtArgs = new System.Windows.Forms.TextBox();
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.btnDownloadNode = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "应用名称：";
            // 
            // txtName
            // 
            this.txtName.BackColor = System.Drawing.Color.White;
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.txtName.Location = new System.Drawing.Point(120, 27);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(280, 21);
            this.txtName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "工作目录：";
            // 
            // txtWorkDir
            // 
            this.txtWorkDir.BackColor = System.Drawing.Color.White;
            this.txtWorkDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtWorkDir.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.txtWorkDir.Location = new System.Drawing.Point(120, 67);
            this.txtWorkDir.Name = "txtWorkDir";
            this.txtWorkDir.Size = new System.Drawing.Size(220, 21);
            this.txtWorkDir.TabIndex = 3;
            // 
            // btnBrowseDir
            // 
            this.btnBrowseDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseDir.Location = new System.Drawing.Point(346, 65);
            this.btnBrowseDir.Name = "btnBrowseDir";
            this.btnBrowseDir.Size = new System.Drawing.Size(54, 23);
            this.btnBrowseDir.TabIndex = 4;
            this.btnBrowseDir.Text = "浏览";
            this.btnBrowseDir.UseVisualStyleBackColor = true;
            this.btnBrowseDir.Click += new System.EventHandler(this.btnBrowseDir_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "入口脚本：";
            // 
            // txtScript
            // 
            this.txtScript.BackColor = System.Drawing.Color.White;
            this.txtScript.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtScript.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.txtScript.Location = new System.Drawing.Point(120, 107);
            this.txtScript.Name = "txtScript";
            this.txtScript.Size = new System.Drawing.Size(220, 21);
            this.txtScript.TabIndex = 6;
            // 
            // btnBrowseScript
            // 
            this.btnBrowseScript.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseScript.Location = new System.Drawing.Point(346, 105);
            this.btnBrowseScript.Name = "btnBrowseScript";
            this.btnBrowseScript.Size = new System.Drawing.Size(54, 23);
            this.btnBrowseScript.TabIndex = 7;
            this.btnBrowseScript.Text = "浏览";
            this.btnBrowseScript.UseVisualStyleBackColor = true;
            this.btnBrowseScript.Click += new System.EventHandler(this.btnBrowseScript_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "Node 版本：";
            // 
            // cmbVersion
            // 
            this.cmbVersion.BackColor = System.Drawing.Color.White;
            this.cmbVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.cmbVersion.FormattingEnabled = true;
            this.cmbVersion.Location = new System.Drawing.Point(120, 147);
            this.cmbVersion.Name = "cmbVersion";
            this.cmbVersion.Size = new System.Drawing.Size(210, 20);
            this.cmbVersion.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 190);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "启动参数：";
            // 
            // txtArgs
            // 
            this.txtArgs.BackColor = System.Drawing.Color.White;
            this.txtArgs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtArgs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.txtArgs.Location = new System.Drawing.Point(120, 187);
            this.txtArgs.Name = "txtArgs";
            this.txtArgs.Size = new System.Drawing.Size(280, 21);
            this.txtArgs.TabIndex = 11;
            // 
            // chkAutoStart
            // 
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Location = new System.Drawing.Point(120, 220);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new System.Drawing.Size(132, 16);
            this.chkAutoStart.TabIndex = 12;
            this.chkAutoStart.Text = "管理器启动时自启动";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // btnDownloadNode
            // 
            this.btnDownloadNode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownloadNode.Location = new System.Drawing.Point(340, 146);
            this.btnDownloadNode.Name = "btnDownloadNode";
            this.btnDownloadNode.Size = new System.Drawing.Size(60, 23);
            this.btnDownloadNode.TabIndex = 10;
            this.btnDownloadNode.Text = "下载...";
            this.btnDownloadNode.UseVisualStyleBackColor = true;
            this.btnDownloadNode.Click += new System.EventHandler(this.btnDownloadNode_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(240, 260);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnCancel.Location = new System.Drawing.Point(325, 260);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // AddAppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.ClientSize = new System.Drawing.Size(434, 311);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkAutoStart);
            this.Controls.Add(this.txtArgs);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbVersion);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnBrowseScript);
            this.Controls.Add(this.txtScript);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnBrowseDir);
            this.Controls.Add(this.txtWorkDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.btnDownloadNode);
            this.Controls.Add(this.label1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddAppForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "应用配置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtWorkDir;
        private System.Windows.Forms.Button btnBrowseDir;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtScript;
        private System.Windows.Forms.Button btnBrowseScript;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbVersion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtArgs;
        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDownloadNode;
    }
}
