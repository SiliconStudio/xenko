using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    public class SpriteStudioPipelinePlugin : PipelinePlugin<SpriteStudioRenderFeature>
    {
        protected override SpriteStudioRenderFeature CreateRenderFeature(PipelinePluginContext context)
        {
            // Mandatory render stages
            var transparentRenderStage = context.RenderSystem.GetOrCreateRenderStage("Transparent", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            var spriteRenderFeature = new SpriteStudioRenderFeature();
            spriteRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            {
                EffectName = "Test",
                RenderStage = transparentRenderStage
            });

            return spriteRenderFeature;
        }
    }

    public class PickingSpriteStudioPipelinePlugin : PipelinePlugin<SpriteStudioRenderFeature>
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            PipelinePluginManager.RegisterAutomaticPlugin(typeof(PickingSpriteStudioPipelinePlugin), typeof(SpriteStudioPipelinePlugin), typeof(PickingPipelinePlugin));
        }

        public override void Load(PipelinePluginContext context)
        {
            base.Load(context);

            var pickingRenderStage = context.RenderSystem.GetRenderStage("Picking");

            RegisterRenderStageSelector(new SimpleGroupToRenderStageSelector
            {
                EffectName = "TestEffect.Picking",
                RenderStage = pickingRenderStage,
            });
        }
    }
}