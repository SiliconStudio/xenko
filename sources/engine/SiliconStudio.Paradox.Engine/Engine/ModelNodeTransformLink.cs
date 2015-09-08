using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine
{
    public class ModelNodeTransformLink : TransformLink
    {
        private readonly ModelComponent parentModelComponent;
        private ModelViewHierarchyUpdater modelViewHierarchy;
        private string nodeName;
        private int nodeIndex = int.MaxValue;

        public ModelNodeTransformLink(ModelComponent parentModelComponent, string nodeName)
        {
            this.parentModelComponent = parentModelComponent;
            this.nodeName = nodeName;
        }

        public TransformTRS Transform;

        /// <inheritdoc/>
        public override void ComputeMatrix(out Matrix matrix)
        {
            // Updated? (rare slow path)
            if (parentModelComponent.ModelViewHierarchy != modelViewHierarchy)
            {
                modelViewHierarchy = parentModelComponent.ModelViewHierarchy;
                if (modelViewHierarchy == null)
                {
                    goto failed;
                }

                // Find our node index
                nodeIndex = int.MaxValue;
                for (int index = 0; index < modelViewHierarchy.Nodes.Length; index++)
                {
                    var node = modelViewHierarchy.Nodes[index];
                    if (node.Name == nodeName)
                    {
                        nodeIndex = index;
                    }
                }
            }

            var nodes = modelViewHierarchy.Nodes;
            var nodeTransformations = modelViewHierarchy.NodeTransformations;
            if (nodeIndex >= nodes.Length)
            {
                goto failed;
            }

            // Hopefully, if ref locals gets merged in roslyn, this code can be refactored
            // Compute
            matrix = nodeTransformations[nodeIndex].WorldMatrix;
            return;

        failed:
            // Fallback to TransformComponent
            matrix = parentModelComponent.Entity.Transform.WorldMatrix;
            return;
        }

        public bool NeedsRecreate(Entity parentEntity, string targetNodeName)
        {
            return parentModelComponent.Entity != parentEntity
                || !object.ReferenceEquals(nodeName, targetNodeName); // note: supposed to use same string instance so no need to compare content
        }
    }
}