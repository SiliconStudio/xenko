using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SiliconStudio.Assets;
using SiliconStudio.Paradox.GameStudio.Plugin.Debugging;

namespace SiliconStudio.Paradox.ConnectionRouter
{
    public static class RouterHelper
    {
        /// <summary>
        /// The mutex to check if a process containing <see cref="Router"/> has been launched.
        /// </summary>
        public static Mutex RouterMutex = new Mutex(false, "SiliconStudioParadoxRouter");

        /// <summary>
        /// Gets the paradox SDK dir.
        /// </summary>
        /// <param name="paradoxVersion">The paradox version. If null, it will get latest version.</param>
        /// <returns></returns>
        public static string FindParadoxSdkDir(string paradoxVersion = null)
        {
            // TODO: Almost duplicate of ParadoxCommandsProxy.FindParadoxSdkDir!!
            // TODO: Maybe move it in some common class somewhere? (in this case it would be included with "Add as link" in VSPackage)
            var paradoxSdkDir = DirectoryHelper.GetInstallationDirectory("Paradox");

            if (paradoxSdkDir == null)
            {
                paradoxSdkDir = Environment.GetEnvironmentVariable("SiliconStudioParadoxDir");
            }

            if (paradoxSdkDir == null)
            {
                return null;
            }

            // Check if it is a dev directory
            if (File.Exists(Path.Combine(paradoxSdkDir, "build\\Paradox.sln")))
                return paradoxSdkDir;

            // Check if we are in a root directory with store/packages facilities
            if (NugetStore.IsStoreDirectory(paradoxSdkDir))
            {
                var store = new NugetStore(paradoxSdkDir);

                var paradoxPackages = store.GetPackagesInstalled(store.MainPackageId);
                var paradoxPackage = paradoxVersion != null
                    ? (paradoxPackages.FirstOrDefault(p => p.Version.ToString() == paradoxVersion)
                        ?? paradoxPackages.FirstOrDefault(p => VersionWithoutSpecialPart(p.Version.ToString()) == VersionWithoutSpecialPart(paradoxVersion))) // If no exact match, try a second time without the special version tag (beta, alpha, etc...)
                    : paradoxPackages.FirstOrDefault();
                if (paradoxPackage == null)
                    return null;

                var packageDirectory = store.PathResolver.GetPackageDirectory(paradoxPackage);
                return Path.Combine(paradoxSdkDir, store.RepositoryPath, packageDirectory);
            }

            return null;
        }

        private static string VersionWithoutSpecialPart(string version)
        {
            var indexOfDash = version.IndexOf('-');
            if (indexOfDash == -1)
                return version;

            return version.Substring(0, indexOfDash);
        }

        public static bool EnsureRouterLaunched(bool attachChildJob = false)
        {
            try
            {
                // Try to connect to router
                FileVersionInfo runningRouterVersion = null;
                Process runningRouterProcess = null;
                foreach (var process in Process.GetProcessesByName("SiliconStudio.Paradox.ConnectionRouter"))
                {
                    try
                    {
                        runningRouterVersion = process.MainModule.FileVersionInfo;
                        runningRouterProcess = process;
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                var routerAssemblyLocation = typeof(Router).Assembly.Location;
                var routerAssemblyExe = Path.GetFileName(routerAssemblyLocation);

                // Find latest paradox
                var paradoxSdkDir = FindParadoxSdkDir();
                if (paradoxSdkDir == null)
                {
                    throw new FileNotFoundException("Could not find Paradox Sdk Dir");
                }

                var paradoxSdkBinDir = Path.Combine(paradoxSdkDir, @"Bin\Windows-Direct3D11");

                var routerAssemblyFile = Path.Combine(paradoxSdkBinDir, routerAssemblyExe);

                if (!File.Exists(routerAssemblyLocation))
                {
                    // Should we allow it to continue if there is an existing router? (routerVersion != null)
                    throw new FileNotFoundException("Could not find Paradox Connection Router executable");
                }

                // If already started, check if found version is better
                if (runningRouterVersion != null)
                {
                    var routerAssemblyFileVersionInfo = FileVersionInfo.GetVersionInfo(routerAssemblyLocation);

                    // Check that current router is at least as good as the one of latest found Paradox
                    if (new Version(routerAssemblyFileVersionInfo.FileVersion) <= new Version(runningRouterVersion.FileVersion))
                        return true;
                }

                // Kill previous router process (if any)
                if (runningRouterProcess != null)
                {
                    runningRouterProcess.Kill();
                    runningRouterProcess.WaitForExit();
                }

                // Start new router process
                var spawnedRouterProcess = Process.Start(routerAssemblyFile);

                // If we are in "developer" mode, attach job so that it gets killed with editor
                if (attachChildJob && spawnedRouterProcess != null)
                {
                    new AttachedChildProcessJob(spawnedRouterProcess);
                }

                return spawnedRouterProcess != null;
            }
            catch
            {
                return false;
            }
        }

        public static void ParseUrl(string url, out string[] segments, out string parameters)
        {
            // Ideally we would like to reuse Uri (or some other similar code), but it doesn't work without a Host
            var parameterIndex = url.IndexOf('?');
            parameters = parameterIndex != -1 ? url.Substring(parameterIndex + 1) : string.Empty;

            var urlWithoutParameters = parameterIndex != -1 ? url.Substring(0, parameterIndex) : url;

            segments = urlWithoutParameters.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static NameValueCollection ParseQueryString(string query)
        {
            return System.Web.HttpUtility.ParseQueryString(query);
        }
    }
}