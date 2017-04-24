// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    public class SpriteStudioRendererProcessor : EntityProcessor<SpriteStudioComponent, RenderSpriteStudio>, IEntityComponentRenderProcessor
    {
        public SpriteStudioRendererProcessor()
            : base(typeof(TransformComponent))
        {
            Order = 550;
        }

        protected override RenderSpriteStudio GenerateComponentData(Entity entity, SpriteStudioComponent component)
        {
            return new RenderSpriteStudio
            {
                SpriteStudioComponent = component,
                TransformComponent = entity.Transform
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, SpriteStudioComponent component, RenderSpriteStudio associatedData)
        {
            return
                component == associatedData.SpriteStudioComponent &&
                entity.Transform == associatedData.TransformComponent;
        }

        protected override void OnEntityComponentAdding(Entity entity, SpriteStudioComponent component, RenderSpriteStudio data)
        {
            VisibilityGroup.RenderObjects.Add(data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, SpriteStudioComponent component, RenderSpriteStudio data)
        {
            VisibilityGroup.RenderObjects.Remove(data);
        }

        public override void Draw(RenderContext context)
        {
            foreach (var spriteStateKeyPair in ComponentDatas)
            {
                var renderSpriteStudio = spriteStateKeyPair.Value;
                renderSpriteStudio.Enabled = renderSpriteStudio.SpriteStudioComponent.Enabled;

                if(!renderSpriteStudio.Enabled || !renderSpriteStudio.SpriteStudioComponent.ValidState) continue;

                renderSpriteStudio.BoundingBox = new BoundingBoxExt { Center = renderSpriteStudio.TransformComponent.WorldMatrix.TranslationVector };
                renderSpriteStudio.RenderGroup = renderSpriteStudio.SpriteStudioComponent.RenderGroup;               
            }
        }

        public VisibilityGroup VisibilityGroup { get; set; }
    }
}
