using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyGraph(typeof(AssetComposite))]
    public class AssetCompositePropertyGraph : AssetPropertyGraph
    {
        public AssetCompositePropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger) : base(container, assetItem, logger)
        {
        }

        public override bool ShouldListenToTargetNode(MemberContent member, IGraphNode targetNode)
        {
            // Make sure it's actually a target (not a member) node.
            return !IsReferencedPart(member, targetNode) && base.ShouldListenToTargetNode(member, targetNode);
        }

        public virtual bool IsReferencedPart(MemberContent member, IGraphNode targetNode)
        {
            return false;
        }

        protected override GraphVisitorBase CreateReconcilierVisitor()
        {
            return new AssetCompositePartVisitor(this);
        }

        protected override bool ShouldReconcileItem(MemberContent member, IGraphNode targetNode, object localValue, object baseValue, bool isReference)
        {
            // Always reconcile referenced parts
            if (isReference && IsReferencedPart(member, targetNode))
            {
                return true;
            }
            return base.ShouldReconcileItem(member, targetNode, localValue, baseValue, isReference);
        }
    }
}
