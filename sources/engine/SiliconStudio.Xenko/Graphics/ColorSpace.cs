// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
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