using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetGraphNodeChangeListener : GraphNodeChangeListener
    {
        public AssetGraphNodeChangeListener(IGraphNode rootNode, [NotNull] AssetPropertyGraph propertyGraph)
            : base(rootNode, member => !propertyGraph.IsObjectReference(member, Index.Empty, member.Retrieve()), (node, index) => !propertyGraph.IsObjectReference(node, index, node.Retrieve(index)))
        {
        }
    }
}
