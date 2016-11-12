using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyGraph(typeof(AssetCompositeHierarchy<,>))]
    public class AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> : AssetCompositePropertyGraph<TAssetPartDesign, TAssetPart>
        where TAssetPart : class, IIdentifiable
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
    {
        public AssetCompositeHierarchyPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
        }

        public AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> AssetHierarchy => (AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)AssetItem.Asset;

        public override IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
        {
            // TODO: try to generalize what the overrides of this implementation are doing.
            // Connect the entities to their base that can be entities from a prefab
            var part = sourceNode.Content.Value as TAssetPart;
            if (part != null && sourceNode.Content is ObjectContent)
            {
                TAssetPartDesign partDesign;
                // The entity might be being moved and could possibly be currently not into the Parts collection.
                if (AssetHierarchy.Hierarchy.Parts.TryGetValue(part.Id, out partDesign) && partDesign.Base != null)
                {
                    var basePrefab = Container.GetAssetById(partDesign.Base.BasePartAsset.Id);
                    // Base prefab might have been deleted
                    if (basePrefab == null)
                        return base.FindTarget(sourceNode, target);

                    // Entity might have been deleted in base prefab
                    TAssetPartDesign basePart;
                    ((AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>)basePrefab.Asset).Hierarchy.Parts.TryGetValue(partDesign.Base.BasePartId, out basePart);
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
                var ownerEntity = (TAssetPartDesign)multiContentNode?.GetContent(NodesToOwnerPartVisitor.OwnerPartContentName).Retrieve();
                if (ownerEntity != null)
                {
                    // Then instead of creating a clone, we just return the corresponding part in this asset (in term of base and base instance)
                    var partInDerived = AssetHierarchy.Hierarchy.Parts.FirstOrDefault(x => x.Base?.BasePartId == part.Id && x.Base?.InstanceId == ownerEntity.Base?.InstanceId);
                    return partInDerived?.Part;
                }
            }

            var result = base.CloneValueFromBase(value, node);
            return result;
        }
    }
}