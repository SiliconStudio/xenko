// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Processors
{
    public class ModelNodeLinkProcessor : EntityProcessor<ModelNodeLinkComponent>
    {
        public ModelNodeLinkProcessor()
            : base(typeof(TransformComponent))
        {
        }

        protected override ModelNodeLinkComponent GenerateComponentData(Entity entity, ModelNodeLinkComponent component)
        {
            return component;
        }

        protected override void OnEntityComponentRemoved(Entity entity, ModelNodeLinkComponent component, ModelNodeLinkComponent data)
        {
            // Reset TransformLink
            if (entity.Transform.TransformLink is ModelNodeTransformLink)
                entity.Transform.TransformLink = null;
        }

        public override void Draw(RenderContext context)
        {
            foreach (var item in ComponentDatas)
            {
                var entity = item.Key.Entity;
                var modelNodeLink = item.Value;
                var transformComponent = entity.Transform;
                var transformLink = transformComponent.TransformLink as ModelNodeTransformLink;

                // Try to use Target, otherwise Parent
                var modelComponent = modelNodeLink.Target;
                var modelEntity = modelComponent?.Entity ?? transformComponent.Parent?.Entity;

                // Check against Entity instead of ModelComponent to avoid having to get ModelComponent when nothing changed)
                if (transformLink == null || transformLink.NeedsRecreate(modelEntity, modelNodeLink.NodeName))
                {
                    // In case we use parent, modelComponent still needs to be resolved
                    if (modelComponent == null)
                        modelComponent = modelEntity?.Get<ModelComponent>();

                    // If model component is not parent, we want to use forceRecursive because we might want to update this link before the modelComponent.Entity is updated (depending on order of transformation update)
                    transformComponent.TransformLink = modelComponent != null ? new ModelNodeTransformLink(modelComponent, modelNodeLink.NodeName, modelEntity != transformComponent.Parent?.Entity) : null;
                }
            }
        }
    }
}