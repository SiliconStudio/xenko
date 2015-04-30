// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NuGet;
using SiliconStudio.Assets;

namespace SiliconStudio.LauncherApp
{
    public partial class SplashForm : Form
    {
        private string loadingMessage;
        private int loadingStep;
        private LauncherApp launcher;
        private ConcurrentQueue<NugetLogEventArgs> tempLogEvents;
        private bool isMovingWindow;
        private Point previousMousePosition;
        private readonly Timer timer;

        private bool isInitialized;

        public SplashForm()
        {
            InitializeComponent();
            tempLogEvents = new ConcurrentQueue<NugetLogEventArgs>();
            timer = new Timer() { Interval = 300 };
            launcherVersionLabel.Text = "Launcher Version: " + LauncherApp.Version;
            ExitOnUserClose = true;
            DoubleBuffered = true; // Prevents flickering
        }

        public bool ExitOnUserClose { get; set; }

        public void Initialize(LauncherApp launcher, string defaultLogText = null)
        {
            this.launcher = launcher;
            logLabel.Text = defaultLogText ?? string.Empty;
            versionLabel.Text = string.Empty;
            
            launcher.Loading += launcher_Loading;
            launcher.ProgressAvailable += launcher_ProgressAvailable;
            launcher.LogAvailable += launcher_LogAvailable;
            launcher.SDKVersionListUpdated += launcher_SDKVersionListUpdated;
            launcher.Processing += launcher_processing;
            launcher.Idle += launcher_idle;
            timer.Tick += timer_Tick;
            timer.Start();

            isInitialized = true;
        }

        void launcher_ProgressAvailable(object sender, NuGet.ProgressEventArgs e)
        {
            loadingMessage = string.Format("{0} ({1}%)", e.Operation, e.PercentComplete);

            if (IsDisposed || !launcher.IsDownloading)
            {
                return;
            }

            tempLogEvents.Enqueue(new NugetLogEventArgs(MessageLevel.Info, loadingMessage));
        }

        void launcher_Loading(object sender, LoadingEventArgs e)
        {
            tempLogEvents.Enqueue(new NugetLogEventArgs(MessageLevel.Info, "Loading " + e.Package));
            versionLabel.InvokeSafe(() =>
            {
                versionLabel.Text = "Version: " + e.Version;
            });
        }

        private NugetLogEventArgs lastLog;

        void timer_Tick(object sender, EventArgs e)
        {
            NugetLogEventArgs log;
            while (tempLogEvents.TryDequeue(out log))
            {
                lastLog = log;
            }

            if (lastLog == null)
            {
                return;
            }

            var logMessage = lastLog.Message;
            if (lastLog.Level != MessageLevel.Debug && lastLog.Level != MessageLevel.Info)
            {
                logMessage = lastLog.Level + ": " + logMessage;
            }

            var label = logMessage;
            if (logMessage.IsEmpty()) label = "Ready";
            if (launcher.IsProcessing)
            {
                label = logMessage + string.Concat(Enumerable.Repeat(".", (loadingStep & 3) + 1));
            }
            logLabel.InvokeSafe(() =>
            {
                logLabel.Text = label;
            });
            loadingStep++;
        }

        void launcher_LogAvailable(object sender, NugetLogEventArgs e)
        {
            tempLogEvents.Enqueue(e);
        }

        void launcher_SDKVersionListUpdated(object sender, EventArgs e)
        {
            synchronizeView();
        }

        private void SplashForm_MouseDown(object sender, MouseEventArgs e)
        {
            previousMousePosition = Cursor.Position;
            isMovingWindow = true;
        }

