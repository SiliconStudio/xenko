// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Updates <see cref="Engine.ModelComponent.Skeleton"/>.
    /// </summary>
    public class ModelViewHierarchyTransformOperation : TransformOperation
    {
        public readonly ModelComponent ModelComponent;

        public ModelViewHierarchyTransformOperation(ModelComponent modelComponent)
        {
            ModelComponent = modelComponent;
        }

        /// <inheritdoc/>
        public override void Process(TransformComponent transformComponent)
        {
            ModelComponent.Update(transformComponent);
        }
    }
}
