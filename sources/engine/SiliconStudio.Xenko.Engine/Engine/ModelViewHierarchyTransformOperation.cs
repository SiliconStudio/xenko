// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
            modelComponent.Update(transformComponent);
        }
    }
}
