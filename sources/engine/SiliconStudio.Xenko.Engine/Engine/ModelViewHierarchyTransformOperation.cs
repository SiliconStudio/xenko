using System;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Updates <see cref="ModelComponent.Skeleton"/>.
    /// </summary>
    public class ModelViewHierarchyTransformOperation : TransformOperation
    {
        private readonly ModelComponent modelComponent;
        public ModelViewHierarchyTransformOperation(ModelComponent modelComponent)
        {
            this.modelComponent = modelComponent;
        }

        /// <inheritdoc/>
        public override void Process(TransformComponent transformComponent)
        {
            // Waiting for roslyn ref locals to avoid having to pass world matrix
            modelComponent.Update(transformComponent, ref transformComponent.WorldMatrix);
        }
    }
}