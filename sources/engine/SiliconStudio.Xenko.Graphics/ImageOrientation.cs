// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Defines the possible rotations to apply on image regions.
    /// </summary>
    public enum ImageOrientation
    {
        /// <summary>
        /// The image region is taken as is.
        /// </summary>
        AsIs = 0,

        /// <summary>
        /// The image is rotated of the 90 degrees (clockwise) in the source texture.
        /// </summary>
        Rotated90 = 1,
    }
}
