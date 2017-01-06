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
    public abstract class AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> : AssetComposite
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
        where TAssetPart : class, IIdentifiable
    {
        /// <summary>
        /// Gets or sets the container of the hierarchy of asset parts.
        /// </summary>
        [DataMember(100)]
        [Display(Browsable = false)]
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> Hierarchy { get; set; } = new AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>();

        /// <summary>
        /// Gets the parent of the given part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns>The part that is the parent of the given part, or null if the given part is at the root level.</returns>
        /// <remarks>Implementations of this method should not rely on the <see cref="Hierarchy"/> property to determine the parent.</remarks>
        /// <exception cref="ArgumentNullException">The given part is null.</exception>
        [CanBeNull]
        public abstract TAssetPart GetParent([NotNull] TAssetPart part);

        /// <summary>
        /// Gets the index of the given part in the child list of its parent, or in the list of root if this part is a root part.
        /// </summary>
        /// <param name="part">The part for which to retrieve the index.</param>
        /// <returns>The index of the part, or a negative value if the part is an orphan part that is not a member of this asset.</returns>
        /// <exception cref="ArgumentNullException">The given part is null.</exception>
        [Pure]
        public abstract int IndexOf([NotNull] TAssetPart part);

        /// <summary>
        /// Gets the child of the given part that matches the given index.
        /// </summary>
        /// <param name="part">The part for which to retrieve a child.</param>
        /// <param name="index">The index of the child to retrieve.</param>
        /// <returns>The the child of the given part that matches the given index.</returns>
        /// <exception cref="ArgumentNullException">The given part is null.</exception>
        /// <exception cref="IndexOutOfRangeException">The given index is out of range.</exception>
        [Pure]
        public abstract TAssetPart GetChild([NotNull] TAssetPart part, int index);

        /// <summary>
        /// Gets the number of children in the given part.
        /// </summary>
        /// <param name="part">The part for which to retrieve the number of children.</param>
        /// <returns>The number of children in the given part.</returns>
        /// <exception cref="ArgumentNullException">The given part is null.</exception>
        [Pure]
        public abstract int GetChildCount([NotNull] TAssetPart part);

        /// <summary>
        /// Enumerates parts that are children of the given part.
        /// </summary>
        /// <param name="part">The part for which to enumerate child parts.</param>
        /// <param name="isRecursive">If true, child parts will be enumerated recursively.</param>
        /// <returns>A sequence containing the child parts of the given part.</returns>
        /// <remarks>Implementations of this method should not rely on the <see cref="Hierarchy"/> property to enumerate.</remarks>
        [NotNull, Pure]
        public abstract IEnumerable<TAssetPart> EnumerateChildParts([NotNull] TAssetPart part, bool isRecursive);

        /// <inheritdoc/>
        [NotNull]
        public override IEnumerable<AssetPart> CollectParts()
        {
            return Hierarchy.Parts.Select(x => new AssetPart(x.Part.Id, x.Base, newBase => x.Base = newBase));
        }

        /// <inheritdoc/>
        [CanBeNull]
        public override IIdentifiable FindPart(Guid partId)
        {
            return Hierarchy.Parts.FirstOrDefault(x => x.Part.Id == partId)?.Part;
        }

        /// <inheritdoc/>
        public override bool ContainsPart(Guid id)
        {
            return Hierarchy.Parts.ContainsKey(id);
        }

        /// <inheritdoc/>
        public override Asset CreateDerivedAsset(string baseLocation, IDictionary<Guid, Guid> idRemapping = null)
        {
            var newAsset = (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)base.CreateDerivedAsset(baseLocation, idRemapping);

            var remappingDictionary = idRemapping ?? new Dictionary<Guid, Guid>();

            var instanceId = Guid.NewGuid();
            foreach (var part in newAsset.Hierarchy.Parts)
            {
                part.Base = new BasePart(new AssetReference(Id, baseLocation), part.Part.Id, instanceId);
                // Create and register a new id for this part
                var newId = Guid.NewGuid();
                remappingDictionary.Add(part.Part.Id, newId);
                // Apply the new Guid
                part.Part.Id = newId;
            }

            AssetPartsAnalysis.RemapPartsId(newAsset.Hierarchy, remappingDictionary);

            return newAsset;
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootIds">The ids that are the roots of the sub-hierarchies to clone.</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="idRemapping">A dictionary containing the mapping of ids from the source parts to the new parts.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <remarks>The parts passed to this methods must be independent in the hierarchy.</remarks>
        [NotNull, Pure]
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchies([NotNull] IEnumerable<Guid> sourceRootIds, bool cleanReference, [NotNull] out Dictionary<Guid, Guid> idRemapping)
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
            var clonedHierarchy = AssetCloner.Clone(subTreeHierarchy);
            foreach (var rootEntity in clonedHierarchy.RootPartIds)
            {
                PostClonePart(clonedHierarchy.Parts[rootEntity].Part);
            }
            if (cleanReference)
            {
                ClearPartReferences(clonedHierarchy);
            }
            // Generate part mapping
            idRemapping = new Dictionary<Guid, Guid>();
            foreach (var partDesign in clonedHierarchy.Parts)
            {
                // Generate new Id
                var newId = Guid.NewGuid();
                // Update mappings
                idRemapping.Add(partDesign.Part.Id, newId);
                // Update part with new id
                partDesign.Part.Id = newId;
            }
            // Rewrite part references
            // Should we nullify invalid references?
            AssetPartsAnalysis.RemapPartsId(clonedHierarchy, idRemapping);
            return clonedHierarchy;
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

        /// <inheritdoc/>
        [CanBeNull]
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
