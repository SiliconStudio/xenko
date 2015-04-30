using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace SiliconStudio.LauncherApp
{
    public class SDKVersion
    {

        public int Major = 0;

        public int Minor = 0;

        public IPackage Package { get; set; }

        public IPackage InstalledPackage { get; set; }

        public IPackage LatestAvailablePackage { get; set; }

        public List<IPackage> remotePackages { get; set; }

        public SDKVersion()
        {
            remotePackages = new List<IPackage>();
        }

        public bool IsInstalled
        {
            get
            {
                return InstalledPackage != null;
            }
        }

        public bool CanBeInstalled
        {
            get
            {
                return !IsInstalled;
            }
        }

        public bool CanBeUpgraded
        {
            get
            {
                if (InstalledPackage == null) return false;
                return InstalledPackage.Version < LatestAvailablePackage.Version;
            }
        }

        public bool CanBeUninstalled
        {
            get
            {
                return InstalledPackage != null;
            }
        }
    }
}
