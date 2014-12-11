// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Textures
{
    /// <summary>
    /// Represents the different format of texture possibly desired.
    /// </summary>
    [DataContract]
    public enum TextureFormat
    {
        /// <summary>
        /// The texture is compressed.
        /// </summary>
        /// <userdoc>Compresses the image in hardware supported format.</userdoc>
        Compressed,

        /// <summary>
        /// The texture is in a format having high level of details in color channels (and low in alpha).
        /// </summary>
        /// <userdoc>Convert the image in a format having higher level of details for the color channels</userdoc>
        HighColor,

        /// <summary>
        /// The texture is in a format having the same level of details for all channels (8-bits).
        /// </summary>
        /// <userdoc>Convert the image in a format having an uniform level of details for all channels (8-bits)</userdoc>
        TrueColor,

        /// <summary>
        /// The texture is as it is in the source file.
        /// </summary>
        /// <userdoc>Does not perform any format conversions. Take the image as it is in the source file.</userdoc>
        AsIs,
    }
}