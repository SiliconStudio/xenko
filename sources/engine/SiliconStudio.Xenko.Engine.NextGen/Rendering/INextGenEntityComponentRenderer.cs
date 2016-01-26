namespace SiliconStudio.Xenko.Rendering
{
    public interface INextGenEntityComponentRenderer : IGraphicsRendererCore
    {
        /// <summary>
        /// Extract information from the component, and fill the appropriate collections/views in <see cref="NextGenRenderSystem"/>.
        /// </summary>
        /// <param name="renderSystem"></param>
        void Extract(NextGenRenderSystem renderSystem);

        /// <summary>
        /// Performs drawing operations.
        /// </summary>
        /// <param name="renderSystem"></param>
        void Draw(NextGenRenderSystem renderSystem);
    }
}