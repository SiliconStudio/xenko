// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering
{
    public class MeshPipelinePlugin : PipelinePlugin<MeshRenderFeature>
    {
        public const string DefaultEffectName = "XenkoForwardShadingEffect";

        private string modelEffect;
        private MeshTransparentRenderStageSelector meshRenderStageSelector;

        public ForwardLightingRenderFeature ForwardLightingRenderFeature { get; private set; }

        protected override MeshRenderFeature CreateRenderFeature(PipelinePluginContext context)
        {
            var meshRenderFeature = new MeshRenderFeature
            {
                RenderFeatures =
                {
                    new TransformRenderFeature(),
                    new SkinningRenderFeature(),
                    new MaterialRenderFeature(),
                    (ForwardLightingRenderFeature = new ForwardLightingRenderFeature()),
                },
            };

            return meshRenderFeature;
        }

        public string ModelEffect
        {
            get { return modelEffect; }
            set
            {
                modelEffect = value;
                
                // Readd the render stage selector if needed
                if (meshRenderStageSelector != null)
                {
                    RenderFeature.RenderStageSelectors.Remove(meshRenderStageSelector);
                    meshRenderStageSelector.EffectName = modelEffect ?? DefaultEffectName;
                    RenderFeature.RenderStageSelectors.Add(meshRenderStageSelector);
                }
            }
        }

        public override void Load(PipelinePluginContext context)
        {
            base.Load(context);

            // Mandatory render stages
            var mainRenderStage = context.RenderSystem.GetOrCreateRenderStage("Main", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            var transparentRenderStage = context.RenderSystem.GetOrCreateRenderStage("Transparent", "Main", new RenderOutputDescription(context.RenderContext.GraphicsDevice.Presenter.BackBuffer.ViewFormat, context.RenderContext.GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

            // Set default stage selector
            RegisterRenderStageSelector(meshRenderStageSelector = new MeshTransparentRenderStageSelector
            {
                EffectName = modelEffect ?? DefaultEffectName,
                MainRenderStage = mainRenderStage,
                TransparentRenderStage = transparentRenderStage,
            });

            // Default pipeline state
            RegisterPostProcessPipelineState((RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
            {
                if (renderNode.RenderStage == transparentRenderStage)
                {
                    pipelineState.BlendState = context.RenderContext.GraphicsDevice.BlendStates.AlphaBlend;
                }
            });
        }

        public override void Unload(PipelinePluginContext context)
        {
            meshRenderStageSelector = null;

            base.Unload(context);
        }
    }


    public class ShadowMeshPipelinePlugin : PipelinePlugin<MeshRenderFeature>
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            PipelinePluginManager.RegisterAutomaticPlugin(typeof(ShadowMeshPipelinePlugin), typeof(MeshPipelinePlugin), typeof(ShadowPipelinePlugin));
        }

        public override void Load(PipelinePluginContext context)
        {
            base.Load(context);

            var shadowMapRenderStage = context.RenderSystem.GetRenderStage("ShadowMapCaster");

            var forwardLightingRenderFeature = RenderFeature.RenderFeatures.OfType<ForwardLightingRenderFeature>().First();
            forwardLightingRenderFeature.ShadowMapRenderStage = shadowMapRenderStage;

            RegisterPostProcessPipelineState((RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
            {
                if (renderNode.RenderStage == shadowMapRenderStage)
                {
                    pipelineState.RasterizerState = new RasterizerStateDescription(CullMode.None) { DepthClipEnable = false };
                }
            });

            RegisterRenderStageSelector(new ShadowMapRenderStageSelector
            {
                EffectName = MeshPipelinePlugin.DefaultEffectName + ".ShadowMapCaster",
                ShadowMapRenderStage = shadowMapRenderStage,
            });
        }
    }
}