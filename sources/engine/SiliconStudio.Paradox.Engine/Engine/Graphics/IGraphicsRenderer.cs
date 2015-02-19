// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A graphics renderer.
    /// </summary>
    public interface IGraphicsRenderer : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphicsRenderer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set; }

        /// <summary>
        /// Loads this renderer. See remarks.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>This method allow a renderer to prepare for rendering. This method should be called once to initialize a renderer.</remarks>
        void Initialize(RenderContext context);

        /// <summary>
        /// Draws this renderer with the specified context. See remarks.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>The method <see cref="Initialize"/> should be called automatically by the implementation if it was not done before the first draw.</remarks>
        void Draw(RenderContext context);
    }
}