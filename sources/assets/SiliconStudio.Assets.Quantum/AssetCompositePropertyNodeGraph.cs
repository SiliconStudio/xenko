using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyNodeGraph(typeof(AssetComposite))]
    public class AssetCompositePropertyNodeGraph<TAssetPartDesign, TAssetPart> : AssetPropertyNodeGraph
        where TAssetPart : class, IIdentifiable
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
    {
        public AssetCompositePropertyNodeGraph(AssetPropertyNodeGraphContainer container, AssetItem assetItem)
            : base(container, assetItem)
        {
        }

        public override bool ShouldListenToTargetNode(MemberContent member, IGraphNode targetNode)
        {
            // Make sure it's actually a target (not a member) node.
            return !IsReferencedPart(member, targetNode) && base.ShouldListenToTargetNode(member, targetNode);
        }

        public virtual bool IsReferencedPart(MemberContent member, IGraphNode targetNode)
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
            return false;
        }
    }

    [AssetPropertyNodeGraph(typeof(AssetCompositeHierarchy<,>))]
    public class AssetCompositeHierarchyPropertyNodeGraph<TAssetPartDesign, TAssetPart> : AssetCompositePropertyNodeGraph<TAssetPartDesign, TAssetPart>
        where TAssetPart : class, IIdentifiable
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
    {
        public AssetCompositeHierarchyPropertyNodeGraph(AssetPropertyNodeGraphContainer container, AssetItem assetItem)
            : base(container, assetItem)
        {
        }

        public AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> AssetHierarchy => (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)assetItem.Asset;

        public override IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
        {
            // TODO: try to generalize what the overrides of this implementation are doing.
            // Connect the entities to their base that can be entities from a prefab
            var part = sourceNode.Content.Value as TAssetPart;
            if (part != null && sourceNode.Content is ObjectContent)
            {
                TAssetPartDesign partDesign;
                // The entity might be being moved and could possibly be currently not into the Parts collection.
                if (AssetHierarchy.Hierarchy.Parts.TryGetValue(part.Id, out partDesign) && partDesign.BaseId.HasValue && partDesign.BasePartInstanceId.HasValue)
                {
                    var baseAsset = AssetHierarchy.BaseParts?.Select(x => x.Asset).OfType<AssetComposite>().FirstOrDefault(x => x.ContainsPart(partDesign.BaseId.Value));
                    if (baseAsset != null)
                    {
                        var basePrefab = Container.GetAssetById(baseAsset.Id);
                        // Base prefab might have been deleted
                        if (basePrefab == null)
                            return base.FindTarget(sourceNode, target);

                        // Entity might have been deleted in base prefab
                        TAssetPartDesign basePart;
                        ((AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)basePrefab.Asset).Hierarchy.Parts.TryGetValue(partDesign.BaseId.Value, out basePart);
                        return basePart != null ? Container.NodeContainer.GetOrCreateNode(basePart.Part) : base.FindTarget(sourceNode, target);
                    }
                }
            }

            return base.FindTarget(sourceNode, target);
        }

    }


}
