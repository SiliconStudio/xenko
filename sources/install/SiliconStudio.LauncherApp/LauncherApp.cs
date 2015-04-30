// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NuGet;
using SiliconStudio.Assets;

namespace SiliconStudio.LauncherApp
{
    public class LauncherApp : IDisposable, IProgressProvider, ILogger
    {
        private const string MainExecutableKey = "mainExecutable";

        private const string LauncherAppCallbackParam = "LauncherAppCallback";
        private readonly NugetStore store;
        private readonly string mainPackage;
        private readonly string mainExecutable;
        private readonly string vsixPackage;
        private bool isSynchronous = false;
        private readonly List<Thread> downloadThreads;
        private readonly Stopwatch clock;
        private int maxPercentage;
        private bool isInNegativeMode; // workaround for download progression

        private bool isDownloading;

        public SDKVersionManager SdkVersionManager { get; set; }

        public bool IsSelfUpdated { get; private set; }

        public bool IsNewPackageAvailable { get; private set; }

        public IntPtr MainWindowHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is diagnostic mode (all logs redirected to a file)
        /// </summary>
        /// <value><c>true</c> if this instance is diagnostic mode; otherwise, <c>false</c>.</value>
        public bool IsDiagnosticMode { get; set; }

        public event EventHandler<ProgressEventArgs> ProgressAvailable;

        internal event EventHandler<NugetLogEventArgs> LogAvailable;

        internal event EventHandler<EventArgs> SDKVersionListUpdated;

        public event EventHandler<LoadingEventArgs> Loading;

        public event EventHandler<EventArgs> DownloadFinished;

        public event EventHandler<DialogEventArgs> DialogAvailable;

        public event EventHandler<Exception> UnhandledException;

        public event EventHandler<EventArgs> Running;

        public event EventHandler<EventArgs> Processing;

        public event EventHandler<EventArgs> Idle;

        public bool IsDownloading
        {
            get
            {
                return isDownloading;
            }
            set
            {
                var previousValue = isDownloading;
                isDownloading = value;
                if (previousValue && !isDownloading)
                {
                    OnDownloadFinished();
                }
            }
        }

        public bool IsProcessing { get; set; }

        public static readonly string Version;

        static LauncherApp()
        {
            var assembly = typeof(Program).Assembly;

            var assemblyInformationalVersion = CustomAttributeProviderExtensions.GetCustomAttribute<AssemblyInformationalVersionAttribute>(assembly);
            Version = assemblyInformationalVersion.InformationalVersion;
        }

        public LauncherApp()
        {
            clock = Stopwatch.StartNew();
            IsProcessing = false;

            // TODO: Add a way to clear the cache more othen than the default nuget (>= 200 files)

            // Check config file
            DebugStep("Load store");

            // Get the package name and executable to launch/update
            var thisExeDirectory = Path.GetDirectoryName(typeof(LauncherApp).Assembly.Location);

            store = new NugetStore(thisExeDirectory);
            store.Manager.Logger = this;
            store.SourceRepository.Logger = this;

            mainPackage = store.MainPackageId;
            vsixPackage = store.VSIXPluginId;

            SdkVersionManager = new SDKVersionManager(store.Manager, mainPackage, vsixPackage);

            mainExecutable = store.Settings.GetConfigValue(MainExecutableKey);
            if (string.IsNullOrWhiteSpace(mainExecutable))
            {
                throw new LauncherAppException("Invalid configuration. Expecting [{0}] in config", MainExecutableKey);
            }

            var aggregateRepo = (AggregateRepository)store.Manager.SourceRepository;
            foreach (var repo in aggregateRepo.Repositories)
            {
                var progressProvider = repo as IProgressProvider;
                if (progressProvider != null)
                {
                    progressProvider.ProgressAvailable += OnProgressAvailable;
                }
            }

            downloadThreads = new List<Thread>();

            // Update the targets everytime the launcher is used in order to make sure targets are up-to-date
            // with packages installed (rewrite for example after a self-update)
            store.UpdateTargets();
        }

        public void Dispose()
        {
            if (downloadThreads.Count > 0)
            {
                foreach (var downloadThread in downloadThreads)
                {
                    DebugStep("Waiting for thread {0} to terminate");
                    downloadThread.Join();
                }
                downloadThreads.Clear();
            }
        }


