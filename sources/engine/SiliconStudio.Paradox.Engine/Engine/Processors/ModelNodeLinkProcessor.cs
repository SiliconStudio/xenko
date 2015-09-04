// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Processors
{
    public class ModelNodeLinkProcessor : EntityProcessor<ModelNodeLinkComponent>
    {
        internal ModelProcessor meshProcessor;

        public ModelNodeLinkProcessor()
            : base(new PropertyKey[] { TransformComponent.Key, ModelNodeLinkComponent.Key })
        {
        }

        protected override ModelNodeLinkComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(ModelNodeLinkComponent.Key);
        }

        public override void Draw(RenderContext context)
        {
            foreach (var item in enabledEntities)
            {
                var modelNodeLink = item.Value;
                var transformComponent = item.Key.Transform;
                var transformLink = transformComponent.ParentLink as ModelNodeTransformLink;

                var parentEntity = transformComponent.Parent.Entity;
                if (transformLink == null || transformLink.NeedsRecreate(parentEntity, modelNodeLink.NodeName))
                {
                    var modelComponent = parentEntity.Get(ModelComponent.Key);
                    transformComponent.ParentLink = modelComponent != null ? new ModelNodeTransformLink(modelComponent, modelNodeLink.NodeName) : null;
                }
            }
        }
    }
}