// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

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
            var packages = session.Packages;
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
            var packages = session.Packages;
            return packages.Select(packageItem => packageItem.Assets.Find(assetId)).FirstOrDefault(asset => asset != null);
        }

        public static AssetItem FindAssetFromAttachedReference(this PackageSession session, object obj)
        {
            if (obj == null)
                return null;

            var reference = AttachedReferenceManager.GetAttachedReference(obj);
            return reference != null ? (FindAsset(session, reference.Id) ?? FindAsset(session, reference.Url)) : null;
        }

        /// <summary>
        /// Create a <see cref="Package"/> that can be used to compile an <see cref="AssetItem"/> by analyzing and resolving its dependencies.
        /// </summary>
        /// <returns>The package packageSession that can be used to compile the asset item.</returns>
        public static Package CreateCompilePackageFromAsset(this PackageSession session, AssetItem originalAssetItem)
        {
            // create the compile root package and package session
            var assetPackageCloned = new Package();
            var compilePackageSession = new PackageSession(assetPackageCloned);

            AddAssetToCompilePackage(session, originalAssetItem, assetPackageCloned);

            return assetPackageCloned;
        }

        public static void AddAssetToCompilePackage(this PackageSession session, AssetItem originalAssetItem, Package assetPackageCloned)
        {
            if (originalAssetItem == null) throw new ArgumentNullException("originalAssetItem");

            // Find the asset from the session
            var assetItem = originalAssetItem.Package.FindAsset(originalAssetItem.Id);
            if (assetItem == null)
            {
                throw new ArgumentException("Cannot find the specified AssetItem instance in the session");
            }

            // Calculate dependencies
            // Search only for references
            var dependencies = session.DependencyManager.ComputeDependencies(assetItem, AssetDependencySearchOptions.Out | AssetDependencySearchOptions.Recursive, ContentLinkType.Reference);
            var assetItemRootCloned = dependencies.Item.Clone();

            // Store the fullpath to the sourcefolder, this avoid us to clone hierarchy of packages
            assetItemRootCloned.SourceFolder = assetItem.FullPath.GetParent();

            if (assetPackageCloned.Assets.Find(assetItemRootCloned.Id) == null)
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
        }
    }
}