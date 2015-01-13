using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Effects.Shadows;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public class LightingPipelineBuilder : PipelineBuilder
    {
        public string EffectName { get; set; }

        public bool UseShadows { get; set; }

        public override void Load()
        {
            var graphicsService = ServiceRegistry.GetSafeServiceAs<IGraphicsDeviceService>();
            var entitySystem = ServiceRegistry.GetServiceAs<EntitySystem>();

            if (entitySystem != null)
            {
                var lightProcessor = entitySystem.GetProcessor<LightShadowProcessor>();
                if (lightProcessor == null)
                    entitySystem.Processors.Add(new DynamicLightShadowProcessor(graphicsService.GraphicsDevice, UseShadows));
            }

            if (UseShadows)
            {
                var shadowMapPipeline = new RenderPipeline("ShadowMap");
                AddRenderer(shadowMapPipeline, new ModelRenderer(ServiceRegistry, EffectName + ".ShadowMapCaster").AddContextActiveLayerFilter().AddShadowCasterFilter());

                var shadowMapRenderer = new ShadowMapRenderer(ServiceRegistry, shadowMapPipeline);
                AddRenderer(shadowMapRenderer);
            }
        }
    }
}