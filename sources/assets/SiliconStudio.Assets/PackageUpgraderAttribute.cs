// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Attribute that describes what a package upgrader can do.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [BaseTypeRequired(typeof(PackageUpgrader))]
    [AssemblyScan]
    public class PackageUpgraderAttribute : Attribute
    {
        private readonly PackageVersionRange updatedVersionRange;
        
        public string PackageName { get; private set; }

        public PackageVersion PackageMinimumVersion { get; private set; }

        public PackageVersionRange UpdatedVersionRange { get { return updatedVersionRange; } }

        public PackageUpgraderAttribute(string packageName, string packageMinimumVersion, string packageUpdatedVersionRange)
        {
            PackageName = packageName;
            PackageMinimumVersion = new PackageVersion(packageMinimumVersion);
            PackageVersionRange.TryParse(packageUpdatedVersionRange, out this.updatedVersionRange);
        }
    }
}
