// Copyright (c) 2016-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SiliconStudio.Assets.Analysis;
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
        /// <param name="sourceRootIds">The ids that are the roots of the sub-hierarchies to clone.</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="newBaseInstance">If true, a new unique identifier for base part <see cref="BasePart.InstanceId"/> will be assigned in the cloned hierarchy.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <seealso cref="CloneSubHierarchies{TAssetPartDesign,TAssetPart}(AssetCompositeHierarchy{TAssetPartDesign,TAssetPart},IEnumerable{Guid},bool,bool,out Dictionary{Guid,Guid})"/>
        [NotNull, Pure]
        public static AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchies<TAssetPartDesign, TAssetPart>(
            [NotNull] this AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> @this, [NotNull] IEnumerable<Guid> sourceRootIds, bool cleanReference, bool newBaseInstance)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            Dictionary<Guid, Guid> idRemapping;
            return CloneSubHierarchies(@this, sourceRootIds, cleanReference, newBaseInstance, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="this">This asset.</param>
        /// <param name="sourceRootIds">The ids that are the roots of the sub-hierarchies to clone.</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="newBaseInstance">If true, a new unique identifier for base part <see cref="BasePart.InstanceId"/> will be assigned in the cloned hierarchy.</param>
        /// <param name="idRemapping">A dictionary containing the mapping of ids from the source parts to the new parts.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <seealso cref="AssetCompositeHierarchy{TAssetPartDesign,TAssetPart}.CloneSubHierarchies"/>
        [NotNull, Pure]
        public static AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchies<TAssetPartDesign, TAssetPart>(
            [NotNull] this AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> @this, [NotNull] IEnumerable<Guid> sourceRootIds, bool cleanReference, bool newBaseInstance,
            [NotNull] out Dictionary<Guid, Guid> idRemapping)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            var clonedHierarchy = @this.CloneSubHierarchies(sourceRootIds, cleanReference, true, out idRemapping); // FIXME
            if (newBaseInstance)
                AssetPartsAnalysis.GenerateNewBaseInstanceIds(clonedHierarchy);
            return clonedHierarchy;
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="this">This asset.</param>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="newBaseInstance">If true, a new unique identifier for base part <see cref="BasePart.InstanceId"/> will be assigned in the cloned hierarchy.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <seealso cref="CloneSubHierarchies{TAssetPartDesign,TAssetPart}(AssetCompositeHierarchy{TAssetPartDesign,TAssetPart},IEnumerable{Guid},bool,bool,out Dictionary{Guid,Guid})"/>
        [NotNull, Pure]
        public static AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy<TAssetPartDesign, TAssetPart>(
            [NotNull] this AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> @this, Guid sourceRootId, bool cleanReference, bool newBaseInstance)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            Dictionary<Guid, Guid> idRemapping;
            return CloneSubHierarchies(@this, sourceRootId.Yield(), cleanReference, newBaseInstance, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="this">This asset.</param>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="newBaseInstance"></param>
        /// <param name="idRemapping">A dictionary containing the mapping of ids from the source parts to the new parts.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <seealso cref="CloneSubHierarchies{TAssetPartDesign,TAssetPart}(AssetCompositeHierarchy{TAssetPartDesign,TAssetPart},IEnumerable{Guid},bool,bool,out Dictionary{Guid,Guid})"/>
        [NotNull, Pure]
        public static AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy<TAssetPartDesign, TAssetPart>(
            [NotNull] this AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> @this, Guid sourceRootId, bool cleanReference, bool newBaseInstance, [NotNull] out Dictionary<Guid, Guid> idRemapping)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            return CloneSubHierarchies(@this, sourceRootId.Yield(), cleanReference, newBaseInstance, out idRemapping);
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
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
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
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
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
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
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
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            @this.RootPartIds.AddRange(other.RootPartIds);
            @this.Parts.AddRange(other.Parts);
        }
    }
}
