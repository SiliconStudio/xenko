using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyGraph(typeof(AssetCompositeHierarchy<,>))]
    public class AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> : AssetCompositePropertyGraph
        where TAssetPart : class, IIdentifiable
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
    {
        public AssetCompositeHierarchyPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
        }

        public AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> AssetHierarchy => (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)Asset;

        public override IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
        {
            // TODO: try to generalize what the overrides of this implementation are doing.
            // Connect the parts to their base if any.
            var part = sourceNode.Content.Value as TAssetPart;
            if (part != null && sourceNode.Content is ObjectContent)
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

        protected override object CloneValueFromBase(object value, AssetNode node)
        {
            var part = value as TAssetPart;
            // Part reference
            if (part != null)
            {
                // We need to find out for which entity we are cloning this (other) entity
                var multiContentNode = node as MultiContentNode;
                var owner = (TAssetPartDesign)multiContentNode?.GetContent(NodesToOwnerPartVisitor.OwnerPartContentName).Retrieve();
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

        protected override GraphVisitorBase CreateReconcilierVisitor()
        {
            return new AssetCompositeHierarchyPartVisitor<TAssetPartDesign, TAssetPart>(this);
        }

        public override bool IsReferencedPart(MemberContent member, IGraphNode targetNode)
        {
            // If we're not accessing the target node through a member (eg. the target node is the root node of the visit)
            // or if we're visiting the member itself and not yet its target, then we're not a referenced part.
            if (member == null || member == targetNode.Content)
                return false;

            if (typeof(TAssetPart).IsAssignableFrom(targetNode.Content.Type))
            {
                // Check if we're the part referenced by a part design - other cases are references
                return member.Container.OwnerNode.Content.Type != typeof(TAssetPartDesign);
            }
            return base.IsReferencedPart(member, targetNode);
        }
    }
}
