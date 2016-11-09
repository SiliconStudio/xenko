using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyGraph(typeof(AssetComposite))]
    public class AssetCompositePropertyGraph<TAssetPartDesign, TAssetPart> : AssetPropertyGraph
        where TAssetPart : class, IIdentifiable
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
    {
        public AssetCompositePropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
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
}
