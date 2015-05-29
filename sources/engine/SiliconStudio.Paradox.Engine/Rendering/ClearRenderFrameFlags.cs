// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Flags used to clear a render frame.
    /// </summary>
    [DataContract]
    public enum ClearRenderFrameFlags
    {
        /// <summary>
        /// Clears the Color and DepthStencil buffer.
        /// </summary>
        Color,

        /// <summary>
        /// Clears only the depth.
        /// </summary>
        DepthOnly,
    }
}