namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Renders a single stage with the current <see cref="RenderView"/>.
    /// </summary>
    public partial class SingleStageRenderer : SceneRendererBase, ISharedRenderer
    {
        public RenderStage RenderStage;

        protected override void CollectCore(RenderContext context)
        {
            // Fill RenderStage formats
            RenderStage.Output = context.RenderOutputs.Peek();

            context.RenderView.RenderStages.Add(RenderStage);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            context.RenderContext.RenderSystem.Draw(context, context.RenderContext.RenderView, RenderStage);
        }
    }
}