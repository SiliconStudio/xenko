namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Automatically register part of the <see cref="RenderSystem"/> pipeline.
    /// </summary>
    public interface IPipelinePlugin
    {
        void Load(PipelinePluginContext context);
        void Unload(PipelinePluginContext context);
    }
}