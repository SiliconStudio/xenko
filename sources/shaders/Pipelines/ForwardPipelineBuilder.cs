using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Renderers;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public class ForwardPipelineBuilder : MainPipelineBuilder
    {
        private bool isSupportingLight;
        public ForwardPipelineBuilder(bool isSupportingLight)
        {
            this.isSupportingLight = isSupportingLight;
        }

        public override void Load()
        {
            var graphicsService = ServiceRegistry.GetSafeServiceAs<IGraphicsDeviceService>();

            // Sets the render targets and clear them.
            AddRenderer(new RenderTargetSetter(ServiceRegistry)
            {
                ClearColor = ClearColor,
                RenderTarget = graphicsService.GraphicsDevice.BackBuffer,
                DepthStencil = graphicsService.GraphicsDevice.DepthStencilBuffer
            });

            if (BeforeMainRender != null)
                Build(BeforeMainRender);

            // Renders all the meshes with the correct lighting.
            var renderer = new ModelRenderer(ServiceRegistry, EffectName);
            if (isSupportingLight)
            {
                renderer.AddLightForwardSupport();
            }
            AddRenderer(renderer);
        }
    }
}