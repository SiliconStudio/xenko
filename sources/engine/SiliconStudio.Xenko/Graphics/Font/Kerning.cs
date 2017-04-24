// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics.Font
{
    /// <summary>
    /// Describes kerning information.
    /// </summary>
    [DataContract]
    public struct Kerning
    {
        /// <summary>
        /// Unicode for the 1st character.
        /// </summary>
        public int First;

        /// <summary>
        /// Unicode for the 2nd character.
        /// </summary>
        public int Second;

        /// <summary>
        /// X Offsets in pixels to apply between the 1st and 2nd character.
        /// </summary>
        public float Offset;
    }
}
