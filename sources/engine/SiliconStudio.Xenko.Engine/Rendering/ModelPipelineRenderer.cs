// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering
{
    public class ModelPipelineRenderer : IPipelineRenderer
    {
        public void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem)
        {
            // Mandatory render stages
            var mainRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(renderSystem, "Main", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            var transparentRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(renderSystem, "Transparent", "Main", new RenderOutputDescription(context.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            // Optional render stages
            var shadowMapRenderStage = EntityComponentRendererBase.GetRenderStage(renderSystem, "ShadowMapCaster");
            var pickingRenderStage = EntityComponentRendererBase.GetRenderStage(renderSystem, "Picking");

            var meshRenderFeature = renderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault();
            if (meshRenderFeature == null)
            {
                meshRenderFeature = new MeshRenderFeature
                {
                    RenderFeatures =
                    {
                        new TransformRenderFeature(),
                        //new SkinningRenderFeature(),
                        new MaterialRenderFeature(),
                        (renderSystem.forwardLightingRenderFeature = new ForwardLightingRenderFeature { ShadowMapRenderStage = shadowMapRenderStage }),
                        new PickingRenderFeature(),
                    },
                };

                // Set default stage selector
                meshRenderFeature.RenderStageSelectors.Add(new MeshTransparentRenderStageSelector
                {
                    EffectName = "TestEffect",
                    MainRenderStage = mainRenderStage,
                    TransparentRenderStage = transparentRenderStage,
                });

                // Register renderer
                renderSystem.RenderFeatures.Add(meshRenderFeature);
            }

            // TODO GRAPHICS REFACTOR protect against multiple executions?

            // Shadow maps (if enabled)
            if (shadowMapRenderStage != null)
            {
                meshRenderFeature.PostProcessPipelineState += (RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
                {
                    if (renderNode.RenderStage == shadowMapRenderStage)
                    {
                        pipelineState.RasterizerState = new RasterizerStateDescription(CullMode.None) { DepthClipEnable = false };
                    }
                };

                meshRenderFeature.RenderStageSelectors.Add(new ShadowMapRenderStageSelector
                {
                    EffectName = "TestEffect.ShadowMapCaster",
                    ShadowMapRenderStage = shadowMapRenderStage,
                });
            }

            // Picking (if enabled)
            if (pickingRenderStage != null)
            {
                meshRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
                {
                    EffectName = "TestEffect.Picking",
                    RenderStage = pickingRenderStage,
                });
            }

            // Register model processor
            //var sceneInstance = SceneInstance.GetCurrent(Context);
            //sceneInstance.Processors.Add(new NextGenModelProcessor());
        }
    }
}