// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Background
{
    /// <summary>
    /// Automatically registers background rendering.
    /// </summary>
    public class BackgroundPipelinePlugin : PipelinePlugin<BackgroundRenderFeature>
    {
        /// <inheritdoc/>
        protected override BackgroundRenderFeature CreateRenderFeature(PipelinePluginContext context)
        {
            // Mandatory render stages
            var mainRenderStage = context.RenderSystem.GetOrCreateRenderStage("Main", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var backgroundFeature = new BackgroundRenderFeature();
            backgroundFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            {
                RenderStage = mainRenderStage,
                EffectName = "Test",
            });

            return backgroundFeature;
        }
    }
}
