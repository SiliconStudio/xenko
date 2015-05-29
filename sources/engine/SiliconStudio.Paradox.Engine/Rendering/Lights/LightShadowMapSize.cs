// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Size hint of a shadow map. See remarks.
    /// </summary>
    /// <remarks>This is a hint to determine the size of a shadow map</remarks>
    [DataContract("LightShadowMapSize")]
    public enum LightShadowMapSize
    {
        /// <summary>
        /// Use a small size.
        /// </summary>
        Small = 0, // NOTE: Number are used to compute the size, do not change them

        /// <summary>
        /// Use a medium size.
        /// </summary>
        Medium = 1,
            
        /// <summary>
        /// Use a large size.
        /// </summary>
        Large = 2,
    }
}