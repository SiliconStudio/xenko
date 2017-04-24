// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// The colorspace used for textures, materials, lighting...
    /// </summary>
    [DataContract("ColorSpace")]
    public enum ColorSpace
    {
        /// <summary>
        /// Use a linear colorspace.
        /// </summary>
        Linear,

        /// <summary>
        /// Use a gamma colorspace.
        /// </summary>
        Gamma,
    }
}
