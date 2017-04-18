using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    [Display("Debug Renderer")]
    public class DebugRenderer : SceneRendererBase, ISharedRenderer
    {
        public List<RenderStage> DebugRenderStages { get; } = new List<RenderStage>();

        protected override void CollectCore(RenderContext context)
        {
            foreach (var renderStage in DebugRenderStages)
            {
                if (renderStage == null)
                    continue;

                renderStage.Output = context.RenderOutput;
                context.RenderView.RenderStages.Add(renderStage);
            }
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            foreach (var renderStage in DebugRenderStages)
            {
                if (renderStage == null)
                    continue;

                drawContext.RenderContext.RenderSystem.Draw(drawContext, drawContext.RenderContext.RenderView, renderStage);
            }
        }
    }
}