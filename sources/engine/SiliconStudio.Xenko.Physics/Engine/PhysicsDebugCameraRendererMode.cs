using System.Linq;
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

            var wireFrameRenderStage = context.RenderSystem.GetRenderStage("Wireframe");

            RegisterPostProcessPipelineState((RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
            {
                if (renderNode.RenderStage == wireFrameRenderStage)
                {
                    pipelineState.RasterizerState = context.RenderContext.GraphicsDevice.RasterizerStates.Wireframe;
                }
            });

            RegisterRenderStageSelector(new SimpleGroupToRenderStageSelector
            {
                EffectName = MeshPipelinePlugin.DefaultEffectName,
                RenderStage = wireFrameRenderStage,
            });
        }
    }

    [DataContract("PhysicsDebugCameraRendererMode")]
    [NonInstantiable]
    public class PhysicsDebugCameraRendererMode : CameraRenderModeBase
    {
        [DataMemberIgnore]
        public RenderStage WireframeRenderStage { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            if (WireframeRenderStage == null)
                WireframeRenderStage = RenderSystem.GetOrCreateRenderStage("Wireframe", "Wireframe", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));

            if (WireframeRenderStage != null)
            {
                MainRenderView.RenderStages.Add(WireframeRenderStage);
            }

            RenderSystem.PipelinePlugins.InstantiatePlugin<PhysicsDebugPipelinePlugin>();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var renderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
            context.CommandList.Clear(renderFrame.DepthStencil, DepthStencilClearOptions.DepthBuffer);
            RenderSystem.Draw(context, MainRenderView, WireframeRenderStage);
        }
    }
}