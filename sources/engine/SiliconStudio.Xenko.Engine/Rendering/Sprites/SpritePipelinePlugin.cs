// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    /// <summary>
    /// Automatically registers sprite rendering.
    /// </summary>
    public class SpritePipelinePlugin : PipelinePlugin<SpriteRenderFeature>
    {
        protected override SpriteRenderFeature CreateRenderFeature(PipelinePluginContext context)
        {
            // Mandatory render stages
            var mainRenderStage = context.RenderSystem.GetOrCreateRenderStage("Main", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            var transparentRenderStage = context.RenderSystem.GetOrCreateRenderStage("Transparent", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var spriteRenderFeature = new SpriteRenderFeature();
            spriteRenderFeature.RenderStageSelectors.Add(new SpriteTransparentRenderStageSelector
            {
                EffectName = "Test",
                MainRenderStage = mainRenderStage,
                TransparentRenderStage = transparentRenderStage,
            });

            return spriteRenderFeature;
        }
    }
}