// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    /// <summary>
    /// Automatically registers particle rendering.
    /// </summary>
    public class ParticleEmitterPipelinePlugin : PipelinePlugin<ParticleEmitterRenderFeature>
    {
        /// <inheritdoc/>
        protected override ParticleEmitterRenderFeature CreateRenderFeature(PipelinePluginContext context)
        {
            // Mandatory render stages
            var mainRenderStage = context.RenderSystem.GetOrCreateRenderStage("Main", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            var transparentRenderStage = context.RenderSystem.GetOrCreateRenderStage("Transparent", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var particleEmitterRenderFeature = new ParticleEmitterRenderFeature();
            particleEmitterRenderFeature.RenderStageSelectors.Add(new ParticleEmitterTransparentRenderStageSelector
            {
                //EffectName = "Test",
                MainRenderStage = mainRenderStage,
                TransparentRenderStage = transparentRenderStage,
            });

            return particleEmitterRenderFeature;
        }
    }
}