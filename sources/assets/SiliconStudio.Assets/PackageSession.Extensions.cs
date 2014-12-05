// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Extension methods for <see cref="PackageSession"/>.
    /// </summary>
    public static class PackageSessionExtensions
    {
        /// <summary>
        /// Finds an asset from all the packages by its location.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="location">The location of the asset.</param>
        /// <returns>An <see cref="AssetItem" /> or <c>null</c> if not found.</returns>
        public static AssetItem FindAsset(this PackageSession session, UFile location)
        {
            var packages = session.CurrentPackage != null ? session.GetPackagesFromCurrent() : session.Packages;
            return packages.Select(packageItem => packageItem.Assets.Find(location)).FirstOrDefault(asset => asset != null);
        }

        /// <summary>
        /// Finds an asset from all the packages by its id.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="assetId">The assetId of the asset.</param>
        /// <returns>An <see cref="AssetItem" /> or <c>null</c> if not found.</returns>
        public static AssetItem FindAsset(this PackageSession session, Guid assetId)
        {
            return session.Packages.Select(packageItem => packageItem.Assets.Find(assetId)).FirstOrDefault(asset => asset != null);
        }

        /// <summary>
        /// Create a <see cref="PackageSession"/> that can be used to compile an <see cref="AssetItem"/> by analyzing and resolving its dependencies.
        /// </summary>
        /// <returns>The package packageSession that can be used to compile the asset item.</returns>
        public static PackageSession CreateCompilePackageFromAsset(this PackageSession session, AssetItem originalAssetItem)
        {
            if (originalAssetItem == null) throw new ArgumentNullException("originalAssetItem");

            // Find the asset from the session
            var assetItem = session.FindAsset(originalAssetItem.Id);
            if (assetItem == null)
            {
                throw new ArgumentException("Cannot find the specified AssetItem instance in the session");
            }

            // Calculate dependencies
            var dependencies = session.DependencyManager.ComputeDependencies(assetItem, AssetDependencySearchOptions.Out | AssetDependencySearchOptions.Recursive);
            var assetItemRootCloned = dependencies.Item.Clone();

            // Store the fullpath to the sourcefolder, this avoid us to clone hierarchy of packages
            assetItemRootCloned.SourceFolder = assetItem.FullPath.GetParent();

            // create the compile root package and package session
            var assetPackageCloned = new Package();
            var compilePackageSession = new PackageSession(assetPackageCloned);

            assetPackageCloned.Assets.Add(assetItemRootCloned);

            // For each asset item dependency, clone it in the new package
            foreach (var assetLink in dependencies.LinksOut)
            {
                // Only add assets not already added (in case of circular dependencies)
                if (assetPackageCloned.Assets.Find(assetLink.Item.Id) == null)
                {
                    // create a copy of the asset item and add it to the appropriate compile package
                    var itemCloned = assetLink.Item.Clone();

                    // Store the fullpath to the sourcefolder, this avoid us to clone hierarchy of packages
                    itemCloned.SourceFolder = assetLink.Item.FullPath.GetParent();
                    assetPackageCloned.Assets.Add(itemCloned);
                }
            }

            return compilePackageSession;
        }
    }
}