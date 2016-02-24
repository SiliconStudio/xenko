using System.Linq;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    public class ParticleEmitterPipelinePlugin : IPipelinePlugin
    {
        public void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem)
        {
            // Mandatory render stages
            var mainRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(renderSystem, "Main", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            var transparentRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(renderSystem, "Transparent", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var particleEmitterRenderFeature = renderSystem.RenderFeatures.OfType<ParticleEmitterRenderFeature>().FirstOrDefault();
            if (particleEmitterRenderFeature == null)
            {
                particleEmitterRenderFeature = new ParticleEmitterRenderFeature();
                particleEmitterRenderFeature.RenderStageSelectors.Add(new ParticleEmitterTransparentRenderStageSelector
                {
                    //EffectName = "Test",
                    MainRenderStage = mainRenderStage,
                    TransparentRenderStage = transparentRenderStage,
                });

                // Register top level renderers
                // TODO GRAPHICS REFACTOR protect against multiple executions?
                renderSystem.RenderFeatures.Add(particleEmitterRenderFeature);
            }
        }
    }
}