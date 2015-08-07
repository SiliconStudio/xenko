using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SiliconStudio.ExecServer
{
    /// <summary>
    /// Manages <see cref="AppDomainShadow"/>.
    /// </summary>
    internal class AppDomainShadowManager : IDisposable
    {
        private readonly List<AppDomainShadow> appDomainShadows = new List<AppDomainShadow>();

        private readonly string mainAssemblyPath;

        private readonly List<string> nativeDllsPathOrFolderList;

        // We don't use the pool of AppDomain as we need to cleanup resource cleaning in CompilerAsset, so just recreate the AppDomain everytime and Unload it for now.
        private readonly bool useCaching;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainShadowManager"/> class.
        /// </summary>
        /// <param name="mainAssemblyPath">The main assembly path.</param>
        /// <param name="nativeDllsPathOrFolderList">An array of folders path (containing only native dlls) or directly a specific path to a dll.</param>
        /// <exception cref="System.ArgumentNullException">mainAssemblyPath</exception>
        /// <exception cref="System.InvalidOperationException">If the assembly does not exist</exception>
        public AppDomainShadowManager(string mainAssemblyPath, params string[] nativeDllsPathOrFolderList)
        {
            if (mainAssemblyPath == null) throw new ArgumentNullException("mainAssemblyPath");
            if (!File.Exists(mainAssemblyPath)) throw new InvalidOperationException(string.Format("Assembly [{0}] does not exist", mainAssemblyPath));
            this.mainAssemblyPath = mainAssemblyPath;
            this.nativeDllsPathOrFolderList = new List<string>(nativeDllsPathOrFolderList);
            useCaching = Environment.GetEnvironmentVariable("ParadoxAssetCompilerEnableCaching") == "true";
        }

        /// <summary>
        /// Runs the assembly with the specified arguments.xit
        /// </summary>
        /// <param name="args">The main arguments.</param>
        /// <returns>System.Int32.</returns>
        public int Run(string[] args)
        {
            AppDomainShadow shadowDomain = null;


            try
            {
                shadowDomain = GetOrNew(useCaching);
                return shadowDomain.Run(args);
            }
            finally
            {
                if (shadowDomain != null)
                {
                    shadowDomain.EndRun();
                    if (!useCaching)
                    {
                        shadowDomain.Dispose();
                    }
                }
                GC.Collect(2);
            }
        }

        /// <summary>
        /// Recycles any instance that are no longer in sync with original dlls
        /// </summary>
        public void Recycle()
        {
            lock (appDomainShadows)
            {
                for (int i = appDomainShadows.Count - 1; i >= 0; i--)
                {
                    var appDomainShadow = appDomainShadows[i];
                    if (!appDomainShadow.IsUpToDate())
                    {
                        // Try to take the lock on the appdomain to dispose (may be running)
                        if (appDomainShadow.TryLock())
                        {
                            Console.WriteLine("Recycling AppDomain {0}", appDomainShadow.AppDomain.FriendlyName);
                            appDomainShadow.Dispose();
                            appDomainShadows.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get or create a new <see cref="AppDomainShadow"/>.
        /// </summary>
        /// <returns></returns>
        private AppDomainShadow GetOrNew(bool useCache)
        {
            var appDomainName = Path.GetFileNameWithoutExtension(mainAssemblyPath) + "#" + appDomainShadows.Count;
            if (useCache)
            {
                lock (appDomainShadows)
                {
                    Console.WriteLine("Cached AppDomains #{0}", appDomainShadows.Count);

                    foreach (var appDomainShadow in appDomainShadows)
                    {
                        if (appDomainShadow.TryLock())
                        {
                            return appDomainShadow;
                        }
                    }

                    var newAppDomain = new AppDomainShadow(appDomainName, mainAssemblyPath, nativeDllsPathOrFolderList.ToArray());
                    newAppDomain.TryLock();
                    appDomainShadows.Add(newAppDomain);
                    return newAppDomain;
                }
            }
            else
            {
                var newAppDomain = new AppDomainShadow(appDomainName, mainAssemblyPath, nativeDllsPathOrFolderList.ToArray());
                newAppDomain.TryLock();
                return newAppDomain;
            }
        }

        /// <summary>
        /// Dispose the manager and wait that all app domain are finished.
        /// </summary>
        public void Dispose()
        {
            lock (appDomainShadows)
            {
                while (true)
                {
                    for (int i = appDomainShadows.Count - 1; i >= 0; i--)
                    {
                        var appDomainShadow = appDomainShadows[i];
                        if (appDomainShadow.TryLock())
                        {
                            appDomainShadows.RemoveAt(i);
                            appDomainShadow.Dispose();
                        }
                    }
                    if (appDomainShadows.Count == 0)
                    {
                        break;
                    }

                    // Active wait, not ideal, we should better have an event based locking mechanism
                    Thread.Sleep(200);
                }
            }
        }
    }
}