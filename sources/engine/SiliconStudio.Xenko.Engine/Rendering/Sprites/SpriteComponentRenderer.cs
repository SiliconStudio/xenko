// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    public class SpriteComponentRenderer : EntityComponentRendererBase
    {
        public override void SetupPipeline(NextGenRenderSystem renderSystem)
        {
            // Mandatory render stages
            var mainRenderStage = GetOrCreateRenderStage(renderSystem, "Main", "Main", new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            var transparentRenderStage = GetOrCreateRenderStage(renderSystem, "Transparent", "Main", new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

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

            // Attach processor
            var sceneInstance = SceneInstance.GetCurrent(Context);
            sceneInstance.Processors.Add(new SpriteRenderProcessor());
        }
    }
}