        public int Run(string[] args)
        {
            // Start self update
            downloadThreads.Add(RunThread(SelfUpdate));
            DebugStep("SelfUpdate launched");

            // Scan the local installed packages
            IsProcessing = true;
            Info("Retrieving packages");
            SdkVersionManager.fetchLocal();
            SdkVersionManager.synchronizeData();
            OnSDKVersionsUpdated();
            DebugStep("Local Packages");

            // Scan the remote package and update the view
            SdkVersionManager.fetchServer();
            SdkVersionManager.synchronizeData();
            OnSDKVersionsUpdated();
            IsProcessing = false;
            if (!SdkVersionManager.serverPackages.IsEmpty()) Info("");
            DebugStep("Get all server packages");

            return 0;
        }

        protected virtual void OnRunning()
        {
            EventHandler<EventArgs> handler = Running;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        static public int GetProcessCount()
        {
            var currentModule = Assembly.GetAssembly(typeof(LauncherApp));
            return Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)
                          .Count(x => string.Equals(x.MainModule.FileName, currentModule.Location, StringComparison.OrdinalIgnoreCase));
        }

        private string GetMainExecutable(IPackage package)
        {
            var packagePath = store.PathResolver.GetInstallPath(package);
            return Path.Combine(packagePath, mainExecutable);
        }

        private IPackage FindLatestInstalledPackage(IPackageRepository packageRepository)
        {
            return packageRepository.FindPackagesById(mainPackage).OrderByDescending(p => p.Version).FirstOrDefault();
        }

        private IPackage FindPackageUpdate(IPackage previousPackage)
        {
            return store.Manager.SourceRepository.GetUpdates(new[] { previousPackage }, true, false).FirstOrDefault();
        }

        private IQueryable<IPackage> GetServerPackageList()
        {
            return store.Manager.SourceRepository.GetPackages();
        }

