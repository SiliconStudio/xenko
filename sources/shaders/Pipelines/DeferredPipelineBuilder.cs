using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Renderers;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public class DeferredPipelineBuilder : MainPipelineBuilder
    {
        public string PrepassEffectName { get; set; }

        public override void Load()
        {
            var graphicsService = ServiceRegistry.GetSafeServiceAs<IGraphicsDeviceService>();

            // Create G-buffer pass
            // Renders the G-buffer for opaque geometry.
            var gbufferPipeline = new RenderPipeline("GBuffer");
            AddRenderer(gbufferPipeline, new ModelRenderer(ServiceRegistry, EffectName + ".ParadoxGBufferShaderPass").AddOpaqueFilter());

            // Add the G-buffer pass to the main pipeline.
            var gbufferProcessor = new GBufferRenderProcessor(ServiceRegistry, gbufferPipeline, graphicsService.GraphicsDevice.DepthStencilBuffer, false);
            AddRenderer(gbufferProcessor);

            // Performs the light prepass on opaque geometry.
            // Adds this pass to the pipeline.
            AddRenderer(new LightingPrepassRenderer(ServiceRegistry, PrepassEffectName, graphicsService.GraphicsDevice.DepthStencilBuffer, gbufferProcessor.GBufferTexture));

            // Sets the render targets and clear them. Also sets the viewport.
            AddRenderer(new RenderTargetSetter(ServiceRegistry)
            {
                ClearColor = ClearColor,
                EnableClearDepth = false,
                RenderTarget = graphicsService.GraphicsDevice.BackBuffer,
                DepthStencil = graphicsService.GraphicsDevice.DepthStencilBuffer,
                Viewport = new Viewport(0, 0, graphicsService.GraphicsDevice.BackBuffer.Width, graphicsService.GraphicsDevice.BackBuffer.Height)
            });

            if (BeforeMainRender != null)
                Build(BeforeMainRender);

            // Prevents depth write since depth was already computed in G-buffer pas.
            AddRenderer(new RenderStateSetter(ServiceRegistry) { DepthStencilState = graphicsService.GraphicsDevice.DepthStencilStates.DepthRead });
            AddRenderer(new ModelRenderer(ServiceRegistry, EffectName).AddOpaqueFilter());
            AddRenderer(new RenderTargetSetter(ServiceRegistry)
            {
                EnableClearDepth = false,
                EnableClearTarget = false,
                RenderTarget = graphicsService.GraphicsDevice.BackBuffer,
                DepthStencil = graphicsService.GraphicsDevice.DepthStencilBuffer,
                Viewport = new Viewport(0, 0, graphicsService.GraphicsDevice.BackBuffer.Width, graphicsService.GraphicsDevice.BackBuffer.Height)
            });

            // Renders transparent geometry. Depth stencil state is determined by the object to draw.
            //mainPipeline.Renderers.Add(new RenderStateSetter(serviceRegistry) { DepthStencilState = graphicsService.GraphicsDevice.DepthStencilStates.DepthRead });
            AddRenderer(new ModelRenderer(ServiceRegistry, EffectName).AddTransparentFilter());

            // Try to keep a consistent state with forward rendering, by reenabling depth buffer write
            AddRenderer(new RenderStateSetter(ServiceRegistry) { DepthStencilState = graphicsService.GraphicsDevice.DepthStencilStates.Default });
        }
    }
}