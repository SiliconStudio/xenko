// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A graphics renderer.
    /// </summary>
    public interface IGraphicsRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphicsRenderer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set; }

        /// <summary>
        /// Draws this renderer with the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        void Draw(RenderContext context);
    }
}