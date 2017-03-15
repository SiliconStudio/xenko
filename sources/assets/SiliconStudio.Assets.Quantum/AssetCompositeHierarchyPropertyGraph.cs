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
        /// A dictionary mapping instance ids to the common ancestor of the parts corresponding to that instance id in this asset.
        /// </summary>
        /// <remarks>This dictionary is used to remember where the prefab instance was located, if during some time all its parts are removed, for example during some specific operaiton in the base asset.</remarks>
        private readonly Dictionary<Guid, Guid> instancesCommonAncestors = new Dictionary<Guid, Guid>();

        protected AssetCompositeHierarchyPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
            HierarchyNode = RootNode[nameof(AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>.Hierarchy)].Target;
            var rootPartsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)].Target;
            rootPartsNode.ItemChanged += RootPartsChanged;
            foreach (var childPartNode in Asset.Hierarchy.Parts.SelectMany(x => RetrieveChildPartNodes(x.Part)))
            {
                childPartNode.RegisterChanged(ChildPartChanged);
            }
            var partsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<IAssetPartDesign<IIdentifiable>, IIdentifiable>.Parts)].Target;
            partsNode.ItemChanged += PartsChanged;

            foreach (var part in Asset.Hierarchy.Parts)
            {
                LinkToOwnerPart(Container.NodeContainer.GetNode(part.Part), part);
            }
        }

        public AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> AssetHierarchy => Asset;

        internal new AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> Asset => (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)base.Asset;

        protected IObjectNode HierarchyNode { get; }

        /// <summary>
        /// Raised when a part is added to this asset.
        /// </summary>
        public event EventHandler<AssetPartChangeEventArgs> PartAdded;

        /// <summary>
        /// Raised when a part is removed from this asset.
        /// </summary>
        public event EventHandler<AssetPartChangeEventArgs> PartRemoved;

        public abstract bool IsChildPartReference(IGraphNode node, Index index);

        public override void Dispose()
        {
            base.Dispose();
            var rootPartsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)].Target;
            rootPartsNode.ItemChanged -= RootPartsChanged;
            foreach (var childPartNode in Asset.Hierarchy.Parts.SelectMany(x => RetrieveChildPartNodes(x.Part)))
            {
                childPartNode.UnregisterChanged(ChildPartChanged);
            }
            var partsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<IAssetPartDesign<IIdentifiable>, IIdentifiable>.Parts)].Target;
            partsNode.ItemChanged -= PartsChanged;
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
                // We must refresh the base to stop further update from the prefab to the instance entities
                RefreshBase(node, (IAssetNode)node.BaseNode);
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
                var rootEntitiesNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)].Target;
                rootEntitiesNode.Add(child.Part.Id, new Index(index));
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
            if (!AssetHierarchy.Hierarchy.RootPartIds.Contains(partDesign.Part.Id))
            {
                var parent = AssetHierarchy.GetParent(partDesign.Part);
                if (parent == null) throw new InvalidOperationException("The part has no parent but is not in the RootPartIds collection.");
                RemoveChildPartFromParentPart(parent, partDesign.Part);
            }
            else
            {
                var index = new Index(AssetHierarchy.Hierarchy.RootPartIds.IndexOf(partDesign.Part.Id));
                var rootPartsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)].Target;
                rootPartsNode.Remove(partDesign.Part.Id, index);
            }
            RemovePartFromPartsCollection(partDesign);
        }

        /// <summary>
        /// Deletes the given parts and all its children, recursively, and clear all object references to it.
        /// </summary>
        /// <param name="parts">The parts to delete.</param>
        public virtual void DeleteParts([NotNull] IEnumerable<TAssetPart> parts)
        {
            if (parts == null) throw new ArgumentNullException(nameof(parts));
            var partsToDelete = new Stack<TAssetPart>(parts);
            var referencesToClear = new HashSet<Guid>();
            while (partsToDelete.Count > 0)
            {
                // We need to remove children first to keep consistency in our data
                var partToDelete = partsToDelete.Peek();
                var children = AssetHierarchy.EnumerateChildParts(partToDelete, false).ToList();
                if (children.Count > 0)
                {
                    // Enqueue children if there is any, and re-process the stack
                    children.ForEach(x => partsToDelete.Push(x));
                    continue;
                }
                // No children to process, we can safely remove the current entity from the stack
                partToDelete = partsToDelete.Pop();
                // First remove all references to the entity (and its component!) we are deleting
                // Note: we must do this first so instances of this prefabs will be able to properly make the connection with the base entity being cleared
                var containedIdentifiables = IdentifiableObjectCollector.Collect(this, Container.NodeContainer.GetNode(partToDelete));
                containedIdentifiables.Keys.ForEach(x => referencesToClear.Add(x));
                // Then actually remove the entity from the hierarchy
                RemovePartFromAsset(AssetHierarchy.Hierarchy.Parts[partToDelete.Id]);
            }
            ClearReferencesToObjects(referencesToClear);
        }

        public override IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
        {
            // TODO: try to generalize what the overrides of this implementation are doing.
            // Connect the parts to their base if any.
            var part = sourceNode.Retrieve() as TAssetPart;
            if (part != null && sourceNode is IObjectNode)
            {
                TAssetPartDesign partDesign;
                // The part might be being moved and could possibly be currently not into the Parts collection.
                if (AssetHierarchy.Hierarchy.Parts.TryGetValue(part.Id, out partDesign) && partDesign.Base != null)
                {
                    var baseAssetGraph = Container.GetGraph(partDesign.Base.BasePartAsset.Id);
                    // Base prefab might have been deleted
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
                if (!AssetHierarchy.Hierarchy.Parts.ContainsKey(rootId))
                    throw new ArgumentException(@"The source root parts must be parts of this asset.", nameof(sourceRootIds));

                subTreeHierarchy.RootPartIds.Add(rootId);

                subTreeHierarchy.Parts.Add(AssetHierarchy.Hierarchy.Parts[rootId]);
                foreach (var subTreePart in AssetHierarchy.EnumerateChildParts(AssetHierarchy.Hierarchy.Parts[rootId].Part, true))
                    subTreeHierarchy.Parts.Add(AssetHierarchy.Hierarchy.Parts[subTreePart.Id]);
            }

            var preCloningAsset = (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)Activator.CreateInstance(AssetHierarchy.GetType());
            preCloningAsset.Hierarchy = subTreeHierarchy;
            var preCloningAssetGraph = (AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>)AssetQuantumRegistry.ConstructPropertyGraph(Container, new AssetItem("", preCloningAsset), null);
            var externalReferences = ExternalReferenceCollector.GetExternalReferences(preCloningAssetGraph, preCloningAssetGraph.RootNode);
            YamlAssetMetadata<OverrideType> overrides = null;
            if ((flags & SubHierarchyCloneFlags.RemoveOverrides) == 0)
                overrides = GenerateOverridesForSerialization(preCloningAssetGraph.RootNode);

            // clone the parts of the sub-tree
            var clonerFlags = AssetClonerFlags.None;

            if ((flags & SubHierarchyCloneFlags.GenerateNewIdsForIdentifiableObjects) != 0)
                clonerFlags |= AssetClonerFlags.GenerateNewIdsForIdentifiableObjects;
            if ((flags & SubHierarchyCloneFlags.CleanExternalReferences) != 0)
                clonerFlags |= AssetClonerFlags.ClearExternalReferences;

            var clonedHierarchy = AssetCloner.Clone(subTreeHierarchy, clonerFlags, externalReferences, out idRemapping);
            preCloningAssetGraph.RootNode[nameof(AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>.Hierarchy)].Update(clonedHierarchy);
            if ((flags & SubHierarchyCloneFlags.RemoveOverrides) == 0)
                ApplyOverrides(preCloningAssetGraph.RootNode, overrides);

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

        /// <summary>
        /// Retrieves the Quantum <see cref="IGraphNode"/> instances containing the child parts. These contents can be collections or single values.
        /// </summary>
        /// <param name="part">The part instance for which to retrieve the Quantum content/</param>
        /// <returns>A sequence containing all contents containing child parts.</returns>
        // TODO: this method probably don't need to retuern an enumerable, our current use case are single content only.
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
        protected virtual bool ShouldAddNewPartFromBase(AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> baseAssetGraph, TAssetPartDesign newPart, TAssetPart newPartParent)
        {
            return true;
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
                // Let's try to find a sibling in the entities preceding it, in order
                for (var i = partIndex - 1; i >= 0 && insertIndex < 0; --i)
                {
                    var sibling = baseAsset.Hierarchy.Parts[baseAsset.Hierarchy.RootPartIds[i]];
                    var instanceSibling = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == sibling.Part.Id);
                    // This sibling still exists instance-side, let's get its parent.
                    if (instanceSibling != null)
                    {
                        // If the sibling itself has a parent instance-side, let's use the same parent and insert after it
                        // Otherwise the sibling is root, let's insert after it in the root entities
                        var parent = Asset.GetParent(instanceSibling.Part);
                        instanceParent = parent != null ? Asset.Hierarchy.Parts[parent.Id] : null;
                        insertIndex = Asset.IndexOf(instanceSibling.Part) + 1;
                        break;
                    }
                }

                // Let's try to find a sibling in the entities following it, in order
                for (var i = partIndex + 1; i < baseAsset.Hierarchy.RootPartIds.Count && insertIndex < 0; ++i)
                {
                    var sibling = baseAsset.Hierarchy.Parts[baseAsset.Hierarchy.RootPartIds[i]];
                    var instanceSibling = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == sibling.Part.Id);
                    // This sibling still exists instance-side, let's get its parent.
                    if (instanceSibling != null)
                    {
                        // If the sibling itself has a parent instance-side, let's use the same parent and insert after it
                        // Otherwise the sibling is root, let's insert after it in the root entities
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

                    // Let's try to find a sibling in the entities preceding it, in order
                    for (var i = partIndex - 1; i >= 0 && insertIndex < 0; --i)
                    {
                        var sibling = baseAsset.GetChild(newBasePartParent, i);
                        var instanceSibling = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == sibling.Id);
                        // This sibling still exists instance-side, let's insert after it
                        if (instanceSibling != null)
                            insertIndex = i + 1;
                    }

                    // Let's try to find a sibling in the entities following it, in order
                    for (var i = partIndex + 1; i < baseAsset.GetChildCount(newBasePartParent) && insertIndex < 0; ++i)
                    {
                        var sibling = baseAsset.GetChild(newBasePartParent, i);
                        var instanceSibling = Asset.Hierarchy.Parts.FirstOrDefault(x => x.Base?.InstanceId == instanceId && x.Base?.BasePartId == sibling.Id);
                        // This sibling still exists instance-side, let's before after it
                        if (instanceSibling != null)
                            insertIndex = i + 1;
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

        protected override void OnContentChanged(MemberNodeChangeEventArgs args)
        {
            RelinkToOwnerPart((IAssetNode)args.Member, args.NewValue);
            base.OnContentChanged(args);
        }

        protected override void OnItemChanged(ItemChangeEventArgs args)
        {
            RelinkToOwnerPart((IAssetNode)args.Node, args.NewValue);
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
            foreach (var basePartAsset in basePartAssets.Keys)
            {
                basePartAsset.PartAdded -= PartAddedInBaseAsset;
                basePartAsset.PartRemoved -= PartRemovedInBaseAsset;
            }

            UpdatePartBases();

            foreach (var basePartAsset in basePartAssets.Keys)
            {
                basePartAsset.PartAdded += PartAddedInBaseAsset;
                basePartAsset.PartRemoved += PartRemovedInBaseAsset;
            }
        }


        private void UpdatePartBases()
        {
            basePartAssets.Clear();
            instancesCommonAncestors.Clear();

            foreach (var part in Asset.Hierarchy.Parts)
            {
                if (part.Base != null)
                {
                    var baseAssetGraph = Container.GetGraph(part.Base.BasePartAsset.Id) as AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>;
                    if (baseAssetGraph != null)
                    {
                        HashSet<Guid> instanceIds;
                        if (!basePartAssets.TryGetValue(baseAssetGraph, out instanceIds))
                        {
                            instanceIds = new HashSet<Guid>();
                            basePartAssets.Add(baseAssetGraph, instanceIds);
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
            }
        }

        private void PartAddedInBaseAsset(object sender, AssetPartChangeEventArgs e)
        {
            UpdatingPropertyFromBase = true;

            var baseAsset = (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)e.Asset;
            var newPart = baseAsset.Hierarchy.Parts[e.PartId];
            var newPartParent = baseAsset.GetParent(newPart.Part);
            var baseAssetGraph = Container.GetGraph(baseAsset.Id) as AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart>;
            if (baseAssetGraph == null) throw new InvalidOperationException("Unable to find the view model corresponding to the base part");

            // Discard the part if this asset don't want it
            // Note: we still need to add it to the base to keep bases in sync since they are suppose to contain full assets.
            if (!ShouldAddNewPartFromBase(baseAssetGraph, newPart, newPartParent))
                return;

            foreach (var instanceId in basePartAssets[baseAssetGraph])
            {
                TAssetPartDesign instanceParent;
                var insertIndex = FindBestInsertIndex(baseAsset, newPart, newPartParent, instanceId, out instanceParent);
                if (insertIndex < 0)
                    continue;

                // Now we know where to insert, let's clone the new part.
                Dictionary<Guid, Guid> mapping;
                var flags = SubHierarchyCloneFlags.GenerateNewIdsForIdentifiableObjects | SubHierarchyCloneFlags.RemoveOverrides;
                var prefabHierarchy = baseAssetGraph.CloneSubHierarchy(newPart.Part.Id, flags, out mapping);
                foreach (var ids in mapping)
                {
                    TAssetPartDesign clone;
                    // Process only ids that correspond to parts
                    if (!prefabHierarchy.Parts.TryGetValue(ids.Value, out clone))
                        continue;

                    clone.Base = new BasePart(new AssetReference(e.AssetItem.Id, e.AssetItem.Location), ids.Key, instanceId);

                    TAssetPart existingPart;

                    // This add could actually be a move (remove + add). So we compare to the existing baseInstanceMapping and perform another remap if necessary
                    if (baseInstanceMapping.TryGetValue(Tuple.Create(ids.Key, instanceId), out existingPart))
                    {
                        // Replace the cloned part by the one to restore in the list of root if needed
                        if (prefabHierarchy.RootPartIds.Remove(clone.Part.Id))
                            prefabHierarchy.RootPartIds.Add(existingPart.Id);

                        // Overwrite the Ids of the cloned part with the id of the existing one so the cloned part will be considered as a proxy object by the fix reference pass
                        RewriteIds(clone.Part, existingPart);
                        // Replace the cloned part itself by the existing part.
                        clone.Part = existingPart;
                    }
                }

                // We might have changed some ids, let's resort
                prefabHierarchy.Parts.Sort();

                // Then actually add the new part
                var rootClone = prefabHierarchy.Parts[prefabHierarchy.RootPartIds.Single()];
                AddPartToAsset(prefabHierarchy.Parts, rootClone, instanceParent?.Part, insertIndex);
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
            foreach (var childPart in AssetHierarchy.EnumerateChildParts(rootPart.Part, false))
            {
                var partDesign = newPartCollection[childPart.Id];
                InsertPartInPartsCollection(newPartCollection, partDesign);
            }
        }

        private void RemovePartFromPartsCollection(TAssetPartDesign rootPart)
        {
            foreach (var childPart in AssetHierarchy.EnumerateChildParts(rootPart.Part, false))
            {
                var partDesign = AssetHierarchy.Hierarchy.Parts[childPart.Id];
                RemovePartFromPartsCollection(partDesign);
            }
            var node = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.Parts)].Target;
            var index = new Index(AssetHierarchy.Hierarchy.Parts.IndexOf(rootPart));
            node.Remove(rootPart, index);
        }

        private void NotifyPartAdded(Guid partId)
        {
            PartAdded?.Invoke(this, new AssetPartChangeEventArgs(AssetItem, partId));
        }

        private void NotifyPartRemoved(Guid partId)
        {
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
                        childPartNode.RegisterChanged(ChildPartChanged);
                    }
                    break;
                case ContentChangeType.CollectionRemove:
                    // And untrack removed parts
                    part = ((TAssetPartDesign)e.OldValue).Part;
                    foreach (var childPartNode in RetrieveChildPartNodes(part))
                    {
                        childPartNode.UnregisterChanged(ChildPartChanged);
                    }
                    break;
            }
        }
    }
}
