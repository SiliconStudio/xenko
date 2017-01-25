namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Renders a single stage with the current <see cref="RenderView"/>.
    /// </summary>
    public partial class SingleStageRenderer : SceneRendererBase
    {
        public RenderStage MainRenderStage;
        public RenderStage TransparentRenderStage;

        protected override void CollectCore(RenderContext context)
        {
            // Main
            MainRenderStage.Output = context.RenderOutput;
            context.RenderView.RenderStages.Add(MainRenderStage);

            // Transparent
            if (TransparentRenderStage != null)
            {
                TransparentRenderStage.Output = context.RenderOutput;
                context.RenderView.RenderStages.Add(TransparentRenderStage);
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            context.RenderContext.RenderSystem.Draw(context, context.RenderContext.RenderView, MainRenderStage);

            if (TransparentRenderStage != null)
                context.RenderContext.RenderSystem.Draw(context, context.RenderContext.RenderView, TransparentRenderStage);
        }
    }
}