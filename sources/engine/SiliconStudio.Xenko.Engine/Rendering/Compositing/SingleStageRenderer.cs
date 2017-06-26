// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Renders a single stage with the current <see cref="RenderView"/>.
    /// </summary>
    public partial class SingleStageRenderer : SceneRendererBase
    {
        public RenderStage RenderStage { get; set; }

        protected override void CollectCore(RenderContext context)
        {
            if (RenderStage == null)
                return;

            // Collect with current RenderView
            RenderStage.OutputValidator.Validate(ref context.RenderOutput);
            context.RenderView.RenderStages.Add(RenderStage);
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            if (RenderStage == null)
                return;

            // Draw with current RenderView
            drawContext.RenderContext.RenderSystem.Draw(drawContext, drawContext.RenderContext.RenderView, RenderStage);
        }
    }
}
