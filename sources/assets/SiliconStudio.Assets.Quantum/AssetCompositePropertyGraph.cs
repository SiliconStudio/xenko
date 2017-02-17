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

        public virtual bool IsReferencedPart(IMemberNode member, IGraphNode targetNode)
        {
            return false;
        }

        public override GraphVisitorBase CreateReconcilierVisitor()
        {
            return new AssetGraphVisitorBase(this);
        }

        protected override bool ShouldReconcileItem(IMemberNode member, IGraphNode targetNode, object localValue, object baseValue, bool isReference)
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
