using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SiliconStudio.Core;

namespace SiliconStudio.ExecServer
{
    /// <summary>
    /// A AppDomain container for managing shadow copy AppDomain that is working with native dlls.
    /// </summary>
    internal class AppDomainShadow : MarshalByRefObject, IDisposable
    {
        private const string CacheFolder = ".shadow";

        private readonly object singletonLock = new object();

        private readonly string applicationPath;

        private readonly string[] nativeDllsPathOrFolderList;

        private readonly string appDomainName;

        private readonly string mainAssemblyPath;

        private bool isDllImportShadowCopy;

        private readonly AppDomain appDomain;

        private readonly List<FileLoaded> filesLoaded;

        private bool isRunning;

        private bool isUpToDate = true;

        private readonly AssemblyLoaderCallback callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainShadow" /> class.
        /// </summary>
        /// <param name="appDomainName">Name of the application domain.</param>
        /// <param name="mainAssemblyPath">The main assembly path.</param>
        /// <param name="nativeDllsPathOrFolderList">An array of folders path (containing only native dlls) or directly a specific path to a dll.</param>
        /// <exception cref="System.ArgumentNullException">mainAssemblyPath</exception>
        /// <exception cref="System.InvalidOperationException">If the assembly does not exist</exception>
        public AppDomainShadow(string appDomainName, string mainAssemblyPath, params string[] nativeDllsPathOrFolderList)
        {
            if (mainAssemblyPath == null) throw new ArgumentNullException("mainAssemblyPath");
            if (nativeDllsPathOrFolderList == null) throw new ArgumentNullException("nativeDllsPathOrFolderList");
            if (!File.Exists(mainAssemblyPath)) throw new InvalidOperationException(string.Format("Assembly [{0}] does not exist", mainAssemblyPath));

            this.appDomainName = appDomainName;
            this.mainAssemblyPath = mainAssemblyPath;
            this.nativeDllsPathOrFolderList = nativeDllsPathOrFolderList;
            applicationPath = Path.GetDirectoryName(mainAssemblyPath);
            filesLoaded = new List<FileLoaded>();
            appDomain = CreateAppDomain(appDomainName);

            callback = new AssemblyLoaderCallback(AssemblyLoaded);
            appDomain.DoCallBack(callback.RegisterAssemblyLoad);

            Console.WriteLine("AppDomain {0} Created", appDomainName);
        }

        /// <summary>
        /// Gets the application domain managed by this container.
        /// </summary>
        /// <value>The application domain.</value>
        public AppDomain AppDomain
        {
            get
            {
                return appDomain;
            }
        }

        /// <summary>
        /// Tries to take the ownership of this container to run an exe/method from the app domain.
        /// </summary>
        /// <returns><c>true</c> if ownership was successfull (you can then use <see cref="Run"/> method), <c>false</c> otherwise.</returns>
        public bool TryLock()
        {
            bool result;
            lock (singletonLock)
            {
                if (!isRunning)
                {
                    isRunning = true;
                }
                result = isRunning;
            }
            return result;
        }

        /// <summary>
        /// Determines whether all assemblies and native dlls have not changed.
        /// </summary>
        /// <returns><c>true</c> if the appdomain is up-to-date; otherwise, <c>false</c>.</returns>
        public bool IsUpToDate()
        {
            if (isUpToDate)
            {
                var filesToCheck = new List<FileLoaded>();
                lock (filesLoaded)
                {
                    filesToCheck.AddRange(filesLoaded);
                }

                foreach (var fileLoaded in filesToCheck)
                {
                    if (!fileLoaded.IsUpToDate())
                    {
                        Console.WriteLine("Dll File changed: {0}", fileLoaded.filePath);

                        isUpToDate = false;
                        break;
                    }
                }
            }

            return isUpToDate;
        }

        /// <summary>
        /// Runs the main entry point method passing arguments to it
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.InvalidOperationException">Must call TryLock before calling this method</exception>
        public int Run(string[] args)
        {
            if (!isRunning)
            {
                throw new InvalidOperationException("Must call TryLock before calling this method");
            }

            try
            {
                var result = appDomain.ExecuteAssembly(mainAssemblyPath, args);
                Console.WriteLine("Return result: {0}", result);
                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unexpected exception: {0}", exception);
                // TODO: Unexpected exception, close this appdomain?
                return 1;
            }
        }

        public void EndRun()
        {
            lock (singletonLock)
            {
                isRunning = false;
            }
        }

        private void AssemblyLoaded(string location)
        {
            if (!isDllImportShadowCopy)
            {
                ShadowCopyNativeDlls(location);
                isDllImportShadowCopy = true;
            }

            // Register the assembly in order to unload this appdomain if it is no longer relevant
            var assemblyFileName = Path.GetFileName(location);
            RegisterFileLoaded(new FileInfo(Path.Combine(applicationPath, assemblyFileName)));
        }

