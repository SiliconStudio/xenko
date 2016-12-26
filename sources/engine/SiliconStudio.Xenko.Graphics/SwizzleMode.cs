// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Specify how to swizzle a vector.
    /// </summary>
    public enum SwizzleMode
    {
        /// <summary>
        /// Take the vector as is.
        /// </summary>
        None = 0,

        /// <summary>
        /// Take the only the red component of the vector.
        /// </summary>
        RRRR = 1,

        /// <summary>
        /// Reconstructs the Z(B) component from R and G.
        /// </summary>
        NormalMap = 2,

        /// <summary>
        /// Take the only the x component of the vector.
        /// </summary>
        XXXX = RRRR,
    }
}