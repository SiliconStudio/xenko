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
            // Collect with current RenderView
            RenderStage.Output = context.RenderOutput;
            context.RenderView.RenderStages.Add(RenderStage);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Draw with current RenderView
            context.RenderContext.RenderSystem.Draw(context, context.RenderContext.RenderView, RenderStage);
        }
    }
}