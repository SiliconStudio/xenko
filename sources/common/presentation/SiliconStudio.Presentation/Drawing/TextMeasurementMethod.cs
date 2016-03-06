// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Presentation
{
    [Serializable]
    public enum TextMeasurementMethod
    {
        /// <summary>
        /// Measurement by TextBlock.
        /// </summary>
        TextBlock,

        /// <summary>
        /// Measurement by glyph typeface.
        /// </summary>
        GlyphTypeface
    }
}
