namespace SiliconStudio.LauncherApp
{
    partial class SplashForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();

                // Deregister from launcher app when the form is closed
                if (isInitialized)
                {
                    launcher.Loading -= launcher_Loading;
                    launcher.ProgressAvailable -= launcher_ProgressAvailable;
                    launcher.LogAvailable -= launcher_LogAvailable;
                    timer.Stop();
                    timer.Tick -= timer_Tick;
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            this.logLabel = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.launcherVersionLabel = new System.Windows.Forms.Label();
            this.minimizeButton = new System.Windows.Forms.Button();
            this.versionListBox = new System.Windows.Forms.ListBox();
            this.SDKInstall = new System.Windows.Forms.Button();
            this.SDKUpgrade = new System.Windows.Forms.Button();
            this.SDKUninstall = new System.Windows.Forms.Button();
            this.RunnableVersionsComboBox = new System.Windows.Forms.ComboBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.VsixButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.controlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // logLabel
            // 
            this.logLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.logLabel.AutoSize = true;
            this.logLabel.BackColor = System.Drawing.Color.Transparent;
            this.logLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logLabel.ForeColor = System.Drawing.Color.White;
            this.logLabel.Location = new System.Drawing.Point(12, 348);
            this.logLabel.Name = "logLabel";
            this.logLabel.Size = new System.Drawing.Size(135, 21);
            this.logLabel.TabIndex = 0;
            this.logLabel.Text = "Loading Paradox...";
            // 
            // versionLabel
            // 
            this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.versionLabel.BackColor = System.Drawing.Color.Transparent;
            this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionLabel.ForeColor = System.Drawing.Color.White;
            this.versionLabel.Location = new System.Drawing.Point(206, 348);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(396, 21);
            this.versionLabel.TabIndex = 1;
            this.versionLabel.Text = "Version: 1.0.0";
            this.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // launcherVersionLabel
            // 
            this.launcherVersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.launcherVersionLabel.BackColor = System.Drawing.Color.Transparent;
            this.launcherVersionLabel.ForeColor = System.Drawing.Color.Gray;
            this.launcherVersionLabel.Location = new System.Drawing.Point(16, 325);
            this.launcherVersionLabel.Name = "launcherVersionLabel";
            this.launcherVersionLabel.Size = new System.Drawing.Size(586, 23);
            this.launcherVersionLabel.TabIndex = 2;
            this.launcherVersionLabel.Text = "Launcher Version: 1.0.0";
            this.launcherVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // minimizeButton
            // 
            this.minimizeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.minimizeButton.BackColor = System.Drawing.Color.Transparent;
            this.minimizeButton.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.minimizeButton.FlatAppearance.BorderSize = 0;
            this.minimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.minimizeButton.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.minimizeButton.ForeColor = System.Drawing.Color.White;
            this.minimizeButton.Location = new System.Drawing.Point(569, 0);
            this.minimizeButton.Name = "minimizeButton";
            this.minimizeButton.Size = new System.Drawing.Size(18, 21);
            this.minimizeButton.TabIndex = 3;
            this.minimizeButton.Text = "-";
            this.minimizeButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.minimizeButton.UseVisualStyleBackColor = false;
            this.minimizeButton.Click += new System.EventHandler(this.minimizeButton_Click);
            // 
            // versionListBox
            // 
            this.versionListBox.FormattingEnabled = true;
            this.versionListBox.Location = new System.Drawing.Point(234, 25);
            this.versionListBox.Name = "versionListBox";
            this.versionListBox.Size = new System.Drawing.Size(321, 82);
            this.versionListBox.TabIndex = 4;
            this.versionListBox.SelectedIndexChanged += new System.EventHandler(this.versionListBox_SelectedIndexChanged);
            // 
            // SDKInstall
            // 
            this.SDKInstall.Enabled = false;
            this.SDKInstall.Location = new System.Drawing.Point(234, 113);
            this.SDKInstall.Name = "SDKInstall";
            this.SDKInstall.Size = new System.Drawing.Size(75, 23);
            this.SDKInstall.TabIndex = 5;
            this.SDKInstall.Text = "Install";
            this.SDKInstall.UseVisualStyleBackColor = true;
            this.SDKInstall.Click += new System.EventHandler(this.SDKInstall_Click);
            // 
            // SDKUpgrade
            // 
            this.SDKUpgrade.Enabled = false;
            this.SDKUpgrade.Location = new System.Drawing.Point(316, 113);
            this.SDKUpgrade.Name = "SDKUpgrade";
            this.SDKUpgrade.Size = new System.Drawing.Size(75, 23);
            this.SDKUpgrade.TabIndex = 6;
            this.SDKUpgrade.Text = "Upgrade";
            this.SDKUpgrade.UseVisualStyleBackColor = true;
            this.SDKUpgrade.Click += new System.EventHandler(this.SDKUpgrade_Click);
            // 
            // SDKUninstall
            // 
            this.SDKUninstall.Enabled = false;
            this.SDKUninstall.Location = new System.Drawing.Point(397, 113);
            this.SDKUninstall.Name = "SDKUninstall";
            this.SDKUninstall.Size = new System.Drawing.Size(75, 23);
            this.SDKUninstall.TabIndex = 7;
            this.SDKUninstall.Text = "Delete";
            this.SDKUninstall.UseVisualStyleBackColor = true;
            this.SDKUninstall.Click += new System.EventHandler(this.SDKUninstall_Click);
            // 
            // RunnableVersionsComboBox
            // 
            this.RunnableVersionsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RunnableVersionsComboBox.FormattingEnabled = true;
            this.RunnableVersionsComboBox.Location = new System.Drawing.Point(30, 28);
            this.RunnableVersionsComboBox.Name = "RunnableVersionsComboBox";
            this.RunnableVersionsComboBox.Size = new System.Drawing.Size(164, 21);
            this.RunnableVersionsComboBox.TabIndex = 8;
            // 
            // StartButton
            // 
            this.StartButton.Enabled = false;
            this.StartButton.Location = new System.Drawing.Point(30, 55);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(164, 33);
            this.StartButton.TabIndex = 9;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(234, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(321, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Versions Available";
            // 
            // controlPanel
            // 
            this.controlPanel.BackColor = System.Drawing.Color.Transparent;
            this.controlPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.controlPanel.Controls.Add(this.label4);
            this.controlPanel.Controls.Add(this.VsixButton);
            this.controlPanel.Controls.Add(this.label3);
            this.controlPanel.Controls.Add(this.label2);
            this.controlPanel.Controls.Add(this.RunnableVersionsComboBox);
            this.controlPanel.Controls.Add(this.SDKUninstall);
            this.controlPanel.Controls.Add(this.versionListBox);
            this.controlPanel.Controls.Add(this.StartButton);
            this.controlPanel.Controls.Add(this.SDKInstall);
            this.controlPanel.Controls.Add(this.label1);
            this.controlPanel.Controls.Add(this.SDKUpgrade);
            this.controlPanel.Location = new System.Drawing.Point(16, 101);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(586, 221);
            this.controlPanel.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.LightGray;
            this.label4.Location = new System.Drawing.Point(30, 151);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(525, 2);
            this.label4.TabIndex = 15;
            this.label4.Text = "label4";
            // 
            // VsixButton
            // 
            this.VsixButton.Location = new System.Drawing.Point(30, 183);
            this.VsixButton.Name = "VsixButton";
            this.VsixButton.Size = new System.Drawing.Size(140, 23);
            this.VsixButton.TabIndex = 14;
            this.VsixButton.Text = "Install";
            this.VsixButton.UseVisualStyleBackColor = true;
            this.VsixButton.Click += new System.EventHandler(this.VsixButton_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(27, 163);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Visual Studio Integration";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(27, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Paradox SDK";
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.BackColor = System.Drawing.Color.Transparent;
            this.closeButton.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Arial", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeButton.ForeColor = System.Drawing.Color.White;
            this.closeButton.Location = new System.Drawing.Point(585, 0);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(29, 21);
            this.closeButton.TabIndex = 12;
            this.closeButton.Text = "X";
            this.closeButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SiliconStudio.LauncherApp.Properties.Resources.splashscreen;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(614, 376);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.minimizeButton);
            this.Controls.Add(this.launcherVersionLabel);
            this.Controls.Add(this.logLabel);
            this.Controls.Add(this.versionLabel);
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(614, 376);
            this.MinimumSize = new System.Drawing.Size(614, 376);
            this.Name = "SplashForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Paradox";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SplashForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SplashForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SplashForm_MouseUp);
            this.controlPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label logLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label launcherVersionLabel;
        private System.Windows.Forms.Button minimizeButton;
        private System.Windows.Forms.ListBox versionListBox;
        private System.Windows.Forms.Button SDKInstall;
        private System.Windows.Forms.Button SDKUpgrade;
        private System.Windows.Forms.Button SDKUninstall;
        private System.Windows.Forms.ComboBox RunnableVersionsComboBox;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button VsixButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button closeButton;
    }
}