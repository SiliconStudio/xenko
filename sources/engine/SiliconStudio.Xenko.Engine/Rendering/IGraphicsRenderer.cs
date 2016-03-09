// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A graphics renderer.
    /// </summary>
    public interface IGraphicsRenderer : IGraphicsRendererCore
    {
        /// <summary>
        /// Draws this renderer with the specified context. See remarks.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>The method <see cref="IGraphicsRendererCore.Initialize"/> should be called automatically by the implementation if it was not done before the first draw.</remarks>
        void Draw(RenderDrawContext context);
    }
}