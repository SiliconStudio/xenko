// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("SpriteFontTypeBase")]
    public abstract class SpriteFontTypeBase
    {
        /// <summary>
        ///  Gets or sets the size in points of the font (ignored when converting a bitmap font).
        /// </summary>
        /// <userdoc>
        /// The size of the font (in points) for static fonts, the default size for dynamic fonts. This property is ignored when the font source is a bitmap.
        /// </userdoc>
        [DataMemberIgnore]
        public abstract float Size { get; set; } 

    }
}
