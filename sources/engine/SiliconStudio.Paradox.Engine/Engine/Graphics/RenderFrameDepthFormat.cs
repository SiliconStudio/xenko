// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// The format of the depth stencil buffer.
    /// </summary>
    [DataContract("RenderFrameDepthFormat")]
    public enum RenderFrameDepthFormat
    {
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