using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine
{
    public class ModelNodeTransformLink : TransformLink
    {
        private readonly ModelComponent parentModelComponent;
        private ModelViewHierarchyUpdater modelViewHierarchy;
        private readonly bool forceRecursive;
        private string nodeName;
        private int nodeIndex = int.MaxValue;

        public ModelNodeTransformLink(ModelComponent parentModelComponent, string nodeName, bool forceRecursive)
        {
            this.parentModelComponent = parentModelComponent;
            this.nodeName = nodeName;
            this.forceRecursive = forceRecursive;
        }

        public TransformTRS Transform;

        /// <inheritdoc/>
        public override void ComputeMatrix(bool recursive, out Matrix matrix)
        {
            // If model is not in the parent, we might want to force recursive update (since parentModelComponent might not be updated yet)
            if (forceRecursive || recursive)
            {
                parentModelComponent.Entity.Transform.UpdateWorldMatrix();
            }

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