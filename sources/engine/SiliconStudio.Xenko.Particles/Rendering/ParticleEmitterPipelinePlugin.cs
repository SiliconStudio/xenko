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

            particleEmitterRenderFeature.PostProcessPipelineState += (RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
            {
                var renderParticleEmitter = (RenderParticleEmitter)renderObject;
                //renderParticleEmitter.ParticleEmitter.Material;
                    
                pipelineState.BlendState = context.RenderContext.GraphicsDevice.BlendStates.AlphaBlend;
                pipelineState.DepthStencilState = context.RenderContext.GraphicsDevice.DepthStencilStates.DepthRead;
                pipelineState.RasterizerState = new RasterizerStateDescription(CullMode.Back);
            };

            return particleEmitterRenderFeature;
        }
    }
}