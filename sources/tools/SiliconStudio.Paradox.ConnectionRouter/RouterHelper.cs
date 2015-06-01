using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SiliconStudio.Assets;

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
            var paradoxSdkDir = DirectoryHelper.GetPackageDirectory("Paradox");

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
                var paradoxPackage = paradoxVersion != null ? paradoxPackages.FirstOrDefault(p => p.Version.ToString() == paradoxVersion) : paradoxPackages.LastOrDefault();
                if (paradoxPackage == null)
                    return null;

                var packageDirectory = store.PathResolver.GetPackageDirectory(paradoxPackage);
                return Path.Combine(paradoxSdkDir, store.RepositoryPath, packageDirectory);
            }

            return null;
        }

        public static bool EnsureRouterLaunched()
        {
            try
            {
                var routerAssemblyLocation = typeof(Router).Assembly.Location;
                var routerAssemblyExe = Path.GetFileName(routerAssemblyLocation);

                if (RouterMutex.WaitOne(TimeSpan.Zero, true))
                {
                    RouterMutex.ReleaseMutex();
                }
                else
                {
                    // Application is already running
                    return true;
                }

                var paradoxSdkDir = FindParadoxSdkDir();
                if (paradoxSdkDir == null)
                {
                    throw new FileNotFoundException("Could not find Paradox Sdk Dir");
                }

                var paradoxSdkBinDir = Path.Combine(paradoxSdkDir, @"Bin\Windows-Direct3D11");

                var routerAssemblyFile = Path.Combine(paradoxSdkBinDir, routerAssemblyExe);

                if (!File.Exists(routerAssemblyLocation))
                {
                    throw new FileNotFoundException("Could not find Paradox Connection Router executable");
                }

                Process.Start(routerAssemblyFile);

                return true;
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