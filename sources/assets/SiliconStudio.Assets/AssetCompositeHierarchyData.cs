// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A class containing the information of a hierarchy of asset parts contained in an <see cref="AssetCompositeHierarchy{TAssetPartDesign, TAssetPart}"/>.
    /// </summary>
    /// <typeparam name="TAssetPartDesign">The type used for the design information of a part.</typeparam>
    /// <typeparam name="TAssetPart">The type used for the actual parts,</typeparam>
    [DataContract("AssetCompositeHierarchyData")]
    public class AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
        where TAssetPart : class, IIdentifiable
    {
        /// <summary>
        /// Gets a collection if identifier of all the parts that are root of this hierarchy.
        /// </summary>
        [DataMember(10)]
        [NonIdentifiableCollectionItems]
        [NotNull]
        public List<Guid> RootPartIds { get; } = new List<Guid>();

        /// <summary>
        /// Gets a collection of all the parts, root or not, contained in this hierarchy.
        /// </summary>
        [DataMember(20)]
        [NonIdentifiableCollectionItems]
        [ItemNotNull, NotNull]
        public AssetPartCollection<TAssetPartDesign, TAssetPart> Parts { get; } = new AssetPartCollection<TAssetPartDesign, TAssetPart>();

        /// <summary>
        /// Gathers all base assets used in the composition of the given hierarchy, recursively.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public ICollection<AssetId> GatherAllBasePartAssets([NotNull] IAssetFinder assetFinder)
        {
            var baseAssets = new HashSet<AssetId>();
            GatherAllBasePartAssetsRecursively(assetFinder, baseAssets);
            return baseAssets;
        }

        private void GatherAllBasePartAssetsRecursively([NotNull] IAssetFinder assetFinder, [NotNull] ISet<AssetId> baseAssets)
        {
            if (assetFinder == null) throw new ArgumentNullException(nameof(assetFinder));
            if (baseAssets == null) throw new ArgumentNullException(nameof(baseAssets));
            foreach (var part in Parts.Where(x => x.Base != null))
            {
                if (baseAssets.Add(part.Base.BasePartAsset.Id))
                {
                    var baseAsset = assetFinder.FindAsset(part.Base.BasePartAsset.Id)?.Asset as AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>;
                    if (baseAsset != null)
                    {
                        baseAssets.AddRange(baseAsset.Hierarchy.GatherAllBasePartAssets(assetFinder));
                    }
                }
            }
        }
    }
}
