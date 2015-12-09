// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Helper to find available new asset locations and identifiers.
    /// </summary>
    public sealed class AssetResolver
    {
        /// <summary>
        /// Delegate to test if an asset id is already used.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns><c>true</c> if an asset id is already used, <c>false</c> otherwise.</returns>
        public delegate bool ContainsAssetWithIdDelegate(Guid guid);

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetResolver"/> class.
        /// </summary>
        public AssetResolver() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetResolver" /> class.
        /// </summary>
        /// <param name="containsLocation">The delegate used to check if an asset location is already used.</param>
        /// <param name="containsAssetWithId">The delegate used to check if an asset identifier is already used.</param>
        public AssetResolver(NamingHelper.ContainsLocationDelegate containsLocation, ContainsAssetWithIdDelegate containsAssetWithId)
        {
            ExistingIds = new HashSet<Guid>();
            ExistingLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ContainsLocation = containsLocation;
            ContainsAssetWithId = containsAssetWithId;
        }

        /// <summary>
        /// Gets the locations already used.
        /// </summary>
        /// <value>The locations.</value>
        public HashSet<string> ExistingLocations { get; }

        /// <summary>
        /// Gets the asset ids already used.
        /// </summary>
        /// <value>The existing ids.</value>
        public HashSet<Guid> ExistingIds { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to always generate a new id on <see cref="RegisterId"/>.
        /// </summary>
        /// <value><c>true</c> if [force new identifier]; otherwise, <c>false</c>.</value>
        public bool AlwaysCreateNewId { get; set; }

        /// <summary>
        /// Gets or sets a delegate to test if a location is already used.
        /// </summary>
        /// <value>A delegate to test if a location is already used.</value>
        public NamingHelper.ContainsLocationDelegate ContainsLocation { get; set; }

        /// <summary>
        /// Gets or sets a delegate to test if an asset id is already used.
        /// </summary>
        /// <value>A delegate to test if an asset id is already used.</value>
        public ContainsAssetWithIdDelegate ContainsAssetWithId { get; set; }

        /// <summary>
        /// Finds a name available for a new asset. This method will try to create a name based on an existing name and will append
        /// "_" + (number++) on every try. The new location found is added to the known existing locations.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="newLocation">The new location.</param>
        /// <returns><c>true</c> if there is a new location, <c>false</c> otherwise.</returns>
        public bool RegisterLocation(UFile location, out UFile newLocation)
        {
            newLocation = location;
            if (IsContainingLocation(location))
            {
                newLocation = NamingHelper.ComputeNewName(location, IsContainingLocation);
            }
            ExistingLocations.Add(newLocation);
            return newLocation != location;
        }

        /// <summary>
        /// Registers an asset identifier for usage.
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="newGuid">The new unique identifier if an asset has already been registered with the same id.</param>
        /// <returns><c>true</c> if the asset id is already in used. <paramref name="newGuid" /> contains a new guid, <c>false</c> otherwise.</returns>
        public bool RegisterId(Guid assetId, out Guid newGuid)
        {
            newGuid = assetId;
            var result = AlwaysCreateNewId || IsContainingId(assetId);
            if (result)
            {
                newGuid = Guid.NewGuid();
            }
            ExistingIds.Add(newGuid);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="AssetResolver" /> using an existing package to check the existence of asset locations and ids.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>A new AssetResolver.</returns>
        /// <exception cref="System.ArgumentNullException">package</exception>
        public static AssetResolver FromPackage(Package package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            var packagesForLocation = package.FindDependencies(true);
            var packagesForIds = package.Session != null ? package.Session.Packages : packagesForLocation;

            return new AssetResolver(packagesForLocation.ContainsAsset, packagesForIds.ContainsAsset);
        }

        /// <summary>
        /// Creates a new <see cref="AssetResolver"/> using an existing package to check the existence of asset locations and ids.
        /// </summary>
        /// <param name="packages">The packages.</param>
        /// <returns>A new AssetResolver.</returns>
        /// <exception cref="System.ArgumentNullException">package</exception>
        public static AssetResolver FromPackage(IList<Package> packages)
        {
            if (packages == null) throw new ArgumentNullException(nameof(packages));
            return new AssetResolver(packages.ContainsAsset, packages.ContainsAsset);
        }

        /// <summary>
        /// Checks whether the <paramref name="guid"/> is already contained.
        /// </summary>
        private bool IsContainingId(Guid guid)
        {
            if (ExistingIds.Contains(guid))
            {
                return true;
            }
            return ContainsAssetWithId?.Invoke(guid) ?? false;
        }

        /// <summary>
        /// Checks whether the <paramref name="location"/> is already contained.
        /// </summary>
        private bool IsContainingLocation(UFile location)
        {
            if (ExistingLocations.Contains(location))
            {
                return true;
            }
            return ContainsLocation?.Invoke(location) ?? false;
        }
    }
}