namespace SiliconStudio.Xenko.Rendering
{
    public interface IPipelinePlugin
    {
        void SetupPipeline(RenderContext context, NextGenRenderSystem renderSystem);
    }
}