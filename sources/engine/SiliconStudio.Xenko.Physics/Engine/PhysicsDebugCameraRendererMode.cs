using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Physics.Engine
{
    public class WireFramePhysicsDebugPipelinePlugin : IPipelinePlugin
    {
        public void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem)
        {
            var meshRenderFeature = renderSystem.RenderFeatures.OfType<MeshRenderFeature>().First();
            var wireFrameRenderStage = EntityComponentRendererBase.GetRenderStage(renderSystem, "WireFrame");

            meshRenderFeature.PostProcessPipelineState += (RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
            {
                if (renderNode.RenderStage == wireFrameRenderStage)
                {
                    pipelineState.RasterizerState = context.GraphicsDevice.RasterizerStates.WireFrame;
                }
            };

            meshRenderFeature.RenderStageSelectors.Add(new SimpleGroupToRenderStageSelector
            {
                EffectName = "TestEffect",
                RenderStage = wireFrameRenderStage,
            });
        }
    }

    [DataContract("PhysicsDebugCameraRendererMode")]
    public class PhysicsDebugCameraRendererMode : CameraRendererMode
    {
        [DataMemberIgnore]
        public NextGenRenderSystem RenderSystem;

        // Render views
        private RenderView mainRenderView;

        public override string ModelEffect { get; set; }

        [DataMemberIgnore]
        public RenderStage WireFrameRenderStage { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem = Context.Tags.Get(SceneInstance.CurrentRenderSystem);

            WireFrameRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "WireFrame", "WireFrame", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));
           
            var sceneInstance = SceneInstance.GetCurrent(Context);

            // Describe views
            mainRenderView = new RenderView();

            if (WireFrameRenderStage != null)
            {
                mainRenderView.RenderStages.Add(WireFrameRenderStage);
            }

            mainRenderView.SceneInstance = sceneInstance;
            mainRenderView.SceneCameraRenderer = Context.Tags.Get(SceneCameraRenderer.Current);
            mainRenderView.SceneCameraSlotCollection = Context.Tags.Get(SceneCameraSlotCollection.Current);
            RenderSystem.Views.Add(mainRenderView);
        }

        public override void BeforeExtract(RenderContext context)
        {
            base.BeforeExtract(context);

            if (RenderSystem.GetPipelinePlugin<MeshPipelinePlugin>(false) != null)
            {
                RenderSystem.GetPipelinePlugin<WireFramePhysicsDebugPipelinePlugin>(true);
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var renderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
            context.CommandList.Clear(renderFrame.DepthStencil, DepthStencilClearOptions.DepthBuffer);
            RenderSystem.Draw(context, mainRenderView, WireFrameRenderStage);
        }
    }
}