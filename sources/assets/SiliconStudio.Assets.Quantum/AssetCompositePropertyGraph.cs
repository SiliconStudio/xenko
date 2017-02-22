using System;
using SiliconStudio.Assets.Quantum.Visitors;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyGraph(typeof(AssetComposite))]
    public class AssetCompositePropertyGraph : AssetPropertyGraph
    {
        public AssetCompositePropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger) : base(container, assetItem, logger)
        {
        }

        [Obsolete("This method will be removed soon")]
        public virtual bool IsReferencedPart(IMemberNode member, IGraphNode targetNode)
        {
            return false;
        }

        public override GraphVisitorBase CreateReconcilierVisitor()
        {
            return new AssetGraphVisitorBase(this);
        }

        protected sealed override IBaseToDerivedRegistry CreateBaseToDerivedRegistry()
        {
            return new AssetCompositeBaseToDerivedRegistry(this);
        }
    }
}
