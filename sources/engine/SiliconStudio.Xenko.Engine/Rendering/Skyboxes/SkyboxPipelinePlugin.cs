// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    /// <summary>
    /// Automatically registers skybox rendering.
    /// </summary>
    public class SkyboxPipelinePlugin : PipelinePlugin<SkyboxRenderFeature>
    {
        protected override SkyboxRenderFeature CreateRenderFeature(PipelinePluginContext context)
        {
            // Mandatory render stages
            var mainRenderStage = context.RenderSystem.GetOrCreateRenderStage("Main", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var skyboxRenderFeature = new SkyboxRenderFeature();
            skyboxRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            {
                RenderStage = mainRenderStage,
                EffectName = "SkyboxEffect",
            });

            return skyboxRenderFeature;
        }
    }
}