        // Not used anymore
        private void PackageUpdate(IPackage package, bool putInMachineCache)
        {
            //var latestPackages = packages.Where(p => p.IsLatestVersion);
            DebugStep("Find Package Update");
            var newPackage = FindPackageUpdate(package);

            if (newPackage != null && newPackage.Version > package.Version)
            {
                DebugStep("Package Update Found {0} {1}", newPackage.Id, newPackage.Version);

                if (putInMachineCache)
                {
                    IsDownloading = true;
                    IsNewPackageAvailable = true;
                    ShowInformationDialog("A new version " + newPackage.Version + " of " + mainPackage + @" is available.

The download will start in the background.

The new version will be available on next run after all GameStudio are closed");

                    MachineCache.Default.AddPackage(newPackage);

                    // Notfy that the download is finished
                    IsDownloading = false;
                }
                else
                {
                    store.UpdatePackage(newPackage);
                }
            }            
        }

        public Boolean isSafeToProcess()
        {
            var processCount = GetProcessCount();
            bool isSafeToUpdate = processCount <= 1;

            if (isSafeToUpdate) return true;

            ShowInformationDialog(
                string.Format("There are [{0}] instances currently running.\n\nClose all your applications and restart.", processCount));

            return false;
        }

        // Install a package
        public void InstallPackage(IPackage package, Action onComplete = null)
        {
            downloadThreads.Add(RunThread(() =>
            {
                if (!isSafeToProcess()) return;
                Info("Preparing Installation");
                OnProcessing();
                IsDownloading = true;
                try
                {
                    store.InstallPackage(package.Id, package.Version);
                }
                catch (Exception)
                {
                    ShowErrorDialog("Could not retrieve package!");
                }
                IsDownloading = false;
                SdkVersionManager.synchronizeData();
                OnSDKVersionsUpdated();
                Info("");
                OnIdle();
                if (onComplete != null) onComplete();
            }));
        }

        // Uninstall a package
        public void UninstallPackage(IPackage package, Action onComplete = null)
        {
            downloadThreads.Add(RunThread(() =>
            {
                if (!isSafeToProcess()) return;
                Info("Preparing Uninstallation");
                OnProcessing();
                store.UninstallPackage(package);
                SdkVersionManager.synchronizeData();
                OnSDKVersionsUpdated();
                Info("");
                OnIdle();
                if (onComplete != null) onComplete();
            }));
        }

        // Upgrade a package
        public void UpgradePackage(IPackage currentPackage, IPackage newPackage, Action onComplete = null)
        {
            downloadThreads.Add(RunThread(() =>
            {
                if (!isSafeToProcess()) return;
                Info("Preparing Upgrade");
                OnProcessing();
                try
                {
                    store.UninstallPackage(currentPackage);
                    IsDownloading = true;
                    store.InstallPackage(currentPackage.Id, newPackage.Version);
                }
                catch (Exception)
                {
                    ShowErrorDialog("Could not retrieve package!");
                }
                finally
                {
                    IsDownloading = false;
                }
                SdkVersionManager.synchronizeData();
                OnSDKVersionsUpdated();
                Info("");
                OnIdle();
                if (onComplete != null) onComplete();
            }));
        }

        // Install / Upgrade / Re-install the VSIX plugin
        public void InstallVsixPackage(IPackage currentPackage, IPackage newPackage, Action onComplete = null)
        {
            downloadThreads.Add(RunThread(() =>
            {
                if (!isSafeToProcess()) return;
                Info("Preparing VSIX Upgrade");
                OnProcessing();
                try
                {
                    if (currentPackage != null) store.UninstallPackage(currentPackage);
                    IsDownloading = true;
                    store.InstallPackage(newPackage.Id, newPackage.Version);
                    IsDownloading = false;
                    // Now installs the plugin
                    Info("Installing plugin for Visual Studio");
                    store.InstallVsix(newPackage);
                }
                catch (Exception)
                {
                    ShowErrorDialog("VSIX package cannot be retrieved!");
                }
                finally
                {
                    IsDownloading = false;
                }
                SdkVersionManager.synchronizeData();
                OnSDKVersionsUpdated();
                Info("");
                OnIdle();
                if (onComplete != null) onComplete();
            }));
        }

        // Run the game studio of a specific version
        public void LaunchSDK(IPackage package)
        {
            downloadThreads.Add(RunThread(() =>
            {
                Info("Launching " + package);
                OnProcessing();

                // Load the assembly and call the default entry point:
                var fullExePath = GetMainExecutable(package);

                try
                {
                    Process.Start(fullExePath);
                }
                catch (Exception)
                {
                }

                // Visual feedback: gives a few seconds to the editor to start
                Thread.Sleep(3000); 
                Info("");
                OnIdle();
            }));
            
        }

        private Thread RunThread(ThreadStart threadStart)
        {
            // Start self update
            if (isSynchronous)
            {
                threadStart();
                return null;
            }
            else
            {
                var thread = new Thread(
                    () =>
                    {
                        try
                        {
                            threadStart();
                        }
                        catch (Exception exception)
                        {
                            OnUnhandledException(exception);
                        }
                    }) { IsBackground = true };
                thread.Start();
                return thread;
            }
        }

        private void SelfUpdate()
        {
            var version = new SemanticVersion(Version);
            var productAttribute = CustomAttributeProviderExtensions.GetCustomAttribute<AssemblyProductAttribute>(typeof(LauncherApp).Assembly);
            var packageId = productAttribute.Product;

            var package = store.Manager.SourceRepository.GetUpdates(new[] { new PackageName(packageId, version) }, true, false).FirstOrDefault();

            // Check to see if an update is needed
            if (package != null && version < package.Version)
            {
                var movedFiles = new List<string>();

                // Copy files from tools\ to the current directory
                var inputFiles = package.GetFiles();
                const string directoryRoot = "tools\\"; // Important!: this is matching where files are store in the nuspec
                foreach (var file in inputFiles.Where(file => file.Path.StartsWith(directoryRoot)))
                {
                    var fileName =  Path.Combine(store.RootDirectory, file.Path.Substring(directoryRoot.Length));

                    // Move previous files to .old
                    string renamedPath = fileName + ".old";

                    try
                    {
                        if (File.Exists(fileName))
                        {
                            Move(fileName, renamedPath);
                            movedFiles.Add(fileName);
                        }

                        // Update the file
                        UpdateFile(fileName, file);
                    }
                    catch (Exception)
                    {
                        // Revert all olds files if a file didn't work well
                        foreach (var oldFile in movedFiles)
                        {
                            renamedPath = oldFile + ".old";
                            Move(renamedPath, oldFile);
                        }
                        return;
                    }
                }


                // Remove .old files
                foreach (var oldFile in movedFiles)
                {
                    try
                    {
                        var renamedPath = oldFile + ".old";

                        if (File.Exists(renamedPath))
                        {
                            File.Delete(renamedPath);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }


                IsSelfUpdated = true;
            }
        }

        private static void EnsureDirectory(string filePath)
        {
            // Create dest directory if it exists
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void UpdateFile(string newFilePath, IPackageFile file)
        {
            EnsureDirectory(newFilePath);
            using (Stream fromStream = file.GetStream(), toStream = File.Create(newFilePath))
            {
                fromStream.CopyTo(toStream);
            }
        }

        private static void Move(string oldPath, string newPath)
        {
            EnsureDirectory(newPath);
            try
            {
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }
            }
            catch (FileNotFoundException)
            {

            }

            File.Move(oldPath, newPath);
        }

        private void OnProgressAvailable(object sender, ProgressEventArgs e)
        {

            // Bug with nuget? At some point, the percent complete revert to negative going downward
            var percentComplete = e.PercentComplete;
            if (percentComplete > 0 && !isInNegativeMode)
            {
                maxPercentage = percentComplete;
            }

            if (percentComplete < 0 || isInNegativeMode)
            {
                isInNegativeMode = true;
                percentComplete = maxPercentage * 2 + percentComplete + 1;
            }

            // clamp
            percentComplete = percentComplete < 0 ? 0 : percentComplete > 100 ? 100 : percentComplete;

            e = new ProgressEventArgs(e.Operation, percentComplete);

            var handler = ProgressAvailable;
            if (handler != null) handler(sender, e);
        }

        FileConflictResolution IFileConflictResolver.ResolveFileConflict(string message)
        {
            // TODO handle ignore
            return FileConflictResolution.Ignore;
        }

        void ILogger.Log(MessageLevel level, string message, params object[] args)
        {
            OnLogAvailable(new NugetLogEventArgs(level, message, args));
        }

        private void Info(string message)
        {
            OnLogAvailable(new NugetLogEventArgs(MessageLevel.Info, message));
        }

        private void Info(string message, params object[] args)
        {
            OnLogAvailable(new NugetLogEventArgs(MessageLevel.Info, message, args));
        }

        private void Error(string message)
        {
            OnLogAvailable(new NugetLogEventArgs(MessageLevel.Error, message));
        }

        private void Error(string message, params object[] args)
        {
            OnLogAvailable(new NugetLogEventArgs(MessageLevel.Error, message, args));
        }

        private void OnLogAvailable(NugetLogEventArgs e)
        {
            var handler = LogAvailable;
            if (handler != null) handler(this, e);
        }

        private void OnLoading(LoadingEventArgs e)
        {
            var handler = Loading;
            if (handler != null) handler(this, e);
        }

        private void OnSDKVersionsUpdated()
        {
            var handler = SDKVersionListUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void DebugStep(string step)
        {
            Console.WriteLine("Step {0} ({1}ms)", step, clock.ElapsedMilliseconds);
        }
        private void DebugStep(string step, params object[] args)
        {
            DebugStep(string.Format(step, args));
        }

        private DialogResult ShowQuestionDialog(string text)
        {
            var arg = new DialogEventArgs(text, "Launcher information", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
            OnDialogAvailable(arg);
            return arg.Result;
        }

        private DialogResult ShowInformationDialog(string text)
        {
            var arg = new DialogEventArgs(text, "Launcher information", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
            OnDialogAvailable(arg);
            return arg.Result;
        }

        private void ShowErrorDialog(string text)
        {
            Error(text);
            OnDialogAvailable(new DialogEventArgs(text, "Error in Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0));
        }

        private void OnDialogAvailable(DialogEventArgs e)
        {
            var handler = DialogAvailable;
            if (handler != null) handler(this, e);
        }

        private void OnDownloadFinished()
        {
            EventHandler<EventArgs> handler = DownloadFinished;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnProcessing()
        {
            IsProcessing = true;
            EventHandler<EventArgs> handler = Processing;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnIdle()
        {
            IsProcessing = false;
            EventHandler<EventArgs> handler = Idle;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnUnhandledException(Exception e)
        {
            EventHandler<Exception> handler = UnhandledException;
            if (handler != null) handler(this, e);
        }
    }
}