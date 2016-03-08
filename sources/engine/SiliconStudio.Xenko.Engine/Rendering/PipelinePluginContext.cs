namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Context used by <see cref="PipelinePluginManager"/>.
    /// </summary>
    public struct PipelinePluginContext
    {
        public readonly RenderContext RenderContext;
        public readonly NextGenRenderSystem RenderSystem;

        public PipelinePluginContext(RenderContext renderContext, NextGenRenderSystem renderSystem)
        {
            RenderContext = renderContext;
            RenderSystem = renderSystem;
        }
    }
}