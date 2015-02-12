using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public interface IImageEffect : IGraphicsRenderer
    {
        /// <summary>
        /// Sets an input texture
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="texture">The texture.</param>
        void SetInput(int slot, Texture texture);

        /// <summary>
        /// Sets the render target output.
        /// </summary>
        /// <param name="view">The render target output view.</param>
        /// <exception cref="System.ArgumentNullException">view</exception>
        void SetOutput(Texture view);


        /// <summary>
        /// Sets the render target outputs.
        /// </summary>
        /// <param name="views">The render target output views.</param>
        void SetOutput(params Texture[] views);
    }
}