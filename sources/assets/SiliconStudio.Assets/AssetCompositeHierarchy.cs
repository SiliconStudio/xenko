using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NuGet;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Assets
{
    public abstract class AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> : AssetComposite
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
        where TAssetPart : class, IIdentifiable
    {
        /// <summary>
        /// Gets or sets the container of the hierarchy of asset parts.
        /// </summary>
        [DataMember(100)]
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> Hierarchy { get; set; } = new AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>();

        /// <summary>
        /// Gets the parent of the given part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns>The part that is the parent of the given part, or null if the given part is at the root level.</returns>
        /// <remarks>Implementations of this method should not rely on the <see cref="Hierarchy"/> property to determine the parent.</remarks>
        /// <exception cref="ArgumentNullException">The given part is null.</exception>
        public abstract TAssetPart GetParent(TAssetPart part);

        /// <summary>
        /// Gets the index of the given part in the child list of its parent, or in the list of root if this part is a root part.
        /// </summary>
        /// <param name="part">The part for which to retrieve the index.</param>
        /// <returns>The index of the part, or a negative value if the part is an orphan part that is not a member of this asset.</returns>
        /// <exception cref="ArgumentNullException">The given part is null.</exception>
        public abstract int IndexOf(TAssetPart part);

        /// <summary>
        /// Gets the child of the given part that matches the given index.
        /// </summary>
        /// <param name="part">The part for which to retrieve a child.</param>
        /// <param name="index">The index of the child to retrieve.</param>
        /// <returns>The the child of the given part that matches the given index.</returns>
        /// <exception cref="ArgumentNullException">The given part is null.</exception>
        /// <exception cref="IndexOutOfRangeException">The given index is out of range.</exception>
        public abstract TAssetPart GetChild(TAssetPart part, int index);

        /// <summary>
        /// Gets the number of children in the given part.
        /// </summary>
        /// <param name="part">The part for which to retrieve the number of children.</param>
        /// <returns>The number of children in the given part.</returns>
        /// <exception cref="ArgumentNullException">The given part is null.</exception>
        public abstract int GetChildCount(TAssetPart part);

        /// <summary>
        /// Enumerates parts that are children of the given part.
        /// </summary>
        /// <param name="part">The part for which to enumerate child parts.</param>
        /// <param name="isRecursive">If true, child parts will be enumerated recursively.</param>
        /// <returns>A sequence containing the child parts of the given part.</returns>
        /// <remarks>Implementations of this method should not rely on the <see cref="Hierarchy"/> property to enumerate.</remarks>
        public abstract IEnumerable<TAssetPart> EnumerateChildParts(TAssetPart part, bool isRecursive);

        /// <summary>
        /// Enumerates design parts that are children of the given design part.
        /// </summary>
        /// <param name="partDesign">The design part for which to enumerate child parts.</param>
        /// <param name="hierarchyData">The hierarchy data object in which the design parts can be retrieved.</param>
        /// <param name="isRecursive">If true, child design parts will be enumerated recursively.</param>
        /// <returns>A sequence containing the child design parts of the given design part.</returns>
        public IEnumerable<TAssetPartDesign> EnumerateChildParts(TAssetPartDesign partDesign, AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> hierarchyData, bool isRecursive)
        {
            return EnumerateChildParts(partDesign.Part, isRecursive).Select(e => hierarchyData.Parts[e.Id]);
        }

        public override IEnumerable<AssetPart> CollectParts()
        {
            return Hierarchy.Parts.Select(x => new AssetPart(x.Part.Id, x.Base, newBase => x.Base = newBase));
        }

        public override IIdentifiable FindPart(Guid partId)
        {
            return Hierarchy.Parts.FirstOrDefault(x => x.Part.Id == partId)?.Part;
        }

        public override bool ContainsPart(Guid id)
        {
            return Hierarchy.Parts.ContainsKey(id);
        }

        public override Asset CreateDerivedAsset(string baseLocation, out Dictionary<Guid, Guid> idRemapping)
        {
            var newAsset = (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)base.CreateDerivedAsset(baseLocation, out idRemapping);

            var instanceId = Guid.NewGuid();
            foreach (var part in Hierarchy.Parts)
            {
                var newPart = newAsset.Hierarchy.Parts[idRemapping[part.Part.Id]];
                newPart.Base = new BasePart(new AssetReference(Id, baseLocation), part.Part.Id, instanceId);
            }

            AssetPartsAnalysis.RemapPartsId(newAsset.Hierarchy, idRemapping);

            return newAsset;
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="generateNewIdsForIdentifiableObjects">If true, the cloned objects that implement <see cref="IIdentifiable"/> will have new ids.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy(Guid sourceRootId, bool cleanReference, bool generateNewIdsForIdentifiableObjects)
        {
            Dictionary<Guid, Guid> idRemapping;
            return CloneSubHierarchies(sourceRootId.Yield(), cleanReference, generateNewIdsForIdentifiableObjects, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="generateNewIdsForIdentifiableObjects">If true, the cloned objects that implement <see cref="IIdentifiable"/> will have new ids.</param>
        /// <param name="idRemapping">A dictionary containing the remapping of <see cref="IIdentifiable.Id"/> if <see cref="AssetClonerFlags.GenerateNewIdsForIdentifiableObjects"/> has been passed to the cloner.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy(Guid sourceRootId, bool cleanReference, bool generateNewIdsForIdentifiableObjects, out Dictionary<Guid, Guid> idRemapping)
        {
            return CloneSubHierarchies(sourceRootId.Yield(), cleanReference, generateNewIdsForIdentifiableObjects, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootIds">The ids that are the roots of the sub-hierarchies to clone.</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="generateNewIdsForIdentifiableObjects">If true, the cloned objects that implement <see cref="IIdentifiable"/> will have new ids.</param>
        /// <param name="idRemapping">A dictionary containing the remapping of <see cref="IIdentifiable.Id"/> if <see cref="AssetClonerFlags.GenerateNewIdsForIdentifiableObjects"/> has been passed to the cloner.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <remarks>The parts passed to this methods must be independent in the hierarchy.</remarks>
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchies(IEnumerable<Guid> sourceRootIds, bool cleanReference, bool generateNewIdsForIdentifiableObjects, out Dictionary<Guid, Guid> idRemapping)
        {
            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeHierarchy = new AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>();
            foreach (var rootId in sourceRootIds)
            {
                if (!Hierarchy.Parts.ContainsKey(rootId))
                    throw new ArgumentException(@"The source root parts must be parts of this asset.", nameof(sourceRootIds));

                subTreeHierarchy.RootPartIds.Add(rootId);

                subTreeHierarchy.Parts.Add(Hierarchy.Parts[rootId]);
                foreach (var subTreePart in EnumerateChildParts(Hierarchy.Parts[rootId].Part, true))
                    subTreeHierarchy.Parts.Add(Hierarchy.Parts[subTreePart.Id]);
            }
            // clone the parts of the sub-tree
            var clonedHierarchy = AssetCloner.Clone(subTreeHierarchy, generateNewIdsForIdentifiableObjects ? AssetClonerFlags.GenerateNewIdsForIdentifiableObjects : AssetClonerFlags.None, out idRemapping);

            // Remap ids from the root id collection to the new ids generated during cloning
            AssetPartsAnalysis.RemapPartsId(clonedHierarchy, idRemapping);

            foreach (var rootEntity in clonedHierarchy.RootPartIds)
            {
                PostClonePart(clonedHierarchy.Parts[rootEntity].Part);
            }
            if (cleanReference)
            {
                ClearPartReferences(clonedHierarchy);
            }

            return clonedHierarchy;
        }

        /// <summary>
        /// Generates a hierarchy object from the given part that is compatible with the given asset.
        /// </summary>
        /// <typeparam name="TAssetPartDesign">The type of part design for this asset.</typeparam>
        /// <typeparam name="TAssetPart">The type of part for this asset.</typeparam>
        /// <param name="partDesign">The root part design for the hierarchy to generate.</param>
        /// <returns>A hierarchy containing the given part as root and all its children.</returns>
        /// <remarks>
        /// The given part design does not need to be a member of the given asset for this method to work.
        /// </remarks>
        [NotNull]
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> GenerateHierarchyFromPart([NotNull] TAssetPartDesign partDesign)
        {
            if (partDesign == null) throw new ArgumentNullException(nameof(partDesign));
            var result = new AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>();
            foreach (var child in EnumerateChildParts(partDesign, Hierarchy, true))
            {
                result.Parts.Add(child);
            }
            result.Parts.Add(partDesign);
            result.RootPartIds.Add(partDesign.Part.Id);
            return result;
        }

        /// <summary>
        /// Clears the part references on the cloned hierarchy. Called by <see cref="CloneSubHierarchies"/> when parameter <i>cleanReference</i> is <c>true</c>.
        /// </summary>
        /// <param name="clonedHierarchy">The cloned hierarchy.</param>
        protected virtual void ClearPartReferences(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> clonedHierarchy)
        {
            // default implementation does nothing
        }

        /// <summary>
        /// Called by <see cref="CloneSubHierarchies"/> after a part has been cloned.
        /// </summary>
        /// <param name="part">The cloned part.</param>
        protected virtual void PostClonePart(TAssetPart part)
        {
            // default implementation does nothing
        }

        protected override object ResolvePartReference(object partReference)
        {
            var reference = partReference as TAssetPart;
            if (reference != null)
            {
                TAssetPartDesign realPart;
                Hierarchy.Parts.TryGetValue(reference.Id, out realPart);
                return realPart?.Part;
            }
            return null;
        }
    }
}
