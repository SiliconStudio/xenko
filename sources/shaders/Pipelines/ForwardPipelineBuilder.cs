using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Renderers;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public class ForwardPipelineBuilder : MainPipelineBuilder
    {
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
            AddRenderer(new ModelRenderer(ServiceRegistry, EffectName));
        }
    }
}