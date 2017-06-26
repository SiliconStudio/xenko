// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    public class SpriteTransparentRenderStageSelector : TransparentRenderStageSelector
    {
        public override void Process(RenderObject renderObject)
        {
            if (((RenderGroupMask)(1U << (int)renderObject.RenderGroup) & RenderGroup) != 0)
            {
                var renderSprite = (RenderSprite)renderObject;

                var renderStage = renderSprite.SpriteComponent.CurrentSprite.IsTransparent ? TransparentRenderStage : OpaqueRenderStage;
                if (renderStage != null)
                    renderObject.ActiveRenderStages[renderStage.Index] = new ActiveRenderStage(EffectName);
            }
        }
    }
}
