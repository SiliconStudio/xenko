// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Extension methods for <see cref="AssetCompositeHierarchy{TAssetPartDesign,TAssetPart}"/> and <see cref="AssetCompositeHierarchyData{TAssetPartDesign,TAssetPart}"/>
    /// </summary>
    public static class AssetCompositeHierarchyExtensions
    {
        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="this">This asset.</param>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <seealso cref="AssetCompositeHierarchy{TAssetPartDesign,TAssetPart}.CloneSubHierarchies"/>
        [NotNull, Pure]
        public static AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy<TAssetPartDesign, TAssetPart>(
            [NotNull] this AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> @this, Guid sourceRootId, bool cleanReference)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            Dictionary<Guid, Guid> idRemapping;
            return @this.CloneSubHierarchies(sourceRootId.Yield(), cleanReference, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="this">This asset.</param>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="idRemapping">A dictionary containing the mapping of ids from the source parts to the new parts.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <seealso cref="AssetCompositeHierarchy{TAssetPartDesign,TAssetPart}.CloneSubHierarchies"/>
        [NotNull, Pure]
        public static AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy<TAssetPartDesign, TAssetPart>(
            [NotNull] this AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> @this, Guid sourceRootId, bool cleanReference, [NotNull] out Dictionary<Guid, Guid> idRemapping)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            return @this.CloneSubHierarchies(sourceRootId.Yield(), cleanReference, out idRemapping);
        }
        
        /// <summary>
        /// Enumerates design parts that are children of the given design part.
        /// </summary>
        /// <param name="this">This asset.</param>
        /// <param name="partDesign">The design part for which to enumerate child parts.</param>
        /// <param name="hierarchyData">The hierarchy data object in which the design parts can be retrieved.</param>
        /// <param name="isRecursive">If true, child design parts will be enumerated recursively.</param>
        /// <returns>A sequence containing the child design parts of the given design part.</returns>
        [NotNull, Pure]
        public static IEnumerable<TAssetPartDesign> EnumerateChildPartDesigns<TAssetPartDesign, TAssetPart>([NotNull] this AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> @this,
            [NotNull] TAssetPartDesign partDesign, AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> hierarchyData, bool isRecursive)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            return @this.EnumerateChildParts(partDesign.Part, isRecursive).Select(e => hierarchyData.Parts[e.Id]);
        }

        /// <summary>
        /// Combines the specified hierarchies into a new hierarchy containing the parts of both.
        /// </summary>
        /// <remarks>
        /// This method does not check whether the two hierarchies have independent parts and will fail otherwise.
        /// </remarks>
        /// <typeparam name="TAssetPartDesign">The type used for the design information of a part.</typeparam>
        /// <typeparam name="TAssetPart">The type used for the actual parts,</typeparam>
        /// <seealso cref="MergeInto{TAssetPartDesign,TAssetPart}"/>
        [NotNull, Pure]
        public static AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> Concat<TAssetPartDesign, TAssetPart>(
            [NotNull] AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> lhs,
            [NotNull] AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> rhs)
            where TAssetPartDesign : IAssetPartDesign<TAssetPart>
            where TAssetPart : IIdentifiable
        {
            var combinedHierarchy = new AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>();
            combinedHierarchy.MergeInto(lhs);
            combinedHierarchy.MergeInto(rhs);
            return combinedHierarchy;
        }

        /// <summary>
        /// Enumerates the root design parts of this hierarchy.
        /// </summary>
        /// <typeparam name="TAssetPartDesign">The type used for the design information of a part.</typeparam>
        /// <typeparam name="TAssetPart">The type used for the actual parts,</typeparam>
        /// <param name="this">This hierarchy.</param>
        /// <returns>A sequence containing the root design parts of this hierarchy.</returns>
        [ItemNotNull, NotNull, Pure]
        public static IEnumerable<TAssetPartDesign> EnumerateRootPartDesigns<TAssetPartDesign, TAssetPart>([NotNull] this AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> @this)
            where TAssetPartDesign : IAssetPartDesign<TAssetPart>
            where TAssetPart : IIdentifiable
        {
            return @this.RootPartIds.Select(rootId => @this.Parts[rootId]);
        }

        /// <summary>
        /// Enumerates the root parts of this hierarchy.
        /// </summary>
        /// <typeparam name="TAssetPartDesign">The type used for the design information of a part.</typeparam>
        /// <typeparam name="TAssetPart">The type used for the actual parts,</typeparam>
        /// <param name="this">This hierarchy.</param>
        /// <returns>A sequence containing the root parts of this hierarchy.</returns>
        [ItemNotNull, NotNull, Pure]
        public static IEnumerable<TAssetPart> EnumerateRootParts<TAssetPartDesign, TAssetPart>([NotNull] this AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> @this)
            where TAssetPartDesign : IAssetPartDesign<TAssetPart>
            where TAssetPart : IIdentifiable
        {
            return @this.RootPartIds.Select(rootId => @this.Parts[rootId].Part);
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
            where TAssetPartDesign : IAssetPartDesign<TAssetPart>
            where TAssetPart : IIdentifiable
        {
            @this.RootPartIds.AddRange(other.RootPartIds);
            @this.Parts.AddRange(other.Parts);
        }
    }
}
