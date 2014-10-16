// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Extensions for <see cref="Package"/>
    /// </summary>
    public static class PackageExtensions
    {
        /// <summary>
        /// Finds the package dependencies for the specified <see cref="Package" />. See remarks.
        /// </summary>
        /// <param name="rootPackage">The root package.</param>
        /// <param name="includeRootPackage">if set to <c>true</c> [include root package].</param>
        /// <param name="isRecursive">if set to <c>true</c> [is recursive].</param>
        /// <returns>List&lt;Package&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">rootPackage</exception>
        /// <exception cref="System.ArgumentException">Root package must be part of a session;rootPackage</exception>
        public static PackageCollection FindDependencies(this Package rootPackage, bool includeRootPackage = false, bool isRecursive = true)
        {
            if (rootPackage == null) throw new ArgumentNullException("rootPackage");
            var packages = new PackageCollection();

            if (includeRootPackage)
            {
                packages.Add(rootPackage);
            }

            FillPackageDependencies(rootPackage, isRecursive, packages);

            return packages;
        }

        /// <summary>
        /// Determines whether the specified packages contains an asset by its guid.
        /// </summary>
        /// <param name="packages">The packages.</param>
        /// <param name="assetGuid">The asset unique identifier.</param>
        /// <returns><c>true</c> if the specified packages contains asset; otherwise, <c>false</c>.</returns>
        public static bool ContainsAsset(this IEnumerable<Package> packages, Guid assetGuid)
        {
            return packages.Any(package => package.Assets.ContainsById(assetGuid));
        }

        /// <summary>
        /// Determines whether the specified packages contains an asset by its location.
        /// </summary>
        /// <param name="packages">The packages.</param>
        /// <param name="location">The location.</param>
        /// <returns><c>true</c> if the specified packages contains asset; otherwise, <c>false</c>.</returns>
        public static bool ContainsAsset(this IEnumerable<Package> packages, UFile location)
        {
            return packages.Any(package => package.Assets.Find(location) != null);
        }

        private static void FillPackageDependencies(Package rootPackage, bool isRecursive, ICollection<Package> packagesFound)
        {
            var session = rootPackage.Session;

            if (session == null && (rootPackage.Meta.Dependencies.Count > 0 || rootPackage.LocalDependencies.Count > 0))
            {
                throw new InvalidOperationException("Cannot query package with dependencies when it is not attached to a session");
            }

            // 1. Load store package
            foreach (var packageDependency in rootPackage.Meta.Dependencies)
            {
                var package = session.Packages.Find(packageDependency);
                if (package == null)
                {
                    continue;
                }

                if (!packagesFound.Contains(package))
                {
                    packagesFound.Add(package);

                    if (isRecursive)
                    {
                        FillPackageDependencies(package, isRecursive, packagesFound);
                    }
                }
            }

            // 2. Load local packages
            foreach (var packageReference in rootPackage.LocalDependencies)
            {
                var package = session.Packages.Find(packageReference.Id);
                if (package == null)
                {
                    continue;
                }

                if (!packagesFound.Contains(package))
                {
                    packagesFound.Add(package);

                    if (isRecursive)
                    {
                        FillPackageDependencies(package, isRecursive, packagesFound);
                    }
                }
            }

        }
    }
}