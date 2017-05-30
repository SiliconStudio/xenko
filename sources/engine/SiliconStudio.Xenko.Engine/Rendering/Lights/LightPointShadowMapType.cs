// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Possible types of shadow mapping for point lights
    /// </summary>
    [DataContract]
    public enum LightPointShadowMapType
    {
        /// <summary>
        /// Renders the scene only twice to 2 hemisphere textures, might look distorted or generate artifacts with low-poly shadow casters
        /// </summary>
        DualParaboloid,
        /// <summary>
        /// Renders the scene to 6 faces of a cube, provides more stable shadow maps with the tradoff of having to render the scene 6 times
        /// </summary>
        CubeMap,
    }
}
