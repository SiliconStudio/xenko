// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Extension methods for <see cref="AssetCompositeHierarchy{TAssetPartDesign,TAssetPart}"/> and <see cref="AssetCompositeHierarchyData{TAssetPartDesign,TAssetPart}"/>
    /// </summary>
    public static class AssetCompositeHierarchyExtensions
    {
        /// <summary>
        /// Enumerates the root design parts of this hierarchy.
        /// </summary>
        /// <typeparam name="TAssetPartDesign">The type used for the design information of a part.</typeparam>
        /// <typeparam name="TAssetPart">The type used for the actual parts,</typeparam>
        /// <param name="this">This hierarchy.</param>
        /// <returns>A sequence containing the root design parts of this hierarchy.</returns>
        [ItemNotNull, NotNull, Pure]
        public static IEnumerable<TAssetPartDesign> EnumerateRootPartDesigns<TAssetPartDesign, TAssetPart>([NotNull] this AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> @this)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            return @this.RootParts.Select(x => @this.Parts[x.Id]);
        }

        /// <summary>
        /// Merges the <paramref name="other"/> hierarchy into this hierarchy.
        /// </summary>
        /// <remarks>
        /// This method does not check whether the two hierarchies have independent parts and will fail otherwise.
        /// </remarks>
        /// <typeparam name="TAssetPartDesign">The type used for the design information of a part.</typeparam>
        /// <typeparam name="TAssetPart">The type used for the actual parts,</typeparam>
        /// <param name="this">This hierarchy.</param>
        /// <param name="other">The other hierarchy which parts will added to this hierarchy.</param>
        public static void MergeInto<TAssetPartDesign, TAssetPart>([NotNull] this AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> @this,
            [NotNull] AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> other)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            @this.RootParts.AddRange(other.RootParts);
            @this.Parts.AddRange(other.Parts);
        }
    }
}
