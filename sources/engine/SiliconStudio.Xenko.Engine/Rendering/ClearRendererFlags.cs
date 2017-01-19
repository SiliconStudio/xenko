// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Flags used to clear a render frame.
    /// </summary>
    [DataContract("ClearRenderFrameFlags")]
    public enum ClearRendererFlags
    {
        /// <summary>
        /// Clears both the Color and DepthStencil buffer.
        /// </summary>
        /// <userdoc>Clears both the Color and DepthStencil buffers</userdoc>
        [Display("Color and Depth")]
        [DataAlias("Color")] // The previous name was using `Color` only
        ColorAndDepth,

        /// <summary>
        /// Clears only the Color buffer.
        /// </summary>
        /// <userdoc>Clears only the Color buffer.</userdoc>
        [Display("Color Only")]
        ColorOnly,

        /// <summary>
        /// Clears only the depth.
        /// </summary>
        /// <userdoc>Clears only the DepthStencil buffer</userdoc>
        [Display("Depth Only")]
        DepthOnly,
    }
}