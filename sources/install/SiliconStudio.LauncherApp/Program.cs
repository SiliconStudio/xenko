// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Windows.Forms;
using SiliconStudio.Assets;
using SiliconStudio.Core.Windows;

namespace SiliconStudio.LauncherApp
{
    class Program
    {
        private const string LaunchAppRestartOption = "/LaunchAppRestart";
        private LauncherApp launcherApp;
        private SplashForm splashscreen;
        private int previousComplete;

        private bool isPostDownloading;
        bool relaunchThisProcess = false;

        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)] // Optimize loading of AppDomain assemblies
        private static void Main()
        {
            // Setup the SiliconStudioParadoxDir to make sure that it is passed to the underlying process (msbuild...etc.)
            Environment.SetEnvironmentVariable("SiliconStudioParadoxDir", Path.GetDirectoryName(typeof(Program).Assembly.Location));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var program = new Program();
            program.RunSafe(() => program.Run(AppHelper.GetCommandLineArgs()));
        }

        private void RunSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                ShowUnhandledException(exception);
            }
        }

        private void ShowUnhandledException(Exception exception)
        {
            var message = AppHelper.BuildErrorToClipboard(exception, string.Format("LauncherApp Version: {0}\n", LauncherApp.Version));
            MessageBox.Show(GetCurrentWindow(), "An unhandled exception has occured (copied to the clipboard) : \n\n" + message, "Launcher error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void Run(string[] args)
        {
            if (args.Length > 0 && args[0] == LaunchAppRestartOption)
            {
                Console.WriteLine("Restart, wait for 200ms");
                Thread.Sleep(200);
                args = args.Skip(1).ToArray();
            }

            using (launcherApp = new LauncherApp())
            {
                launcherApp.DialogAvailable += launcherApp_DialogAvailable;
                launcherApp.UnhandledException += (sender, exception) => ShowUnhandledException(exception);

                var evt = new ManualResetEvent(false);

                var splashThread = new Thread(
                    () =>
                    {
                        splashscreen = new SplashForm();
                        splashscreen.Initialize(launcherApp);
                        splashscreen.Show();

                        launcherApp.MainWindowHandle = splashscreen.Handle;
                        evt.Set();

                        Application.Run(splashscreen);
                        splashscreen = null;
                    });
                splashThread.Start();

                evt.WaitOne();

                launcherApp.ProgressAvailable += launcherApp_ProgressAvailable;
                launcherApp.LogAvailable += launcherApp_LogAvailable;
                var runningForm = splashscreen;
                launcherApp.Running += (sender, eventArgs) => SafeWindowClose(runningForm);

                var result = launcherApp.Run(args);

                // Make sure the splashscreen is closed in case of an error
                if (result != 0)
                {
                    runningForm.ExitOnUserClose = false;
                    SafeWindowClose(runningForm);
                }

                // Reopen the SplashForm if we are still downloading files
                if (launcherApp.IsDownloading)
                {
                    isPostDownloading = true;
                    launcherApp.DownloadFinished += launcherApp_DownloadFinished;

                    splashscreen = new SplashForm();
                    splashscreen.Initialize(launcherApp, "Downloading new version");
                    splashscreen.Show();
                    Application.Run(splashscreen);
                    splashscreen = null;
                }
            }
            launcherApp = null;

            // Relaunch this application if necessary
            if (relaunchThisProcess)
            {
                var newArgs = new List<string>() { LaunchAppRestartOption };
                newArgs.AddRange(args);
                var startInfo = new ProcessStartInfo(typeof(Program).Assembly.Location)
                {
                    Arguments = string.Join(" ", newArgs),
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
        }

        static int GetLauncherAppProcessCount()
        {
            return Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;
        }

        private static void SafeWindowClose(Form form)
        {
            if (form != null)
            {
                form.InvokeSafe(() =>
                {
                    if (!form.IsDisposed)
                    {
                        form.Close();
                    }
                });
            };
        }

        void launcherApp_DownloadFinished(object sender, EventArgs e)
        {
            if (splashscreen != null)
            {
                // Propose to re-launch GameStudio only if this is a new package and there is only a single launcher 
                var launcherCount = GetLauncherAppProcessCount();
                if (launcherApp.IsNewPackageAvailable && isPostDownloading && launcherCount == 1)
                {
                    var result = MessageBox.Show(GetCurrentWindow(), "Do you want to install and run the new version now?", "Launcher", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    relaunchThisProcess = result == DialogResult.Yes;
                }

                var form = splashscreen;
                splashscreen.InvokeSafe(form.Close);
            }
        }

        void launcherApp_DialogAvailable(object sender, DialogEventArgs e)
        {
            e.Result = MessageBox.Show(GetCurrentWindow(), e.Text, e.Caption, e.Buttons, e.Icon, e.DefaultButton, e.Options);
        }

        void launcherApp_ProgressAvailable(object sender, NuGet.ProgressEventArgs e)
        {
            if (previousComplete != e.PercentComplete)
            {
                Console.WriteLine("Download {0} {1}", e.Operation, e.PercentComplete);
            }
            previousComplete = e.PercentComplete;
        }

        static void launcherApp_LogAvailable(object sender, NugetLogEventArgs e)
        {
            Console.WriteLine(e);
        }

        IWin32Window GetCurrentWindow()
        {
            // When debugger is attached, don't try to have a current window for show dialog (bug?)
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return null;
            }

            // Trying to get the main window handle when splashscreen is not displayed is not working?
            var windowHandler = Process.GetCurrentProcess().MainWindowHandle;
            var wnd = windowHandler != IntPtr.Zero ? NativeWindow.FromHandle(windowHandler) : null;

            return splashscreen != null && !splashscreen.IsDisposed ? (IWin32Window)splashscreen : wnd;
        }
    }
}
