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
            var baseValue = baseNode?.Retrieve();
            if (baseValue == null)
                return;

            if (!propertyGraph.IsObjectReference(baseNode, Index.Empty, baseValue))
            {
                if (baseValue is IIdentifiable)
                {
                    baseToDerived[baseNode] = derivedNode;
                    var baseMemberNode = baseNode as IAssetMemberNode;
                    if (baseMemberNode?.Target != null)
                    {
                        baseToDerived[baseMemberNode.Target] = ((IAssetMemberNode)derivedNode).Target;
                    }
                }
            }
            var objectNode = derivedNode as IObjectNode;
            if (objectNode?.ItemReferences != null)
            {
                foreach (var reference in objectNode.ItemReferences)
                {
                    var target = propertyGraph.baseLinker.FindTargetReference(derivedNode, baseNode, reference);
                    if (target == null)
                        continue;

                    baseValue = target.TargetNode?.Retrieve();
                    if (!propertyGraph.IsObjectReference(baseNode, target.Index, baseValue))
                    {
                        if (baseValue is IIdentifiable)
                        {
                            baseToDerived[(IAssetNode)target.TargetNode] = (IAssetNode)objectNode.IndexedTarget(reference.Index);
                        }
                    }
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
