// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics.Font
{
    /// <summary>
    /// Type of a font.
    /// </summary>
    [Flags]
    [DataContract]
    public enum FontStyle
    {
        /// <summary>
        /// A regular font.
        /// </summary>
        Regular = 0,

        /// <summary>
        /// A bold font.
        /// </summary>
        Bold = 1,

        /// <summary>
        /// An italic font.
        /// </summary>
        Italic = 2,
    }
}
