namespace SiliconStudio.Xenko.Rendering
{
    public interface IRenderCollector
    {
        /// <summary>
        /// Executed before extract. Should create views, update RenderStages, etc...
        /// </summary>
        /// <param name="context"></param>
        void Collect(RenderContext context);
    }
}