using System.Linq;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    public class ParticleEmitterPipelinePlugin : PipelinePlugin<ParticleEmitterRenderFeature>
    {
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