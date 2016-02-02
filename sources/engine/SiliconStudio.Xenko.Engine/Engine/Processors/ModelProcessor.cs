// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// The processor for <see cref="ModelComponent"/>.
    /// </summary>
    public class ModelProcessor : EntityProcessor<ModelComponent, ModelProcessor.ModelTransformationInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelProcessor"/> class.
        /// </summary>
        public ModelProcessor()
            : base(typeof(TransformComponent))
        {
        }

        protected override ModelTransformationInfo GenerateComponentData(Entity entity, ModelComponent component)
        {
            return new ModelTransformationInfo();
        }

        protected override bool IsAssociatedDataValid(Entity entity, ModelComponent component, ModelTransformationInfo associatedData)
        {
            return entity.Get<ModelComponent>() == component;
        }

        protected override void OnEntityComponentAdding(Entity entity, ModelComponent component, ModelTransformationInfo data)
        {
            // Register model view hierarchy update
            entity.Transform.PostOperations.Add(data.TransformOperation = new ModelViewHierarchyTransformOperation(component));
        }

        protected override void OnEntityComponentRemoved(Entity entity, ModelComponent component, ModelTransformationInfo data)
        {
            // Unregister model view hierarchy update
            entity.Transform.PostOperations.Remove(data.TransformOperation);
        }
        
        public class ModelTransformationInfo
        {
            public ModelViewHierarchyTransformOperation TransformOperation;
        }
    }
}