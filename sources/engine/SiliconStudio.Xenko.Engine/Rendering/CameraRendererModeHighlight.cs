using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract("CameraRendererModeHighlight")]
    [Display("Highlight")]
    public class CameraRendererModeHighlight : CameraRenderModeBase
    {
        [DataMemberIgnore]
        public RenderStage HighlightRenderStage { get; set; }

        [DataMemberIgnore]
        public readonly HashSet<Entity> EnabledEntities = new HashSet<Entity>();

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // Create optional render stages that don't exist yet
            if (HighlightRenderStage == null)
                HighlightRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "Highlight", "Highlight", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));

            if (HighlightRenderStage != null)
            {
                MainRenderView.RenderStages.Add(HighlightRenderStage);
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
                RenderSystem.GetPipelinePlugin<HighlightMeshPipelinePlugin>(true);
            }
        }
        protected override void DrawCore(RenderDrawContext context)
        {
            RenderSystem.Draw(context, MainRenderView, HighlightRenderStage);
        }
    }
}