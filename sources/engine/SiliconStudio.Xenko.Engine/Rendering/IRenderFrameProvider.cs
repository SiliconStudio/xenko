// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Rendering
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
        RenderFrame GetRenderFrame(RenderDrawContext context);
    }

    /// <summary>
    /// Extensions for <see cref="IRenderFrameProvider"/>.
    /// </summary>
    public static class RenderFrameProviderExtensions
    {
        /// <summary>
        /// Gets a render frame handling null <see cref="IRenderFrameProvider"/>.
        /// </summary>
        /// <param name="renderFrameProvider">The render frame provider.</param>
        /// <param name="context">The context.</param>
        /// <returns>RenderFrame or null if IRenderFrameProvider is null.</returns>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public static RenderFrame GetSafeRenderFrame(this IRenderFrameProvider renderFrameProvider, RenderDrawContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return renderFrameProvider == null ? null : renderFrameProvider.GetRenderFrame(context);
        }
    }
}