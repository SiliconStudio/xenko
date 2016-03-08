namespace SiliconStudio.Xenko.Rendering
{
    public interface INextGenRenderer
    {
        /// <summary>
        /// Executed before extract. Should create views, update RenderStages, etc...
        /// </summary>
        /// <param name="context"></param>
        void BeforeExtract(RenderContext context);
    }
}