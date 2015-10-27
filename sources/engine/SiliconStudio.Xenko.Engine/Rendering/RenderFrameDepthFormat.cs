// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
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
        /// <userdoc>Use the depth stencil buffer from the current frame without creating a new one (only if the size are the same).</userdoc>
        Shared,

        /// <summary>
        /// No depth stencil is used.
        /// </summary>
        /// <userdoc>No depth stencil is used.</userdoc>
        None,

        /// <summary>
        /// A depth only buffer.
        /// </summary>
        /// <userdoc>A depth only buffer.</userdoc>
        Depth,

        /// <summary>
        /// The depth and stencil buffer.
        /// </summary>
        /// <userdoc>The depth and stencil buffer.</userdoc>
        DepthAndStencil,
    }
}