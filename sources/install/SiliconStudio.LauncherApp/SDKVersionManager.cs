using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using SiliconStudio.Assets;

namespace SiliconStudio.LauncherApp
{
    public class SDKVersionManager
    {
        // List of the Major.Minor version displayed to the user
        public List<SDKVersion> Versions { get; set; }

        // List of the installed versions the user can start
        public List<SDKVersion> RunnableVersions { get; set; }

        public SDKVersion VsixVersion { get; set; }

        // Latest packages on the server
        public IEnumerable<IPackage> serverPackages { get; private set; }
        public IEnumerable<IPackage> serverVsixPackages { get; private set; }

        public IEnumerable<IPackage> localPackages { get; private set; }
        public IEnumerable<IPackage> localVsixPackages { get; private set; }

        private PackageManager manager;
        private String packageName;
        private String vsixPackageName;
       

        public SDKVersionManager(PackageManager manager, String packageName, String vsixPackageName)
        {
            Versions = new List<SDKVersion>();
            RunnableVersions = new List<SDKVersion>();
            VsixVersion = null;
            this.manager = manager;
            this.packageName = packageName;
            this.vsixPackageName = vsixPackageName;
        }

        // Retrieve package list from the server
        public void fetchServer()
        {
            // SDK versions
            serverPackages = manager.SourceRepository.FindPackagesById(packageName).OrderByDescending(p => p.Version);
            // VSIX versions
            serverVsixPackages = manager.SourceRepository.FindPackagesById(vsixPackageName).OrderByDescending(p => p.Version);
        }

        // Retrieve local package list
        public void fetchLocal()
        {
            localPackages = manager.LocalRepository.FindPackagesById(packageName).OrderByDescending(p => p.Version);
            localVsixPackages = manager.LocalRepository.FindPackagesById(vsixPackageName).OrderByDescending(p => p.Version);
        }

        // Synchronize remote and local data
        public void synchronizeData()
        {
            Versions.Clear();
            RunnableVersions.Clear();
            VsixVersion = null;

            // If we don't have the remote list, build from the local package list
            var packageList = serverPackages;
            if (packageList.IsEmpty()) packageList = localPackages;

            // Group packages by Major.Minor revision
            var majorMinorPkg = packageList.GroupBy(p => p.Version.Version.Major, p => p);

            foreach (var major in majorMinorPkg)
            {
                var majorVersion = major.Key;

                var minorPkg = major.GroupBy(p => p.Version.Version.Minor, p => p);

                foreach (var minor in minorPkg)
                {
                    var latestPackage = minor.First();

                    var sdkVersionItem = new SDKVersion()
                    {
                        LatestAvailablePackage = latestPackage,
                        Major = majorVersion,
                        Minor = latestPackage.Version.Version.Minor
                    };

                    // Check if we have one revision of the Major.Minor version already installed locally
                    var localPackage = manager.LocalRepository.FindPackagesById(packageName).OrderByDescending(p => p.Version)
                                            .FirstOrDefault(p => p.Version.Version.Major == sdkVersionItem.Major && p.Version.Version.Minor == sdkVersionItem.Minor);
                    sdkVersionItem.InstalledPackage = localPackage;

                    Versions.Add(sdkVersionItem);
                }

            }

            // List of the startable versions
            RunnableVersions = Versions.Where(v => v.IsInstalled).ToList();

            // VSIX plugin
            VsixVersion = new SDKVersion();
            var vsixPackageList = serverVsixPackages;
            if (vsixPackageList.IsEmpty()) vsixPackageList = localVsixPackages;

            if (!vsixPackageList.IsEmpty())
            {
                var latestVsix = vsixPackageList.FirstOrDefault();
                VsixVersion.LatestAvailablePackage = latestVsix;

                // Check our VSIX currently installed
                VsixVersion.InstalledPackage = manager.LocalRepository.FindPackagesById(vsixPackageName).OrderByDescending(p => p.Version).FirstOrDefault();
            }
        }
    }
}
