// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// The format of the depth stencil buffer.
    /// </summary>
    [DataContract("RenderFrameDepthFormat")]
    public enum RenderFrameDepthFormat
    {
        /// <summary>
        /// Use the depth stencil buffer from the current frame without creating a new one (only if the size are the same)
        /// </summary>
        Shared,

        /// <summary>
        /// No depth stencil is used.
        /// </summary>
        None,

        /// <summary>
        /// A depth only buffer.
        /// </summary>
        Depth,

        /// <summary>
        /// The depth and stencil buffer.
        /// </summary>
        DepthAndStencil,
    }
}