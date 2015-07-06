using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Attribute that describes what a package upgrader can do.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PackageUpgraderAttribute : Attribute
    {
        private readonly PackageVersionRange packageUpdatedVersionRange;
        
        public string PackageName { get; private set; }

        public PackageVersion PackageMinimumVersion { get; private set; }

        public PackageVersionRange PackageUpdatedVersionRange { get { return packageUpdatedVersionRange; } }

        public PackageUpgraderAttribute(string packageName, string packageMinimumVersion, string packageUpdatedVersionRange)
        {
            PackageName = packageName;
            PackageMinimumVersion = new PackageVersion(packageMinimumVersion);
            PackageVersionRange.TryParse(packageUpdatedVersionRange, out this.packageUpdatedVersionRange);
        }
    }
}