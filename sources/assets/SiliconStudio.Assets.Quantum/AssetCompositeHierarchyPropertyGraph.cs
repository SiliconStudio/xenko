using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public abstract class AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> : AssetCompositePropertyGraph
        where TAssetPart : class, IIdentifiable
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
    {
        protected AssetCompositeHierarchyPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
            HierarchyNode = RootNode[nameof(AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>.Hierarchy)].Target;
        }

        public AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> AssetHierarchy => (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)Asset;

        protected IObjectNode HierarchyNode { get; }

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
                var rootEntitiesNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)];
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
                RemoveChildPartFromParentPart(parent, partDesign.Part);
            }
            else
            {
                var index = new Index(AssetHierarchy.Hierarchy.RootPartIds.IndexOf(partDesign.Part.Id));
                var rootPartsNode = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.RootPartIds)];
                rootPartsNode.Remove(partDesign.Part.Id, index);
            }
            RemovePartFromPartsCollection(partDesign);
        }

        public override IContentNode FindTarget(IContentNode sourceNode, IContentNode target)
        {
            // TODO: try to generalize what the overrides of this implementation are doing.
            // Connect the parts to their base if any.
            var part = sourceNode.Value as TAssetPart;
            if (part != null && sourceNode is IObjectNode)
            {
                TAssetPartDesign partDesign;
                // The part might be being moved and could possibly be currently not into the Parts collection.
                if (AssetHierarchy.Hierarchy.Parts.TryGetValue(part.Id, out partDesign) && partDesign.Base != null)
                {
                    var baseAsset = Container.GetAssetById(partDesign.Base.BasePartAsset.Id);
                    // Base prefab might have been deleted
                    if (baseAsset == null)
                        return base.FindTarget(sourceNode, target);

                    // Part might have been deleted in base asset
                    TAssetPartDesign basePart;
                    ((AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)baseAsset.Asset).Hierarchy.Parts.TryGetValue(partDesign.Base.BasePartId, out basePart);
                    return basePart != null ? Container.NodeContainer.GetOrCreateNode(basePart.Part) : base.FindTarget(sourceNode, target);
                }
            }

            return base.FindTarget(sourceNode, target);
        }

        protected internal override object CloneValueFromBase(object value, IAssetNode node)
        {
            var part = value as TAssetPart;
            // Part reference
            if (part != null)
            {
                // We need to find out for which entity we are cloning this (other) entity
                var owner = (TAssetPartDesign)node?.GetContent(NodesToOwnerPartVisitor.OwnerPartContentName).Retrieve();
                if (owner != null)
                {
                    // Then instead of creating a clone, we just return the corresponding part in this asset (in term of base and base instance)
                    var partInDerived = AssetHierarchy.Hierarchy.Parts.FirstOrDefault(x => x.Base?.BasePartId == part.Id && x.Base?.InstanceId == owner.Base?.InstanceId);
                    return partInDerived?.Part;
                }
            }

            var result = base.CloneValueFromBase(value, node);
            return result;
        }

        public override GraphVisitorBase CreateReconcilierVisitor()
        {
            return new AssetCompositeHierarchyPartVisitor<TAssetPartDesign, TAssetPart>(this);
        }

        public override bool IsReferencedPart(IMemberNode member, IContentNode targetNode)
        {
            // If we're not accessing the target node through a member (eg. the target node is the root node of the visit)
            // or if we're visiting the member itself and not yet its target, then we're not a referenced part.
            if (member == null || targetNode == null || member == targetNode)
                return false;

            if (typeof(TAssetPart).IsAssignableFrom(targetNode.Type))
            {
                // Check if we're the part referenced by a part design - other cases are references
                return member.Parent.Type != typeof(TAssetPartDesign);
            }
            return base.IsReferencedPart(member, targetNode);
        }

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

        private void InsertPartInPartsCollection(AssetPartCollection<TAssetPartDesign, TAssetPart> newPartCollection, TAssetPartDesign rootPart)
        {
            var node = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.Parts)];
            node.Add(rootPart);
            foreach (var childPart in AssetHierarchy.EnumerateChildParts(rootPart.Part, true))
            {
                var partDesign = newPartCollection[childPart.Id];
                InsertPartInPartsCollection(newPartCollection, partDesign);
            }
        }

        private void RemovePartFromPartsCollection(TAssetPartDesign rootPart)
        {
            foreach (var childPart in AssetHierarchy.EnumerateChildParts(rootPart.Part, true))
            {
                var partDesign = AssetHierarchy.Hierarchy.Parts[childPart.Id];
                RemovePartFromPartsCollection(partDesign);
            }
            var node = HierarchyNode[nameof(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>.Parts)];
            var index = new Index(AssetHierarchy.Hierarchy.Parts.IndexOf(rootPart));
            node.Remove(rootPart, index);
        }
    }
}
