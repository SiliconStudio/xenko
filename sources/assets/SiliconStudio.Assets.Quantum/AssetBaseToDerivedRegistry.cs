using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    internal class AssetBaseToDerivedRegistry : IBaseToDerivedRegistry
    {
        private readonly AssetPropertyGraph propertyGraph;
        private readonly Dictionary<IAssetNode, IAssetNode> baseToDerived = new Dictionary<IAssetNode, IAssetNode>();

        public AssetBaseToDerivedRegistry(AssetPropertyGraph propertyGraph)
        {
            this.propertyGraph = propertyGraph;
        }

        public void RegisterBaseToDerived(IAssetNode baseNode, IAssetNode derivedNode)
        {
            if (baseNode != null)
            {
                if (!propertyGraph.IsObjectReference(baseNode, Index.Empty, baseNode.Retrieve()))
                {
                    baseToDerived[baseNode] = derivedNode;
                }
            }
        }

        public IIdentifiable ResolveFromBase(object baseObjectReference, IAssetNode derivedReferencerNode)
        {
            if (derivedReferencerNode == null) throw new ArgumentNullException(nameof(derivedReferencerNode));
            if (baseObjectReference == null)
                return null;

            var baseNode = (IAssetNode)propertyGraph.Container.NodeContainer.GetNode(baseObjectReference);
            IAssetNode derivedNode;
            baseToDerived.TryGetValue(baseNode, out derivedNode);
            return derivedNode?.Retrieve() as IIdentifiable;
        }
    }
}