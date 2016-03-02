using System.Linq;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.UI
{
    public class UIPipelinePlugin : IPipelinePlugin
    {
        public void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem)
        {
            // Mandatory render stages
            var transparentRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(renderSystem, "Transparent", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var uiRenderFeature = renderSystem.RenderFeatures.OfType<UIRenderFeature>().FirstOrDefault();
            if (uiRenderFeature == null)
            {
                uiRenderFeature = new UIRenderFeature();
                uiRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
                {
                    EffectName = "Test",
                    RenderStage = transparentRenderStage,
                });

                // Register top level renderers
                // TODO GRAPHICS REFACTOR protect against multiple executions?
                renderSystem.RenderFeatures.Add(uiRenderFeature);
            }
        }
    }
}