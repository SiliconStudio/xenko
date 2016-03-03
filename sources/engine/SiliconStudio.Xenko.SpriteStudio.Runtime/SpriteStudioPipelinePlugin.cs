using System.Linq;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    public class SpriteStudioPipelinePlugin : IPipelinePlugin
    {
        public void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem)
        {
            // Mandatory render stages
            var transparentRenderStage = renderSystem.GetOrCreateRenderStage("Transparent", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var spriteRenderFeature = renderSystem.RenderFeatures.OfType<SpriteStudioRenderFeature>().FirstOrDefault();
            if (spriteRenderFeature != null) return;

            spriteRenderFeature = new SpriteStudioRenderFeature();
            spriteRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            {
                EffectName = "Test",
                RenderStage = transparentRenderStage
            });

            // Register top level renderers
            // TODO GRAPHICS REFACTOR protect against multiple executions?
            renderSystem.RenderFeatures.Add(spriteRenderFeature);
        }
    }

    public class PickingSpriteStudioPipelinePlugin : IPipelinePlugin
    {
        public void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem)
        {
            var spriteRenderFeature = renderSystem.RenderFeatures.OfType<SpriteStudioRenderFeature>().First();
            var pickingRenderStage = renderSystem.GetRenderStage("Picking");

            spriteRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            {
                EffectName = "TestEffect.Picking",
                RenderStage = pickingRenderStage,
            });
        }
    }
}