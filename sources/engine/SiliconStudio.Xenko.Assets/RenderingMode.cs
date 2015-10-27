// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// A rendering mode of Preview and Thumbnail for a game.
    /// </summary>
    [DataContract("RenderingMode")]
    public enum RenderingMode
    {
        /// <summary>
        /// The preview and thumbnail will use a low dynamic range settings when displaying assets.
        /// </summary>
        /// <userdoc>The preview and thumbnail will use a low dynamic range settings when displaying assets.</userdoc>
        [Display("Low Dynamic Range")]
        LDR,

        /// <summary>
        /// The preview and thumbnail will use a high dynamic range settings when displaying assets.
        /// </summary>
        /// <userdoc>The preview and thumbnail will use a high dynamic range settings when displaying assets.</userdoc>
        [Display("High Dynamic Range")]
        HDR,
    }
}