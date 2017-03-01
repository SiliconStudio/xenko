using System;
using SiliconStudio.Assets.Quantum.Visitors;
using SiliconStudio.Core.Annotations;
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

        protected void LinkToOwnerPart([NotNull] IGraphNode node, object part)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            var visitor = new NodesToOwnerPartVisitor(this, Container.NodeContainer, part);
            visitor.Visit(node);
        }

        protected sealed override IBaseToDerivedRegistry CreateBaseToDerivedRegistry()
        {
            return new AssetCompositeBaseToDerivedRegistry(this);
        }
    }
}
