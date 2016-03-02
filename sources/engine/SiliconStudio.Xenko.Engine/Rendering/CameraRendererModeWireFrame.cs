using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract("CameraRendererModeWireFrame")]
    [Display("WireFrame")]
    public class CameraRendererModeWireFrame : CameraRenderModeBase
    {
        [DataMemberIgnore]
        public RenderStage WireFrameRenderStage { get; set; }

        [DataMemberIgnore]
        public readonly HashSet<Entity> EnabledEntities = new HashSet<Entity>();

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // Create optional render stages that don't exist yet
            if (WireFrameRenderStage == null)
                WireFrameRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "WireFrame", "WireFrame", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));

            if (WireFrameRenderStage != null)
            {
                MainRenderView.RenderStages.Add(WireFrameRenderStage);
            }

            VisibilityGroup.ViewObjectFilters.Add(MainRenderView, renderObject =>
            {
                var renderMesh = renderObject as RenderMesh;
                return renderMesh != null && EnabledEntities.Contains(renderMesh.RenderModel.ModelComponent.Entity);
            });
        }

        public override void BeforeExtract(RenderContext context)
        {
            base.BeforeExtract(context);

            if (RenderSystem.GetPipelinePlugin<MeshPipelinePlugin>(false) != null)
            {
                // If MeshPipelinePlugin exists and we have wire frame, let's enable WireFrameRenderFeature
                RenderSystem.GetPipelinePlugin<WireFrameMeshPipelinePlugin>(true);
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var renderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
            context.CommandList.Clear(renderFrame.DepthStencil, DepthStencilClearOptions.DepthBuffer);
            RenderSystem.Draw(context, MainRenderView, WireFrameRenderStage);
        }
    }
}