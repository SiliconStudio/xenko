// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Common interface for the provider of a <see cref="RenderFrame"/>.
    /// </summary>
    public interface IRenderFrameProvider : IDisposable
    {
        /// <summary>
        /// Gets the render frame.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>RenderFrame.</returns>
        RenderFrame GetRenderFrame(RenderContext context);
    }
}