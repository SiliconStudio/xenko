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
        [NotNull]
        AssetDependencies ComputeDependencies(AssetId assetId, AssetDependencySearchOptions dependenciesOptions = AssetDependencySearchOptions.All, ContentLinkType linkTypes = ContentLinkType.Reference, HashSet<AssetId> visited = null);
    }
}
