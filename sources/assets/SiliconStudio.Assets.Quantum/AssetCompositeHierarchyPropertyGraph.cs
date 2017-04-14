using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Quantum.Visitors;
using SiliconStudio.Assets.Yaml;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public abstract class AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> : AssetCompositePropertyGraph
        where TAssetPart : class, IIdentifiable
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
    {
        /// <summary>
        /// A dictionary mapping each base asset to a collection of instance ids existing in this asset.
        /// </summary>
        private readonly Dictionary<AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>, HashSet<Guid>> basePartAssets = new Dictionary<AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>, HashSet<Guid>>();
        /// <summary>
        /// A dictionary mapping a tuple of (base part id, instance id) to the corresponding asset part in this asset.
        /// </summary>
        /// <remarks>Part stored here are preserved after being removed, in case they have to come back later, for example if a part in the base is being moved (removed + added again).</remarks>
        private readonly Dictionary<Tuple<Guid, Guid>, TAssetPart> baseInstanceMapping = new Dictionary<Tuple<Guid, Guid>, TAssetPart>();

        /// <summary>
        /// A mapping of (base part id, instance id) corresponding to deleted parts in specific instances of this asset which base part exists in the base asset.
        /// </summary>
        private readonly HashSet<Tuple<Guid, Guid>> deletedPartsInstanceMapping = new HashSet<Tuple<Guid, Guid>>();
        /// <summary>
        /// A dictionary mapping instance ids to the common ancestor of the parts corresponding to that instance id in this asset.
        /// </summary>
        /// <remarks>This dictionary is used to remember where the part instance was located, if during some time all its parts are removed, for example during some specific operaiton in the base asset.</remarks>
        private readonly Dictionary<Guid, Guid> instancesCommonAncestors = new Dictionary<Guid, Guid>();
        /// <summary>
        /// A hashset of nodes representing the collections of children from a parent part.
        /// </summary>
        private readonly HashSet<IGraphNode> registeredChildParts = new HashSet<IGraphNode>();

        protected AssetCompositeHierarchyPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
            HierarchyNode = RootNode[nameof(AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>.Hierarchy)].Target;
            var rootPartsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)].Target;
            rootPartsNode.ItemChanged += RootPartsChanged;
            foreach (var childPartNode in Asset.Hierarchy.Parts.SelectMany(x => RetrieveChildPartNodes(x.Part)))
            {
                RegisterChildPartNode(childPartNode);
            }
            var partsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<IAssetPartDesign<IIdentifiable>, IIdentifiable>.Parts)].Target;
            partsNode.ItemChanged += PartsChanged;

            foreach (var part in Asset.Hierarchy.Parts)
            {
                LinkToOwnerPart(Container.NodeContainer.GetNode(part.Part), part);
            }
        }

        /// <inheritdoc cref="AssetPropertyGraph.Asset"/>
        public new AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> Asset => (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)base.Asset;

        protected IObjectNode HierarchyNode { get; }

        /// <summary>
        /// Gets the name of the property targeting the part in the <see cref="TAssetPartDesign"/> type.
        /// </summary>
        protected virtual string PartName => nameof(IAssetPartDesign<TAssetPart>.Part);

        /// <summary>
        /// Raised when a part is added to this asset.
        /// </summary>
        public event EventHandler<AssetPartChangeEventArgs> PartAdded;

        /// <summary>
        /// Raised when a part is removed from this asset.
        /// </summary>
        public event EventHandler<AssetPartChangeEventArgs> PartRemoved;

        public abstract bool IsChildPartReference(IGraphNode node, Index index);

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            var rootPartsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)].Target;
            rootPartsNode.ItemChanged -= RootPartsChanged;
            registeredChildParts.ToList().ForEach(UnregisterChildPartNode);
            var partsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<IAssetPartDesign<IIdentifiable>, IIdentifiable>.Parts)].Target;
            partsNode.ItemChanged -= PartsChanged;

            foreach (var basePartAsset in basePartAssets.Keys)
            {
                basePartAsset.PartAdded -= PartAddedInBaseAsset;
                basePartAsset.PartRemoved -= PartRemovedInBaseAsset;
            }
            basePartAssets.Clear();
        }

        /// <inheritdoc/>
        public override void ClearReferencesToObjects(IEnumerable<Guid> objectIds)
        {
            if (objectIds == null) throw new ArgumentNullException(nameof(objectIds));
            var visitor = new ClearObjectReferenceVisitor(this, objectIds, (node, index) => !IsChildPartReference(node, index));
            visitor.Visit(RootNode);
        }

        /// <summary>
        /// Gets all the instance ids corresponding to an instance of the part of the given base asset.
        /// </summary>
        /// <param name="baseAssetPropertyGraph">The property graph of the base asset for which to return instance ids.</param>
        /// <returns>A collection of instances ids corresponding to instances of parts of the given base asset.</returns>
        public IReadOnlyCollection<Guid> GetInstanceIds([NotNull] AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> baseAssetPropertyGraph)
        {
            if (baseAssetPropertyGraph == null) throw new ArgumentNullException(nameof(baseAssetPropertyGraph));
            HashSet<Guid> instanceIds;
            basePartAssets.TryGetValue(baseAssetPropertyGraph, out instanceIds);
            return (IReadOnlyCollection<Guid>)instanceIds ?? new Guid[0];
        }

        /// <summary>
        /// Retrieves all the assets that contains bases of parts of this asset.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public IReadOnlyCollection<AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>> GetBasePartAssets()
        {
            return basePartAssets.Keys;
        }

        public void BreakBasePartLinks([NotNull] IEnumerable<TAssetPartDesign> assetPartDesigns)
        {
            foreach (var part in assetPartDesigns.Where(x => x.Base != null))
            {
                var node = (IAssetObjectNode)Container.NodeContainer.GetNode(part);
                node[nameof(IAssetPartDesign<IIdentifiable>.Base)].Update(null);
                // We must refresh the base to stop further update from the base asset to the instance parts
                RefreshBase(node, null);
            }
        }

        /// <summary>
        /// Adds a part to this asset. This method updates the <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}.Parts"/> collection.
        /// If <paramref name="parent"/> is null, it also updates the <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}.RootPartIds"/> collection.
        /// Otherwise, it updates the collection containing the list of children from the parent part.
        /// </summary>
        /// <param name="newPartCollection">A collection containing the part to add and all its child parts recursively, with their associated <typeparamref name="TAssetPartDesign"/>.</param>
        /// <param name="child">The part to add to this asset.</param>
        /// <param name="parent">The parent part in which to add the child part.</param>
        /// <param name="index">The index in which to insert this part, either in the collection of root part or in the collection of child part of the parent part..</param>
        public void AddPartToAsset(AssetPartCollection<TAssetPartDesign, TAssetPart> newPartCollection, TAssetPartDesign child, [CanBeNull] TAssetPart parent, int index)
        {
            // This insert method does not support negative indices.
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            // For consistency, we need to always add first to the Parts collection before adding to RootPartIds or as a child of an existing part
            InsertPartInPartsCollection(newPartCollection, child);
            if (parent == null)
            {
                var rootPartsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)].Target;
                rootPartsNode.Add(child.Part.Id, new Index(index));
            }
            else
            {
                AddChildPartToParentPart(parent, child.Part, index);
            }
        }

        /// <summary>
        /// Removes a part from this asset. This method updates the <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}.Parts"/> collection.
        /// If the part to remove is a root part, it also updates the <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}.RootPartIds"/> collection.
        /// Otherwise, it updates the collection containing the list of children from the parent of this part.
        /// </summary>
        /// <param name="partDesign">The part to remove from this asset.</param>
        public void RemovePartFromAsset(TAssetPartDesign partDesign)
        {
            if (!Asset.Hierarchy.RootPartIds.Contains(partDesign.Part.Id))
            {
                var parent = Asset.GetParent(partDesign.Part);
                if (parent == null) throw new InvalidOperationException("The part has no parent but is not in the RootPartIds collection.");
                RemoveChildPartFromParentPart(parent, partDesign.Part);
            }
            else
            {
                var index = new Index(Asset.Hierarchy.RootPartIds.IndexOf(partDesign.Part.Id));
                var rootPartsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)].Target;
                rootPartsNode.Remove(partDesign.Part.Id, index);
            }
            RemovePartFromPartsCollection(partDesign);
        }

        /// <summary>
        /// Deletes the given parts and all its children, recursively, and clear all object references to it.
        /// </summary>
        /// <param name="partDesigns">The parts to delete.</param>
        /// <param name="deletedPartsMapping">A mapping of the base information (base part id, instance id) of the deleted parts that have a base.</param>
        public void DeleteParts([NotNull] IEnumerable<TAssetPartDesign> partDesigns, [NotNull] out HashSet<Tuple<Guid, Guid>> deletedPartsMapping)
        {
            if (partDesigns == null) throw new ArgumentNullException(nameof(partDesigns));
            var partsToDelete = new Stack<TAssetPartDesign>(partDesigns);
            var referencesToClear = new HashSet<Guid>();
            deletedPartsMapping = new HashSet<Tuple<Guid, Guid>>();
            while (partsToDelete.Count > 0)
            {
                // We need to remove children first to keep consistency in our data
                var partToDelete = partsToDelete.Peek();
                var children = Asset.EnumerateChildPartDesigns(partToDelete, Asset.Hierarchy, false).ToList();
                if (children.Count > 0)
                {
                    // Enqueue children if there is any, and re-process the stack
                    children.ForEach(x => partsToDelete.Push(x));
                    continue;
                }
                // No children to process, we can safely remove the current part from the stack
                partToDelete = partsToDelete.Pop();
                // First remove all references to the part we are deleting
                // Note: we must do this first so instances of this base will be able to properly make the connection with the base part being cleared
                var containedIdentifiables = IdentifiableObjectCollector.Collect(this, Container.NodeContainer.GetNode(partToDelete.Part));
                containedIdentifiables.Keys.ForEach(x => referencesToClear.Add(x));
                // Then actually remove the part from the hierarchy
                RemovePartFromAsset(partToDelete);
                // Keep track of deleted part instances
                if (partToDelete.Base != null)
                {
                    deletedPartsMapping.Add(Tuple.Create(partToDelete.Base.BasePartId, partToDelete.Base.InstanceId));
                }
            }
            TrackDeletedInstanceParts(deletedPartsMapping);
            ClearReferencesToObjects(referencesToClear);
        }

        /// <inheritdoc/>
        public override IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
        {
            // TODO: try to generalize what the overrides of this implementation are doing.
            // Connect the parts to their base if any.
            var part = sourceNode.Retrieve() as TAssetPart;
            if (part != null && sourceNode is IObjectNode)
            {
                TAssetPartDesign partDesign;
                // The part might be being moved and could possibly be currently not into the Parts collection.
                if (Asset.Hierarchy.Parts.TryGetValue(part.Id, out partDesign) && partDesign.Base != null)
                {
                    var baseAssetGraph = Container.GetGraph(partDesign.Base.BasePartAsset.Id);
                    // Base asset might have been deleted
                    if (baseAssetGraph == null)
                        return base.FindTarget(sourceNode, target);

                    // Part might have been deleted in base asset
                    TAssetPartDesign basePart;
                    ((AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)baseAssetGraph.RootNode.Retrieve()).Hierarchy.Parts.TryGetValue(partDesign.Base.BasePartId, out basePart);
                    return basePart != null ? Container.NodeContainer.GetOrCreateNode(basePart.Part) : base.FindTarget(sourceNode, target);
                }
            }

            return base.FindTarget(sourceNode, target);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="flags">The flags customizing the cloning operation.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy(Guid sourceRootId, SubHierarchyCloneFlags flags)
        {
            Dictionary<Guid, Guid> idRemapping;
            return CloneSubHierarchies(sourceRootId.Yield(), flags, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="flags">The flags customizing the cloning operation.</param>
        /// <param name="idRemapping">A dictionary containing the remapping of <see cref="IIdentifiable.Id"/> if <see cref="AssetClonerFlags.GenerateNewIdsForIdentifiableObjects"/> has been passed to the cloner.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchy(Guid sourceRootId, SubHierarchyCloneFlags flags, out Dictionary<Guid, Guid> idRemapping)
        {
            return CloneSubHierarchies(sourceRootId.Yield(), flags, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootIds">The ids that are the roots of the sub-hierarchies to clone.</param>
        /// <param name="flags">The flags customizing the cloning operation.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <remarks>The parts passed to this methods must be independent in the hierarchy.</remarks>
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchies(IEnumerable<Guid> sourceRootIds, SubHierarchyCloneFlags flags)
        {
            Dictionary<Guid, Guid> idRemapping;
            return CloneSubHierarchies(sourceRootIds, flags, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootIds">The ids that are the roots of the sub-hierarchies to clone.</param>
        /// <param name="flags">The flags customizing the cloning operation.</param>
        /// <param name="idRemapping">A dictionary containing the remapping of <see cref="IIdentifiable.Id"/> if <see cref="AssetClonerFlags.GenerateNewIdsForIdentifiableObjects"/> has been passed to the cloner.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> corresponding to the cloned parts.</returns>
        /// <remarks>The parts passed to this methods must be independent in the hierarchy.</remarks>
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> CloneSubHierarchies(IEnumerable<Guid> sourceRootIds, SubHierarchyCloneFlags flags, out Dictionary<Guid, Guid> idRemapping)
        {
            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeHierarchy = new AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>();
            foreach (var rootId in sourceRootIds)
            {
                if (!Asset.Hierarchy.Parts.ContainsKey(rootId))
                    throw new ArgumentException(@"The source root parts must be parts of this asset.", nameof(sourceRootIds));

                subTreeHierarchy.RootPartIds.Add(rootId);

                subTreeHierarchy.Parts.Add(Asset.Hierarchy.Parts[rootId]);
                foreach (var subTreePart in Asset.EnumerateChildParts(Asset.Hierarchy.Parts[rootId].Part, true))
                    subTreeHierarchy.Parts.Add(Asset.Hierarchy.Parts[subTreePart.Id]);
            }

            var preCloningAsset = (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)Activator.CreateInstance(Asset.GetType());
            preCloningAsset.Hierarchy = subTreeHierarchy;
            var preCloningAssetGraph = (AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>)AssetQuantumRegistry.ConstructPropertyGraph(Container, new AssetItem("", preCloningAsset), null);
            var externalReferences = ExternalReferenceCollector.GetExternalReferences(preCloningAssetGraph, preCloningAssetGraph.RootNode);
            YamlAssetMetadata<OverrideType> overrides = null;
            if ((flags & SubHierarchyCloneFlags.RemoveOverrides) == 0)
            {
                overrides = GenerateOverridesForSerialization(preCloningAssetGraph.RootNode);
            }
            // clone the parts of the sub-tree
            var clonerFlags = AssetClonerFlags.None;

            if ((flags & SubHierarchyCloneFlags.GenerateNewIdsForIdentifiableObjects) != 0)
                clonerFlags |= AssetClonerFlags.GenerateNewIdsForIdentifiableObjects;
            if ((flags & SubHierarchyCloneFlags.CleanExternalReferences) != 0)
                clonerFlags |= AssetClonerFlags.ClearExternalReferences;

            var clonedHierarchy = AssetCloner.Clone(subTreeHierarchy, clonerFlags, externalReferences, out idRemapping);
          
            // When cloning with GenerateNewIdsForIdentifiableObjects, indices in the Parts collection will change. Therefore we need to remap them if we want to keep overrides
            if ((flags & SubHierarchyCloneFlags.RemoveOverrides) == 0 && (flags & SubHierarchyCloneFlags.GenerateNewIdsForIdentifiableObjects) != 0)
            {
                // TODO: we shouldn't have to do that, it would be better to have a more robust collection for parts
                // First, build a mapping old index -> new index
                var partIndexRemapping = new Dictionary<int, int>();
                for (var i = 0; i < preCloningAsset.Hierarchy.Parts.Count; i++)
                {
                    var part = preCloningAsset.Hierarchy.Parts[i];
                    var newIndex = clonedHierarchy.Parts.BinarySearch(idRemapping[part.Part.Id]);
                    partIndexRemapping.Add(i, newIndex);
                }

                // Then, find overrides that work on part by checking the beginning of the YamlAssetPath
                if (overrides == null) throw new InvalidOperationException("overrides collection is null");
                var startPath = new YamlAssetPath();
                startPath.PushMember(nameof(AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>.Hierarchy));
                startPath.PushMember(nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.Parts));
                var fixedOverrides = new YamlAssetMetadata<OverrideType>();
                foreach (var overrideEntry in overrides)
                {
                    var pathItems = overrideEntry.Key.Items;
                    // If this override target a part, we need to fixup the indices.
                    if (pathItems.Count > 3 && startPath.Items[0].Equals(pathItems[0]) && startPath.Items[1].Equals(pathItems[1]))
                    {
                        // Retrieve the previous index
                        var oldIndex = (int)overrideEntry.Key.Items[2].Value;
                        // Append the new index instead on a new YamlAssetPath
                        var newPath = startPath.Clone();
                        newPath.PushIndex(partIndexRemapping[oldIndex]);
                        // And append the rest of the override path normally.
                        for (var i = 3; i < pathItems.Count; ++i)
                        {
                            newPath.Push(pathItems[i]);
                        }
                        fixedOverrides.Set(newPath, overrideEntry.Value);
                    }
                    else
                    {
                        // Otherwise we can just copy the override path as-is
                        fixedOverrides.Set(overrideEntry.Key, overrideEntry.Value);
                    }
                }
                // Replace the overrides collection by the one we just fixed.
                overrides = fixedOverrides;
            }

            // Now that we have fixed overrides (if needed), we can replace the initial hierarchy by the cloned one.
            preCloningAssetGraph.RootNode[nameof(AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>.Hierarchy)].Update(clonedHierarchy);
            if ((flags & SubHierarchyCloneFlags.RemoveOverrides) == 0)
            {
                // And we can apply overrides if needed, with proper (fixed) YamlAssetPath.
                ApplyOverrides(preCloningAssetGraph.RootNode, overrides);
            }

            preCloningAssetGraph.Dispose();

            // Remap ids from the root id collection to the new ids generated during cloning
            if (idRemapping != null)
            {
                preCloningAsset.RemapIdentifiableIds(idRemapping);
            }

            return clonedHierarchy;
        }

        /// <inheritdoc/>
        public override bool IsObjectReference(IGraphNode targetNode, Index index, object value)
        {
            if (targetNode is IObjectNode && index.IsEmpty)
                return base.IsObjectReference(targetNode, index, value);

            if (value is TAssetPart)
            {
                // Check if we're the part referenced by a part design - other cases are references
                var member = targetNode as IMemberNode;
                return member == null || member.Parent.Type != typeof(TAssetPartDesign);
            }

            return base.IsObjectReference(targetNode, index, value);
        }

        /// <inheritdoc/>
        public override void RefreshBase()
        {
            base.RefreshBase();
            UpdateAssetPartBases();
        }

        /// <inheritdoc/>
        public override void RefreshBase(IAssetNode node, IAssetNode baseNode)
        {
            base.RefreshBase(node, baseNode);
            UpdateAssetPartBases();
        }

        /// <summary>
        /// Tracks the given deleted instance parts.
        /// </summary>
        /// <param name="deletedPartsMapping">A mapping of deleted parts (base part id, instance id).</param>
        public void TrackDeletedInstanceParts([NotNull] IEnumerable<Tuple<Guid, Guid>> deletedPartsMapping)
        {
            if (deletedPartsMapping == null) throw new ArgumentNullException(nameof(deletedPartsMapping));
            deletedPartsInstanceMapping.UnionWith(deletedPartsMapping);
        }

        /// <summary>
        /// Untracks the given deleted instance parts.
        /// </summary>
        /// <param name="deletedPartsMapping">A mapping of deleted parts (base part id, instance id).</param>
        public void UntrackDeletedInstanceParts([NotNull] IEnumerable<Tuple<Guid, Guid>> deletedPartsMapping)
        {
            if (deletedPartsMapping == null) throw new ArgumentNullException(nameof(deletedPartsMapping));
            deletedPartsInstanceMapping.ExceptWith(deletedPartsMapping);
        }

        /// <inheritdoc />
        protected override void FinalizeInitialization()
        {
            // Track parts that were removed in instances by comparing to the base
            foreach (var kv in basePartAssets)
            {
                var baseAsset = kv.Key.Asset;
                var instanceIds = kv.Value;
                var baseParts = baseAsset.Hierarchy.Parts.Select(p => p.Part.Id).SelectMany(basePartId => instanceIds, Tuple.Create);
                var existingParts = baseInstanceMapping.Keys;
                var deletedParts = baseParts.Except(existingParts);
                TrackDeletedInstanceParts(deletedParts);
            }
        }

        /// <summary>
        /// Retrieves the Quantum <see cref="IGraphNode"/> instances containing the child parts. These contents can be collections or single values.
        /// </summary>
        /// <param name="part">The part instance for which to retrieve the Quantum content/</param>
        /// <returns>A sequence containing all contents containing child parts.</returns>
        // TODO: this method probably doesn't need to return an enumerable, our current use case are single content only.
        protected abstract IEnumerable<IGraphNode> RetrieveChildPartNodes(TAssetPart part);

        /// <summary>
        /// Retrieves the <see cref="Guid"/> corresponding to the given part.
        /// </summary>
        /// <param name="part">The part for which to retrieve the id.</param>
        protected abstract Guid GetIdFromChildPart(object part);

        /// <summary>
        /// Adds the given child part to the list of children of the given parent part.
        /// </summary>
        /// <param name="parentPart"></param>
        /// <param name="childPart">The child part.</param>
        /// <param name="index">The index of the child part in the list of children of the parent part.</param>
        /// <remarks>This method does not modify the <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> contained in this asset.</remarks>
        protected abstract void AddChildPartToParentPart([NotNull] TAssetPart parentPart, [NotNull] TAssetPart childPart, int index);

        /// <summary>
        /// Removes the given child part from the list of children of the given parent part.
        /// </summary>
        /// <param name="parentPart"></param>
        /// <param name="childPart">The child part.</param>
        /// <remarks>This method does not modify the <see cref="AssetCompositeHierarchyData{TAssetPartDesign, TAssetPart}"/> contained in this asset.</remarks>
        protected abstract void RemoveChildPartFromParentPart([NotNull] TAssetPart parentPart, [NotNull] TAssetPart childPart);

        /// <summary>
        /// Indicates whether a new part added in a base asset should be also cloned and added to this asset.
        /// </summary>
        /// <param name="baseAssetGraph">The property graph of the base asset.</param>
        /// <param name="newPart">The new part that has been added in the base asset.</param>
        /// <param name="newPartParent">The parent of the new part that has been added in the base asset.</param>
        /// <returns><c>true</c> if the part should be cloned and added to this asset; otherwise, <c>false</c>.</returns>
        protected virtual bool ShouldAddNewPartFromBase(AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> baseAssetGraph, TAssetPartDesign newPart, TAssetPart newPartParent, Guid instanceId)
        {
            return !deletedPartsInstanceMapping.Contains(Tuple.Create(newPart.Part.Id, instanceId));
        }

        protected virtual void RewriteIds(TAssetPart targetPart, TAssetPart sourcePart)
        {
            // TODO: this method is temporary!
            targetPart.Id = sourcePart.Id;
        }

        /// <summary>
        /// Finds the best index (and parent) at which to insert a new part that is propagated after being added to one of the bases of this asset.
        /// </summary>
        /// <param name="baseAsset">The base asset for the part that has been added.</param>
        /// <param name="newBasePart">The new part that has been added to the base.</param>
        /// <param name="newBasePartParent">The parent part of the part that has been added to the base.</param>
        /// <param name="instanceId">The id of the instance for which we are looking for an index and parent.</param>
        /// <param name="instanceParent">The parent in which to insert the new instance part. If null, the new part will be inserted as root of the hierarchy.</param>
        /// <returns>The index at which to insert the new part in the instance, or a negative value if the part should be discarded.</returns>
        protected virtual int FindBestInsertIndex(AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> baseAsset, TAssetPartDesign newBasePart, TAssetPart newBasePartParent, Guid instanceId, out TAssetPartDesign instanceParent)
        {
            instanceParent = null;
            var insertIndex = -1;

            // First, let's find out where it is the best to insert this new part
            if (newBasePartParent == null)
            {
                // The part is a root, so we must place it according to its sibling (since no parent exists).
                var partIndex = baseAsset.Hierarchy.RootPartIds.IndexOf(newBasePart.Part.Id);
                // Let's try to find a sibling in the parts preceding it, in order
                for (var i = partIndex - 1; i >= 0 && insertIndex < 0; --i)
                {
                    var sibling = baseAsset.Hierarchy.Parts[baseAsset.Hierarchy.RootPartIds[i]];
                    var instanceSibling = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == sibling.Part.Id);
                    // This sibling still exists instance-side, let's get its parent.
                    if (instanceSibling != null)
                    {
                        // If the sibling itself has a parent instance-side, let's use the same parent and insert after it
                        // Otherwise the sibling is root, let's insert after it in the root parts
                        var parent = Asset.GetParent(instanceSibling.Part);
                        instanceParent = parent != null ? Asset.Hierarchy.Parts[parent.Id] : null;
                        insertIndex = Asset.IndexOf(instanceSibling.Part) + 1;
                        break;
                    }
                }

                // Let's try to find a sibling in the parts following it, in order
                for (var i = partIndex + 1; i < baseAsset.Hierarchy.RootPartIds.Count && insertIndex < 0; ++i)
                {
                    var sibling = baseAsset.Hierarchy.Parts[baseAsset.Hierarchy.RootPartIds[i]];
                    var instanceSibling = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == sibling.Part.Id);
                    // This sibling still exists instance-side, let's get its parent.
                    if (instanceSibling != null)
                    {
                        // If the sibling itself has a parent instance-side, let's use the same parent and insert after it
                        // Otherwise the sibling is root, let's insert after it in the root parts
                        var parent = Asset.GetParent(instanceSibling.Part);
                        instanceParent = parent != null ? Asset.Hierarchy.Parts[parent.Id] : null;
                        insertIndex = Asset.IndexOf(instanceSibling.Part);
                        break;
                    }
                }
            }
            else
            {
                // The new part is not root, it has a parent.
                instanceParent = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == newBasePartParent.Id);

                // If the parent has been removed instance side, the hierarchy to the new part does not exist anymore. We can discard it
                if (instanceParent != null)
                {
                    var partIndex = baseAsset.IndexOf(newBasePart.Part);

                    // Let's try to find a sibling in the parts preceding it, in order
                    for (var i = partIndex - 1; i >= 0 && insertIndex < 0; --i)
                    {
                        var sibling = baseAsset.GetChild(newBasePartParent, i);
                        var instanceSibling = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == sibling.Id);
                        // This sibling still exists instance-side, let's insert after it
                        if (instanceSibling != null)
                            insertIndex = i + 1;
                    }

                    // Let's try to find a sibling in the parts following it, in order
                    for (var i = partIndex + 1; i < baseAsset.GetChildCount(newBasePartParent) && insertIndex < 0; ++i)
                    {
                        var sibling = baseAsset.GetChild(newBasePartParent, i);
                        var instanceSibling = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == sibling.Id);
                        // This sibling still exists instance-side, let's insert before it
                        if (instanceSibling != null)
                            insertIndex = i - 1;
                    }

                    // Default position is first index
                    if (insertIndex < 0)
                        insertIndex = 0;
                }
            }

            if (insertIndex < 0)
            {
                // We couldn't find any parent/sibling in the instance. Either the parent has been removed, in which case we'll discard the part,
                // or the base is a single part that has been moved around, and we'll rely on the last known common ancestor of this instance to re-insert it.
                var isAlone = Asset.Hierarchy.Parts.All(x => x.Base?.InstanceId != instanceId);
                if (isAlone)
                {
                    Guid parentId;
                    instancesCommonAncestors.TryGetValue(instanceId, out parentId);
                    instanceParent = parentId != Guid.Empty ? Asset.Hierarchy.Parts[parentId] : null;
                    insertIndex = 0;
                }
            }
            return insertIndex;
        }

        /// <inheritdoc/>
        protected override void OnContentChanged(MemberNodeChangeEventArgs args)
        {
            RelinkToOwnerPart((IAssetNode)args.Member, args.NewValue);
            base.OnContentChanged(args);
        }

        /// <inheritdoc/>
        protected override void OnItemChanged(ItemChangeEventArgs args)
        {
            RelinkToOwnerPart((IAssetNode)args.Collection, args.NewValue);
            base.OnItemChanged(args);
        }

        private void RelinkToOwnerPart(IAssetNode node, object newValue)
        {
            var partDesign = (TAssetPartDesign)node.GetContent(NodesToOwnerPartVisitor.OwnerPartContentName)?.Retrieve();
            if (partDesign != null)
            {
                // A property of a part has changed
                LinkToOwnerPart(node, partDesign);
            }
            else if (node.Type == typeof(AssetPartCollection<TAssetPartDesign, TAssetPart>) && newValue is TAssetPartDesign)
            {
                // A new part has been added
                partDesign = (TAssetPartDesign)newValue;
                LinkToOwnerPart(Container.NodeContainer.GetNode(partDesign.Part), partDesign);
            }
        }

        private void UpdateAssetPartBases()
        {
            // We need to subscribe to event of new base assets, but we don't want to unregister from previous one, in case the user is moving (remove + add)
            // the single part of a base. In this case we wouldn't have any part linking to the base once it has been removed.
            var newBasePartAsset = new HashSet<AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>>();

            // We want to enumerate parts that are actually "reachable", so we don't use Hierarchy.Parts for iteration - we iterate from the root parts instead.
            // We use Hierarchy.Parts at the end just to retrieve the part design from the actual part.
            var currentParts = Asset.Hierarchy.RootPartIds.Select(x => Asset.Hierarchy.Parts[x].Part).DepthFirst(x => Asset.EnumerateChildParts(x, false)).Select(x => Asset.Hierarchy.Parts[x.Id]);
            foreach (var part in currentParts.Where(x => x.Base != null))
            {
                var baseAssetGraph = Container.GetGraph(part.Base.BasePartAsset.Id) as AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>;
                if (baseAssetGraph != null)
                {
                    HashSet<Guid> instanceIds;
                    if (!basePartAssets.TryGetValue(baseAssetGraph, out instanceIds))
                    {
                        instanceIds = new HashSet<Guid>();
                        basePartAssets.Add(baseAssetGraph, instanceIds);
                        newBasePartAsset.Add(baseAssetGraph);
                    }
                    instanceIds.Add(part.Base.InstanceId);
                }

                // Update mapping
                baseInstanceMapping[Tuple.Create(part.Base.BasePartId, part.Base.InstanceId)] = part.Part;

                // Update common ancestors
                Guid ancestorId;
                if (!instancesCommonAncestors.TryGetValue(part.Base.InstanceId, out ancestorId))
                {
                    instancesCommonAncestors[part.Base.InstanceId] = Asset.GetParent(part.Part)?.Id ?? Guid.Empty;
                }
                else
                {
                    var parent = ancestorId;
                    var parents = new HashSet<Guid>();
                    while (parent != Guid.Empty)
                    {
                        parents.Add(parent);
                        parent = Asset.GetParent(Asset.Hierarchy.Parts[parent].Part)?.Id ?? Guid.Empty;
                    }
                    ancestorId = Asset.GetParent(part.Part)?.Id ?? Guid.Empty;
                    while (ancestorId != Guid.Empty && !parents.Contains(ancestorId))
                    {
                        ancestorId = Asset.GetParent(Asset.Hierarchy.Parts[ancestorId].Part)?.Id ?? Guid.Empty;
                    }
                    instancesCommonAncestors[part.Base.InstanceId] = ancestorId;
                }
            }

            // Register to new base part events
            foreach (var basePartAsset in newBasePartAsset)
            {
                basePartAsset.PartAdded += PartAddedInBaseAsset;
                basePartAsset.PartRemoved += PartRemovedInBaseAsset;
            }
        }

        private void PartAddedInBaseAsset(object sender, AssetPartChangeEventArgs e)
        {
            UpdatingPropertyFromBase = true;

            var baseAsset = (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)e.Asset;
            var newPart = baseAsset.Hierarchy.Parts[e.PartId];
            var newPartParent = baseAsset.GetParent(newPart.Part);
            var baseAssetGraph = Container.GetGraph(baseAsset.Id) as AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>;
            if (baseAssetGraph == null) throw new InvalidOperationException("Unable to find the graph corresponding to the base part");


            foreach (var instanceId in basePartAssets[baseAssetGraph])
            {
                // Discard the part if this asset don't want it
                if (!ShouldAddNewPartFromBase(baseAssetGraph, newPart, newPartParent, instanceId))
                    continue;

                TAssetPartDesign instanceParent;
                var insertIndex = FindBestInsertIndex(baseAsset, newPart, newPartParent, instanceId, out instanceParent);
                if (insertIndex < 0)
                    continue;

                // Now we know where to insert, let's clone the new part.
                Dictionary<Guid, Guid> mapping;
                var flags = SubHierarchyCloneFlags.GenerateNewIdsForIdentifiableObjects | SubHierarchyCloneFlags.RemoveOverrides;
                var baseHierarchy = baseAssetGraph.CloneSubHierarchy(newPart.Part.Id, flags, out mapping);
                foreach (var ids in mapping)
                {
                    TAssetPartDesign clone;
                    // Process only ids that correspond to parts
                    if (!baseHierarchy.Parts.TryGetValue(ids.Value, out clone))
                        continue;

                    clone.Base = new BasePart(new AssetReference(e.AssetItem.Id, e.AssetItem.Location), ids.Key, instanceId);

                    TAssetPart existingPart;

                    // This add could actually be a move (remove + add). So we compare to the existing baseInstanceMapping and perform another remap if necessary
                    if (baseInstanceMapping.TryGetValue(Tuple.Create(ids.Key, instanceId), out existingPart))
                    {
                        // Replace the cloned part by the one to restore in the list of root if needed
                        if (baseHierarchy.RootPartIds.Remove(clone.Part.Id))
                            baseHierarchy.RootPartIds.Add(existingPart.Id);

                        // Overwrite the Ids of the cloned part with the id of the existing one so the cloned part will be considered as a proxy object by the fix reference pass
                        RewriteIds(clone.Part, existingPart);
                        // Replace the cloned part itself by the existing part.
                        var part = Container.NodeContainer.GetNode(clone);
                        part[PartName].Update(existingPart);
                    }
                }

                // We might have changed some ids, let's resort
                baseHierarchy.Parts.Sort();

                // Then actually add the new part
                var rootClone = baseHierarchy.Parts[baseHierarchy.RootPartIds.Single()];
                AddPartToAsset(baseHierarchy.Parts, rootClone, instanceParent?.Part, insertIndex);
            }

            // Reconcile with base
            RefreshBase();
            ReconcileWithBase();

            UpdatingPropertyFromBase = false;
        }

        private void PartRemovedInBaseAsset(object sender, AssetPartChangeEventArgs e)
        {
            UpdatingPropertyFromBase = true;
            foreach (var part in Asset.Hierarchy.Parts.Where(x => x.Base?.BasePartId == e.PartId).ToList())
            {
                RemovePartFromAsset(part);
            }
            UpdatingPropertyFromBase = false;
        }

        private void InsertPartInPartsCollection(AssetPartCollection<TAssetPartDesign, TAssetPart> newPartCollection, TAssetPartDesign rootPart)
        {
            var node = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.Parts)].Target;
            node.Add(rootPart);
            foreach (var childPart in Asset.EnumerateChildParts(rootPart.Part, false))
            {
                var partDesign = newPartCollection[childPart.Id];
                InsertPartInPartsCollection(newPartCollection, partDesign);
            }
        }

        private void RemovePartFromPartsCollection(TAssetPartDesign rootPart)
        {
            foreach (var childPart in Asset.EnumerateChildParts(rootPart.Part, false))
            {
                var partDesign = Asset.Hierarchy.Parts[childPart.Id];
                RemovePartFromPartsCollection(partDesign);
            }
            var node = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.Parts)].Target;
            var index = new Index(Asset.Hierarchy.Parts.IndexOf(rootPart));
            node.Remove(rootPart, index);
        }

        private void NotifyPartAdded(Guid partId)
        {
            UpdateAssetPartBases();
            PartAdded?.Invoke(this, new AssetPartChangeEventArgs(AssetItem, partId));
        }

        private void NotifyPartRemoved(Guid partId)
        {
            UpdateAssetPartBases();
            PartRemoved?.Invoke(this, new AssetPartChangeEventArgs(AssetItem, partId));
        }

        private void RootPartsChanged(object sender, INodeChangeEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ContentChangeType.CollectionAdd:
                    NotifyPartAdded((Guid)e.NewValue);
                    break;
                case ContentChangeType.CollectionRemove:
                    NotifyPartRemoved((Guid)e.OldValue);
                    break;
            }
        }

        private void ChildPartChanged(object sender, INodeChangeEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ContentChangeType.CollectionUpdate:
                case ContentChangeType.ValueChange:
                    if (e.OldValue != null)
                    {
                        NotifyPartRemoved(GetIdFromChildPart(e.OldValue));
                    }
                    if (e.NewValue != null)
                    {
                        NotifyPartAdded(GetIdFromChildPart(e.NewValue));
                    }
                    break;
                case ContentChangeType.CollectionAdd:
                    NotifyPartAdded(GetIdFromChildPart(e.NewValue));
                    break;
                case ContentChangeType.CollectionRemove:
                    NotifyPartRemoved(GetIdFromChildPart(e.OldValue));
                    break;
            }
        }

        private void PartsChanged(object sender, ItemChangeEventArgs e)
        {
            TAssetPart part;
            switch (e.ChangeType)
            {
                case ContentChangeType.CollectionAdd:
                    // Ensure that we track children added later to any new part
                    part = ((TAssetPartDesign)e.NewValue).Part;
                    foreach (var childPartNode in RetrieveChildPartNodes(part))
                    {
                        RegisterChildPartNode(childPartNode);
                    }
                    break;
                case ContentChangeType.CollectionRemove:
                    // And untrack removed parts
                    part = ((TAssetPartDesign)e.OldValue).Part;
                    foreach (var childPartNode in RetrieveChildPartNodes(part))
                    {
                        UnregisterChildPartNode(childPartNode);
                    }
                    break;
            }
        }

        private void RegisterChildPartNode(IGraphNode node)
        {
            if (registeredChildParts.Add(node))
            {
                var memberNode = node as IMemberNode;
                if (memberNode != null)
                {
                    memberNode.ValueChanged += ChildPartChanged;
                }
                var objectNode = node as IObjectNode;
                if (objectNode != null)
                {
                    objectNode.ItemChanged += ChildPartChanged;
                }
            }
        }

        private void UnregisterChildPartNode(IGraphNode node)
        {
            if (registeredChildParts.Remove(node))
            {
                var memberNode = node as IMemberNode;
                if (memberNode != null)
                {
                    memberNode.ValueChanged -= ChildPartChanged;
                }
                var objectNode = node as IObjectNode;
                if (objectNode != null)
                {
                    objectNode.ItemChanged -= ChildPartChanged;
                }
            }
        }
    }
}
