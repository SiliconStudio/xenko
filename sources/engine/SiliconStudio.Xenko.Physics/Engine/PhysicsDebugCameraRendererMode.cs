// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Physics.Engine
{
    public class PhysicsDebugPipelinePlugin : IPipelinePlugin
    {
        public void Load(PipelinePluginContext context)
        {
        }

        public void Unload(PipelinePluginContext context)
        {
        }
    }

    public class MeshPhysicsDebugPipelinePlugin : PipelinePlugin<MeshRenderFeature>
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            PipelinePluginManager.RegisterAutomaticPlugin(typeof(MeshPhysicsDebugPipelinePlugin), typeof(MeshPipelinePlugin), typeof(PhysicsDebugPipelinePlugin));
        }

        public override void Load(PipelinePluginContext context)
        {
            base.Load(context);

            var physicsDebugShapeRenderStage = context.RenderSystem.GetRenderStage("PhysicsDebugShape");

            RegisterPostProcessPipelineState((RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
            {
                if (renderNode.RenderStage == physicsDebugShapeRenderStage)
                {
                    pipelineState.RasterizerState = RasterizerStates.Wireframe;
                }
            });

            RegisterRenderStageSelector(new SimpleGroupToRenderStageSelector
            {
                EffectName = MeshPipelinePlugin.DefaultEffectName,
                RenderStage = physicsDebugShapeRenderStage,
            });
        }
    }

    [DataContract("PhysicsDebugCameraRendererMode")]
    [NonInstantiable]
    public class PhysicsDebugCameraRendererMode : CameraRenderModeBase
    {
        [DataMemberIgnore]
        public RenderStage PhysicsDebugShapeRenderStage { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            if (PhysicsDebugShapeRenderStage == null)
                PhysicsDebugShapeRenderStage = RenderSystem.GetOrCreateRenderStage("PhysicsDebugShape", "PhysicsDebugShape", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));

            if (PhysicsDebugShapeRenderStage != null)
            {
                MainRenderView.RenderStages.Add(PhysicsDebugShapeRenderStage);
            }

            RenderSystem.PipelinePlugins.InstantiatePlugin<PhysicsDebugPipelinePlugin>();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var renderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
            context.CommandList.Clear(renderFrame.DepthStencil, DepthStencilClearOptions.DepthBuffer);
            RenderSystem.Draw(context, MainRenderView, PhysicsDebugShapeRenderStage);
        }
    }
}