// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Extensions methods for the <see cref="RenderContext"/> class
    /// </summary>
    public static class RenderContextExtensions
    {
        /// <summary>
        /// Query the render context whether the current rendering is for picking.
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns><value>True</value> if the current rendering is for picking</returns>
        public static bool IsPicking(this RenderContext context)
        {
            var thisInstacnce = context.Tags.Get(SceneCameraRenderer.Current);
            return thisInstacnce != null && thisInstacnce.IsPickingMode;
        }
    }
}