using System.Linq;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.UI
{
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