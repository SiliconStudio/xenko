// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    public class SpriteStudioNodeLinkProcessor : EntityProcessor<SpriteStudioNodeLinkComponent, SpriteStudioNodeLinkComponent>
    {
        public SpriteStudioNodeLinkProcessor()
            : base(typeof(TransformComponent))
        {
            Order = 551;
        }

        protected override SpriteStudioNodeLinkComponent GenerateComponentData(Entity entity, SpriteStudioNodeLinkComponent component)
        {
            return component;
        }

        protected override void OnEntityComponentRemoved(Entity entity, SpriteStudioNodeLinkComponent component, SpriteStudioNodeLinkComponent data)
        {
            // Reset TransformLink
            if (entity.Transform.TransformLink is SpriteStudioNodeTransformLink)
                entity.Transform.TransformLink = null;
        }

        public override void Draw(RenderContext context)
        {
            foreach (var item in ComponentDatas)
            {
                var modelNodeLink = item.Value;
                var transformComponent = item.Key.Entity.Transform;
                var transformLink = transformComponent.TransformLink as SpriteStudioNodeTransformLink;

                // Try to use Target, otherwise Parent
                var modelComponent = modelNodeLink.Target;
                var modelEntity = modelComponent?.Entity ?? transformComponent.Parent?.Entity;

                // Check against Entity instead of ModelComponent to avoid having to get ModelComponent when nothing changed)
                if (transformLink == null || transformLink.NeedsRecreate(modelEntity, modelNodeLink.NodeName))
                {
                    // In case we use parent, modelComponent still needs to be resolved
                    if (modelComponent == null)
                        modelComponent = modelEntity?.Get<SpriteStudioComponent>(); // TODO: Add support for multiple components?

                    // If model component is not parent, we want to use forceRecursive because we might want to update this link before the modelComponent.Entity is updated (depending on order of transformation update)
                    transformComponent.TransformLink = modelComponent != null ? new SpriteStudioNodeTransformLink(modelComponent, modelNodeLink.NodeName) : null;
                }
            }
        }
    }
}