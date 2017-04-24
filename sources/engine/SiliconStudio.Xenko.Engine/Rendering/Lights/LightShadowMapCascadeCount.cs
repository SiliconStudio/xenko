// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Number of cascades used for a shadow map.
    /// </summary>
    [DataContract("LightShadowMapCascadeCount")]
    public enum LightShadowMapCascadeCount
    {
        /// <summary>
        /// A shadow map with one cascade.
        /// </summary>
        [Display("One Cascade")]
        OneCascade = 1,

        /// <summary>
        /// A shadow map with two cascades.
        /// </summary>
        [Display("Two Cascades")]
        TwoCascades = 2,

        /// <summary>
        /// A shadow map with four cascades.
        /// </summary>
        [Display("Four Cascades")]
        FourCascades = 4
    }
}
