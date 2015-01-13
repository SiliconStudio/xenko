using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public static class PipelineBuilders
    {
        public static CompositePipelineBuilder CreateDefault(IServiceRegistry serviceRegistry, string effectName, bool deferred, string prepassEffectName, Color clearColor, bool useShadows, bool ui, string backgroundName)
        {
            var result = new CompositePipelineBuilder();
            result.ServiceRegistry = serviceRegistry;

            // Setup camera
            result.Add(new CameraSetter(serviceRegistry));

            // Setup lighting (gather light from entities, and render shadow maps)
            result.Add(new LightingPipelineBuilder { EffectName = effectName });

            // Setup forward or deferred rendering
            MainPipelineBuilder mainPipeline;
            if (deferred)
                mainPipeline = new DeferredPipelineBuilder { PrepassEffectName = prepassEffectName };
            else
                mainPipeline = new ForwardPipelineBuilder();

            mainPipeline.EffectName = effectName;
            mainPipeline.ClearColor = clearColor;

            // Draws a background from a texture before main rendering
            if (backgroundName != null)
                mainPipeline.BeforeMainRender = new BackgroundRenderer(serviceRegistry, backgroundName);

            result.Add(mainPipeline);

            // Setup UI rendering
            if (ui)
                result.Add(new UIRenderer(serviceRegistry));

            return result;
        }
    }
}