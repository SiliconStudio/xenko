// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.UI
{
    /// <summary>
    /// Automatically registers UI rendering.
    /// </summary>
    public class UIPipelinePlugin : PipelinePlugin<UIRenderFeature>
    {
        protected override UIRenderFeature CreateRenderFeature(PipelinePluginContext context)
        {
            // Mandatory render stages
            var transparentRenderStage = context.RenderSystem.GetOrCreateRenderStage("Transparent", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var uiRenderFeature = new UIRenderFeature();
            uiRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            {
                EffectName = "Test",
                RenderStage = transparentRenderStage,
            });

            return uiRenderFeature;
        }
    }
}
