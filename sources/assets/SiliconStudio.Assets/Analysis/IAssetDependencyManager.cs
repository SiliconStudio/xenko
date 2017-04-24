// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Analysis
{
    public interface IAssetDependencyManager
    {
        /// <summary>
        /// Computes the dependencies for the specified asset.
        /// </summary>
        /// <param name="assetId">The asset id.</param>
        /// <param name="dependenciesOptions">The dependencies options.</param>
        /// <param name="linkTypes">The type of links to visit while computing the dependencies</param>
        /// <param name="visited">The list of element already visited.</param>
        /// <returns>The dependencies, or <c>null</c> if the object is not tracked.</returns>
        [CanBeNull]
        AssetDependencies ComputeDependencies(AssetId assetId, AssetDependencySearchOptions dependenciesOptions = AssetDependencySearchOptions.All, ContentLinkType linkTypes = ContentLinkType.All, HashSet<AssetId> visited = null);

        /// <summary>
        /// Finds the assets the specified asset id inherits from (this is a direct inheritance, not indirect)..
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="searchOptions">The types of inheritance to search for</param>
        /// <returns>A list of asset the specified asset id is inheriting from.</returns>
        [ItemNotNull, NotNull]
        List<AssetItem> FindAssetInheritances(AssetId assetId, AssetInheritanceSearchOptions searchOptions = AssetInheritanceSearchOptions.All);

        /// <summary>
        /// Finds the assets inheriting from the specified asset id (this is a direct inheritance, not indirect).
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="searchOptions">The types of inheritance to search for</param>
        /// <returns>A list of asset inheriting from the specified asset id.</returns>
        [ItemNotNull, NotNull]
        List<AssetItem> FindAssetsInheritingFrom(AssetId assetId, AssetInheritanceSearchOptions searchOptions = AssetInheritanceSearchOptions.All);
    }
}
