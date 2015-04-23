// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// The pixel format of the render target.
    /// </summary>
    [DataContract("RenderFrameFormat")]
    public enum RenderFrameFormat
    {
        /// <summary>
        /// No render target.
        /// </summary>
        [Display("None")]
        None,

        /// <summary>
        /// The rendering target is a 32bits bits targets (4 x 16 bits half floats per RGBA component).
        /// </summary>
        [Display("Low Dynamic Range")]
        LDR,

        /// <summary>
        /// The rendering target is a floating point 64 bits targets (4 x 16 bits half floats per RGBA component).
        /// </summary>
        [Display("High Dynamic Range")]
        HDR,
    }
}