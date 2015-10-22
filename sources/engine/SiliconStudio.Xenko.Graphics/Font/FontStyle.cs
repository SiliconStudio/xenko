// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics.Font
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