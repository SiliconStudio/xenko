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
            var mainRenderStage = renderSystem.GetOrCreateRenderStage("Main", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            var transparentRenderStage = renderSystem.GetOrCreateRenderStage("Transparent", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

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

                particleEmitterRenderFeature.PostProcessPipelineState += (RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
                {
                    var renderParticleEmitter = (RenderParticleEmitter)renderObject;
                    //renderParticleEmitter.ParticleEmitter.Material;
                    
                    pipelineState.BlendState = context.GraphicsDevice.BlendStates.AlphaBlend;
                    pipelineState.DepthStencilState = context.GraphicsDevice.DepthStencilStates.DepthRead;
                    pipelineState.RasterizerState = new RasterizerStateDescription(CullMode.Back);
                };

                // Register top level renderers
                // TODO GRAPHICS REFACTOR protect against multiple executions?
                renderSystem.RenderFeatures.Add(particleEmitterRenderFeature);
            }
        }
    }
}