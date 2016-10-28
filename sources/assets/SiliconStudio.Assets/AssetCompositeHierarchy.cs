using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;

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
            return Hierarchy.Parts.Select(x => new AssetPart(x.Part.Id, x.BaseId, x.BasePartInstanceId));
        }

        [Obsolete("This method will be removed soon")]
        public override void SetPart(Guid id, Guid baseId, Guid basePartInstanceId)
        {
            TAssetPartDesign partEntry;
            if (Hierarchy.Parts.TryGetValue(id, out partEntry))
            {
                partEntry.BaseId = baseId;
                partEntry.BasePartInstanceId = basePartInstanceId;
            }
        }

        public override bool ContainsPart(Guid id)
        {
            return Hierarchy.Parts.ContainsKey(id);
        }

        public override Asset CreateDerivedAsset(string baseLocation, IDictionary<Guid, Guid> idRemapping = null)
        {
            var newAsset = (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)base.CreateDerivedAsset(baseLocation);

            var remappingDictionary = idRemapping ?? new Dictionary<Guid, Guid>();

            foreach (var part in newAsset.Hierarchy.Parts)
            {
                // Store the baseid of the new version
                part.BaseId = part.Part.Id;
                // Make sure that we don't replicate the base part InstanceId
                part.BasePartInstanceId = null;
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
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy(Guid sourceRootId, bool cleanReference)
        {
            Dictionary<Guid, Guid> idRemapping;
            return CloneSubHierarchy(sourceRootId, cleanReference, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="idRemapping">A dictionary containing the mapping of ids from the source parts to the new parts.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        // TODO: check if this can be factorized in a non-virtal method here
        public abstract AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy(Guid sourceRootId, bool cleanReference, out Dictionary<Guid, Guid> idRemapping);

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
