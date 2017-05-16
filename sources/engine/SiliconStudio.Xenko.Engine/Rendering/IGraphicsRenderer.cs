// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Rendering
{
    public interface IGraphicsRendererBase
    {
        /// <summary>
        /// Draws this renderer with the specified context. See remarks.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>The method <see cref="IGraphicsRendererCore.Initialize"/> should be called automatically by the implementation if it was not done before the first draw.</remarks>
        void Draw(RenderDrawContext context);
    }

    /// <summary>
    /// A graphics renderer.
    /// </summary>
    public interface IGraphicsRenderer : IGraphicsRendererBase, IGraphicsRendererCore
    {
    }
}
