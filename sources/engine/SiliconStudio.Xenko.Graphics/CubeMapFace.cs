// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Defines the faces of a cube map for <see cref="TextureCube"/>.
    /// </summary>
    public enum CubeMapFace
    {
        /// <summary>
        /// Positive x-face of the cube map.
        /// </summary>
        PositiveX,
        /// <summary>
        /// Negative x-face of the cube map.
        /// </summary>
        NegativeX,
        /// <summary>
        /// Positive y-face of the cube map.
        /// </summary>
        PositiveY,
        /// <summary>
        /// Negative y-face of the cube map.
        /// </summary>
        NegativeY,
        /// <summary>
        /// Positive z-face of the cube map.
        /// </summary>
        PositiveZ,
        /// <summary>
        /// Negative z-face of the cube map.
        /// </summary>
        NegativeZ
    }
}
