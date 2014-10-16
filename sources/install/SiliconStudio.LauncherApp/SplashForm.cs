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
            timer = new Timer() { Interval = 200 };
            launcherVersionLabel.Text = "Launcher Version: " + LauncherApp.Version;
            ExitOnUserClose = true;
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

            logLabel.Text = logMessage + string.Concat(Enumerable.Repeat(".", (loadingStep & 3) + 1));
            loadingStep++;
        }

        void launcher_LogAvailable(object sender, NugetLogEventArgs e)
        {
            tempLogEvents.Enqueue(e);
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
    }
}