        private void SplashForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMovingWindow)
            {
                var newPosition = Cursor.Position;

                var delta = new Point(newPosition.X - previousMousePosition.X, newPosition.Y - previousMousePosition.Y);
                var newWindowPosition = this.Location;
                newWindowPosition.Offset(delta);
                this.Location = newWindowPosition;

                previousMousePosition = newPosition;
            }

        }

        private void SplashForm_MouseUp(object sender, MouseEventArgs e)
        {
            isMovingWindow = false;
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (ExitOnUserClose && e.CloseReason == CloseReason.UserClosing)
            {
                Environment.Exit(1);
            }

            base.OnFormClosing(e);
        }

        private void versionListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // A version has been selected in the list, update install/upgrade/delete buttons
            synchronizeVersionListButtons();
        }

        public void synchronizeView()
        {
            synchronizeVersionList();
            synchronizeVersionListButtons();
            synchronizeStartDropDown();
            synchronizeVsixButton();
        }

        public void synchronizeVersionList()
        {
            // Populate the list
            versionListBox.InvokeSafe(() =>
            {

                var versionsList = launcher.SdkVersionManager.Versions;

                versionListBox.BeginUpdate();
                versionListBox.Items.Clear();
                foreach (var sdkVersion in versionsList)
                {
                    String label = sdkVersion.Major + "." + sdkVersion.Minor + " ";
                    if (sdkVersion.CanBeInstalled)
                    {
                        label += "(Not Installed)";
                    }
                    else
                    {
                        if (sdkVersion.CanBeUpgraded)
                        {
                            label += "(Upgrade Available to " + sdkVersion.LatestAvailablePackage.Version + ")";
                        }
                        else
                        {
                            label += "(Installed)";
                        }
                    }
                    versionListBox.Items.Add(label);
                }
                versionListBox.EndUpdate();
            });
        }

        public void synchronizeVersionListButtons()
        {
            versionListBox.InvokeSafe(() =>
            {
                int selectedIndex = versionListBox.SelectedIndex;
                bool installEnable = false;
                bool upgradeEnable = false;
                bool uninstallEnable = false;
                var versions = launcher.SdkVersionManager.Versions;
                if (versions != null && selectedIndex >= 0 && selectedIndex < versions.Count)
                {
                    var selectedVersion = versions[selectedIndex];
                    installEnable = selectedVersion.CanBeInstalled;
                    upgradeEnable = selectedVersion.CanBeUpgraded;
                    uninstallEnable = selectedVersion.CanBeUninstalled;
                }

                    SDKInstall.Enabled = installEnable;
                    SDKUpgrade.Enabled = upgradeEnable;
                    SDKUninstall.Enabled = uninstallEnable;

            });
        }

        public void synchronizeStartDropDown()
        {
            // Update the drop-down with the installed version list
            var list = launcher.SdkVersionManager.RunnableVersions;
            RunnableVersionsComboBox.InvokeSafe(() =>
            {
                RunnableVersionsComboBox.BeginUpdate();
                RunnableVersionsComboBox.Items.Clear();

                foreach (var version in list)
                {
                    RunnableVersionsComboBox.Items.Add("Paradox " + version.InstalledPackage.Version);
                }
                RunnableVersionsComboBox.EndUpdate();

                if (list.Count > 0)
                {
                    
                    StartButton.Enabled = true;
                }
                else
                {
                    RunnableVersionsComboBox.Items.Add("No version installed");
                    StartButton.Enabled = false;
                }
                RunnableVersionsComboBox.SelectedIndex = 0;
                RunnableVersionsComboBox.EndUpdate();
            });
        }

        private SDKVersion getSelectedVersion()
        {
            var index = versionListBox.SelectedIndex;
            if (index < 0 || index >= launcher.SdkVersionManager.Versions.Count) return null;
            return launcher.SdkVersionManager.Versions[index];
        }

        private void synchronizeVsixButton()
        {
            VsixButton.InvokeSafe(() =>
            {
                var vsixVersion = launcher.SdkVersionManager.VsixVersion;
                var latestPkg = vsixVersion.LatestAvailablePackage;
                var currentPkg = vsixVersion.InstalledPackage;

                // No version, remote nor local
                if (latestPkg == null)
                {
                    VsixButton.Text = "Install";
                    VsixButton.Enabled = false;
                    return;
                }

                VsixButton.Enabled = true;
                if (currentPkg == null)
                {
                    VsixButton.Text = "Install";
                    return;
                }

                VsixButton.Text = latestPkg.Version > currentPkg.Version ? "Upgrade to " + latestPkg.Version.Version : "Re-Install";
            });
        }

        private void SDKInstall_Click(object sender, EventArgs e)
        {
            // Install the version
            var version = getSelectedVersion();
            if (version == null) return;

            //Install version
            launcher.InstallPackage(version.LatestAvailablePackage, ProposeVSIXInstallIfNecessary);
        }

        // Checks the VSIX status and recommend to install the latest plugin to the user (if available)
        private void ProposeVSIXInstallIfNecessary()
        {
            VsixButton.InvokeSafe(() =>
            {
                var vsixVersion = launcher.SdkVersionManager.VsixVersion;
                var latestPkg = vsixVersion.LatestAvailablePackage;
                var currentPkg = vsixVersion.InstalledPackage;

                if (latestPkg == null)
                {
                    // No VSIX on the server
                    return;
                }

                bool installPossible = false;
                bool upgradePossible = false;

                if (currentPkg == null)
                {
                    installPossible = true;
                }
                else
                {
                    upgradePossible = latestPkg.Version > currentPkg.Version;
                }

                // No install nor upgrade possible
                if (!installPossible && !upgradePossible) return;

                // Propose install
                var confirmResult = MessageBox.Show("We recommend " + (installPossible? "installing": "upgrading") + " the Visual Studio integration for Paradox.\nInstall plugin for Visual Studio? ",
                    "Visual Studio Integration", MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.No) return;

                launcher.InstallVsixPackage(vsixVersion.InstalledPackage, vsixVersion.LatestAvailablePackage);
            });
        }

        private void SDKUpgrade_Click(object sender, EventArgs e)
        {
            var version = getSelectedVersion();
            if (version == null) return;

            launcher.UpgradePackage(version.InstalledPackage, version.LatestAvailablePackage);
        }

        private void SDKUninstall_Click(object sender, EventArgs e)
        {
            var version = getSelectedVersion();
            if (version == null) return;

            // Confirm
            var confirmResult = MessageBox.Show("Are you sure you wish to uninstall " + version.InstalledPackage  + "?",
                "Uninstall", MessageBoxButtons.YesNo);
            
            if (confirmResult == DialogResult.No) return;

            launcher.UninstallPackage(version.InstalledPackage);
        }

        private void VsixButton_Click(object sender, EventArgs e)
        {
            var vsixVersion = launcher.SdkVersionManager.VsixVersion;
            launcher.InstallVsixPackage(vsixVersion.InstalledPackage, vsixVersion.LatestAvailablePackage);
        }

        public void launcher_processing(object sender, EventArgs e)
        {
            versionListBox.InvokeSafe(() =>
            {
                controlPanel.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
            });
        }

        public void launcher_idle(object sender, EventArgs e)
        {
            synchronizeView();
            versionListBox.InvokeSafe(() =>
            {
                controlPanel.Enabled = true;
                this.Cursor = Cursors.Default;
                this.Enabled = true;
            });
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            var procCount = LauncherApp.GetProcessCount();
            if (launcher.IsProcessing)
            {
                var confirmResult = MessageBox.Show("Some background operations are still in progress. Force close? ",
                "Close", MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.No) return;
            }
            Close();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            var list = launcher.SdkVersionManager.RunnableVersions;
            if (list.Count == 0) return;
            var index = RunnableVersionsComboBox.SelectedIndex;
            if (index < 0 || index >= list.Count) return;
            launcher.LaunchSDK(list[index].InstalledPackage);
        }

        

    }
}
