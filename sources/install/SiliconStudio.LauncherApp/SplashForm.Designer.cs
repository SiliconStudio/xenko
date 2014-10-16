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
            this.launcherVersionLabel.Location = new System.Drawing.Point(16, 313);
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
            this.minimizeButton.Location = new System.Drawing.Point(590, 0);
            this.minimizeButton.Name = "minimizeButton";
            this.minimizeButton.Size = new System.Drawing.Size(26, 21);
            this.minimizeButton.TabIndex = 3;
            this.minimizeButton.Text = "-";
            this.minimizeButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.minimizeButton.UseVisualStyleBackColor = false;
            this.minimizeButton.Click += new System.EventHandler(this.minimizeButton_Click);
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SiliconStudio.LauncherApp.Properties.Resources.splashscreen;
            this.ClientSize = new System.Drawing.Size(614, 376);
            this.Controls.Add(this.minimizeButton);
            this.Controls.Add(this.launcherVersionLabel);
            this.Controls.Add(this.logLabel);
            this.Controls.Add(this.versionLabel);
            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label logLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label launcherVersionLabel;
        private System.Windows.Forms.Button minimizeButton;
    }
}