// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets
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
