// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Attribute that describes what a package upgrader can do.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [BaseTypeRequired(typeof(PackageUpgrader))]
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
