// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    public class SpritePipelineRenderer : IPipelineRenderer
    {
        public void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem)
        {
            // Mandatory render stages
            var mainRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(renderSystem, "Main", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            var transparentRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(renderSystem, "Transparent", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var spriteRenderFeature = renderSystem.RenderFeatures.OfType<SpriteRenderFeature>().FirstOrDefault();
            if (spriteRenderFeature == null)
            {
                spriteRenderFeature = new SpriteRenderFeature();
                spriteRenderFeature.RenderStageSelectors.Add(new SpriteTransparentRenderStageSelector
                {
                    EffectName = "Test",
                    MainRenderStage = mainRenderStage,
                    TransparentRenderStage = transparentRenderStage,
                });

                // Register top level renderers
                // TODO GRAPHICS REFACTOR protect against multiple executions?
                renderSystem.RenderFeatures.Add(spriteRenderFeature);
            }
        }
    }
}