// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Parameters used for loading a package.
    /// </summary>
    public sealed class PackageLoadParameters
    {
        private static readonly PackageLoadParameters DefaultParameters = new PackageLoadParameters();

        /// <summary>
        /// Gets the default parameters.
        /// </summary>
        /// <returns>PackageLoadParameters.</returns>
        public static PackageLoadParameters Default()
        {
            return DefaultParameters.Clone();
        }

        public PackageLoadParameters()
        {
            LoadAssemblyReferences = true;
            AutoCompileProjects = true;
            AutoLoadTemporaryAssets = true;
            ConvertUPathToAbsolute = true;
            BuildConfiguration = "Debug";
        }

        /// <summary>
        /// Gets or sets a value indicating whether [load assembly references].
        /// </summary>
        /// <value><c>true</c> if [load assembly references]; otherwise, <c>false</c>.</value>
        public bool LoadAssemblyReferences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically compile projects that don't have their assembly generated.
        /// </summary>
        /// <value><c>true</c> if [automatic compile projects]; otherwise, <c>false</c>.</value>
        public bool AutoCompileProjects { get; set; }

        /// <summary>
        /// Gets or sets the build configuration used to <see cref="AutoCompileProjects"/>.
        /// </summary>
        /// <value>The build configuration.</value>
        public string BuildConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the extra compile properties, used when <see cref="AutoCompileProjects"/> is true.
        /// </summary>
        /// <value>
        /// The extra compile parameters.
        /// </value>
        public Dictionary<string, string> ExtraCompileProperties { get; set; }

        /// <summary>
        /// Gets or sets the asset files to load, if you want to not rely on the default <see cref="Package.ListAssetFiles"/>.
        /// </summary>
        /// <value>
        /// The load asset files.
        /// </value>
        public List<PackageLoadingAssetFile> AssetFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically load assets. Default is <c>true</c>
        /// </summary>
        /// <value><c>true</c> if [automatic load assets]; otherwise, <c>false</c>.</value>
        public bool AutoLoadTemporaryAssets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to convert all <see cref="UPath"/> to absolute paths when loading. Default
        /// is <c>true</c>
        /// </summary>
        /// <value><c>true</c> if [convert u path to absolute]; otherwise, <c>false</c>.</value>
        public bool ConvertUPathToAbsolute { get; set; }

        /// <summary>
        /// Gets or sets the cancelation token.
        /// </summary>
        /// <value>The cancel token.</value>
        public CancellationToken? CancelToken { get; set; }

        /// <summary>
        /// Gets or sets the assembly container used to load assemblies referenced by the package. If null, will use the 
        /// <see cref="SiliconStudio.Core.Reflection.AssemblyContainer.Default"/>
        /// </summary>
        /// <value>The assembly container.</value>
        public AssemblyContainer AssemblyContainer { get; set; }

        /// <summary>
        /// Gets or sets the generate new asset ids.
        /// </summary>
        /// <value>The generate new asset ids.</value>
        public bool GenerateNewAssetIds { get; set; }

        /// <summary>
        /// Occurs when one or more package upgrades are required for a single package. Returning false will cancel upgrades on this package.
        /// </summary>
        public Func<Package, IList<PackageSession.PendingPackageUpgrade>, bool> PackageUpgradeRequested;

        /// <summary>
        /// Occurs when an asset is about to be loaded, if false is returned the asset will be ignored and not loaded.
        /// </summary>
        public Func<PackageLoadingAssetFile, bool> TemporaryAssetFilter;

        /// <summary>
        /// Gets a boolean telling if MSBuild files should be evaluated when listing assets.
        /// </summary>
        public bool TemporaryAssetsInMsbuild { get; set; } = true;

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>PackageLoadParameters.</returns>
        public PackageLoadParameters Clone()
        {
            return (PackageLoadParameters)MemberwiseClone();
        }
    }
}