        private void ShadowCopyNativeDlls(string currentAssemblyPath)
        {
            // In this method, we copy all native dlls to a subfolder under the shadow cache
            // Each dll has a hash computed from its name and last timestamp
            // This hash is used to create a directory from which the dlls will be stored
            // Later in the AppDomain running and use the NativeLibrary.PreLoadLibrary()
            // The method in PreLoadLibrary will use the dll that have been copied by this instance

            // Get the shadow folder for native dlls
            var rootShadow = GetRootCachePath(currentAssemblyPath).FullName;
            var nativeDllShadowRootFolder = Path.Combine(rootShadow, "native");
            Directory.CreateDirectory(nativeDllShadowRootFolder);

            // Copy check any new native dlls
            var appPath = Path.GetDirectoryName(mainAssemblyPath);

            foreach (var nativeDllFolderOrPath in nativeDllsPathOrFolderList)
            {
                var absolutePathOrFolder = Path.Combine(appPath, nativeDllFolderOrPath);

                // Native dll files to load
                var files = File.Exists(absolutePathOrFolder) ? 
                    new[] { new FileInfo(absolutePathOrFolder) } : 
                    new DirectoryInfo(absolutePathOrFolder).EnumerateFiles("*.dll");

                var hashBuffer = new MemoryStream(new byte[1024]);
                foreach (var file in files)
                {
                    var fileHash = GetFileHash(hashBuffer, file);
                    var nativeDllPath = Path.Combine(nativeDllShadowRootFolder, fileHash, file.Name);
                    if (!File.Exists(nativeDllPath))
                    {
                        SafeCopy(file.FullName, nativeDllPath);
                    }

                    // Register our native path
                    NativeLibraryInternal.SetShadowPathForNativeDll(appDomain, file.Name, Path.GetDirectoryName(nativeDllPath));

                    // Register this dll 
                    RegisterFileLoaded(file);
                }
            }
        }

        private DirectoryInfo GetRootCachePath(string currentPath)
        {
            var info = new DirectoryInfo(currentPath);
            while (info != null)
            {
                if (String.Compare(info.Name, "dl3", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return info;
                    break;
                }
                info = info.Parent;
            }
            throw new InvalidOperationException(String.Format("Unexpected cache layout. Expecting dl3 folder from [{0}]", currentPath));
        }

        private static string Hash(byte[] buffer)
        {
            uint hash = 2166136261;
            for (int i = 0; i < buffer.Length; i++)
            {
                hash = hash ^ buffer[i];
                hash = hash * 16777619;
            }
            return hash.ToString("x");
        }

        private static string GetFileHash(MemoryStream hashBuffer, FileInfo file)
        {
            hashBuffer.Position = 0;
            var nameAsBytes = Encoding.UTF8.GetBytes(file.Name);
            hashBuffer.Write(nameAsBytes, 0, nameAsBytes.Length);
            var timeAsBytes = BitConverter.GetBytes(file.LastWriteTimeUtc.Ticks);
            hashBuffer.Write(timeAsBytes, 0, timeAsBytes.Length);
            return Hash(hashBuffer.ToArray());
        }

        private void RegisterFileLoaded(FileInfo file)
        {
            lock (filesLoaded)
            {
                filesLoaded.Add(new FileLoaded(file));
            }
        }

        private static void SafeCopy(string sourceFilePath, string destinationFilePath)
        {
            var fileName = Path.GetFileName(sourceFilePath);

            var directory = Path.GetDirectoryName(destinationFilePath);
            var parentDirectory = Directory.GetParent(directory).FullName;
            var tempPath = Path.Combine(parentDirectory, Guid.NewGuid().ToString());
            var tempDir = new DirectoryInfo(tempPath);
            Directory.CreateDirectory(tempPath);
            bool tempDirDeleted = false;
            try
            {
                File.Copy(sourceFilePath, Path.Combine(tempPath, fileName));
                try
                {
                    tempDir.MoveTo(directory);
                    tempDirDeleted = true;
                }
                catch (IOException)
                {
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                if (!tempDirDeleted)
                {
                    try
                    {
                        tempDir.Delete(true);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private AppDomain CreateAppDomain(string appDomainName)
        {
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = applicationPath,
                ShadowCopyFiles = "true",
                CachePath = Path.Combine(applicationPath, CacheFolder),
            };

            return AppDomain.CreateDomain(appDomainName, AppDomain.CurrentDomain.Evidence, appDomainSetup);
        }

        private struct FileLoaded
        {
            public FileLoaded(FileInfo file)
            {
                filePath = file.FullName;
                lastWriteTime = file.LastWriteTimeUtc;
            }

            public readonly string filePath;

            private readonly DateTime lastWriteTime;

            public bool IsUpToDate()
            {
                try
                {
                    var currentTime = new FileInfo(filePath).LastWriteTimeUtc;
                    return currentTime == lastWriteTime;
                }
                catch (IOException)
                {
                }
                return false;
            }
        }

        [Serializable]
        private class AssemblyLoaderCallback
        {
            private readonly Action<string> callback;

            public AssemblyLoaderCallback(Action<string> callback)
            {
                this.callback = callback;
            }

            public void RegisterAssemblyLoad()
            {
                // This method is executed in the child application domain
                AppDomain.CurrentDomain.AssemblyLoad += AppDomainOnAssemblyLoad;
            }

            private void AppDomainOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
            {
                var assembly = args.LoadedAssembly;
                if (!assembly.IsDynamic)
                {
                    // This method will be executed in the ExecServer application domain
                    callback(assembly.Location);
                }
            }
        }

        public void Dispose()
        {
            System.AppDomain.Unload(appDomain);
            Console.WriteLine("AppDomain {0} Disposed", appDomainName);
        }
    }
}