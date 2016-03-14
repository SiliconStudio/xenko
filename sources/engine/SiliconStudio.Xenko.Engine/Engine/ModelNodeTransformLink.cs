using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    public class ModelNodeTransformLink : TransformLink
    {
        private readonly ModelComponent parentModelComponent;
        private SkeletonUpdater skeleton;
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

            if (parentModelComponent.Skeleton != skeleton)
            {
                skeleton = parentModelComponent.Skeleton;
                if (skeleton != null)
                {
                    // Find our node index
                    nodeIndex = int.MaxValue;
                    for (int index = 0; index < skeleton.Nodes.Length; index++)
                    {
                        var node = skeleton.Nodes[index];
                        if (node.Name == nodeName)
                        {
                            nodeIndex = index;
                        }
                    }
                }
            }

            // Updated? (rare slow path)
            if (skeleton != null)
            {
                var nodes = skeleton.Nodes;
                var nodeTransformations = skeleton.NodeTransformations;
                if (nodeIndex < nodes.Length)
                {
                    // Hopefully, if ref locals gets merged in roslyn, this code can be refactored
                    // Compute
                    matrix = nodeTransformations[nodeIndex].WorldMatrix;
                    return;
                }
            }

            // Fallback to TransformComponent
            matrix = parentModelComponent.Entity.Transform.WorldMatrix;
        }

        public bool NeedsRecreate(Entity parentEntity, string targetNodeName)
        {
            return parentModelComponent.Entity != parentEntity
                || !object.ReferenceEquals(nodeName, targetNodeName); // note: supposed to use same string instance so no need to compare content
        }
    }
}