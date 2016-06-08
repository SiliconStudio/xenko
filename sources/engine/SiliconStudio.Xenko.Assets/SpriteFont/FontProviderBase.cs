// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SharpDX.DirectWrite;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("FontProviderBase")]
    public abstract class FontProviderBase
    {
        /// <summary>
        /// Gets or sets the style of the font. A combination of 'regular', 'bold', 'italic'. Default is 'regular'.
        /// </summary>
        /// <userdoc>
        /// The style of the font (regular / bold / italic). Note that this property is ignored is the desired style is not available in the font's source file.
        /// </userdoc>
        [DataMember(40)]
        [Display("Style")]
        public Graphics.Font.FontStyle Style { get; set; } = Graphics.Font.FontStyle.Regular;


        /// <summary>
        /// Gets the associated <see cref="FontFace"/>
        /// </summary>
        /// <returns><see cref="FontFace"/> from the specified source or <c>null</c> if not found</returns>
        public abstract FontFace GetFontFace();

        /// <summary>
        /// Gets the actual file path to the font file
        /// </summary>
        /// <returns>Path to the font file</returns>
        public abstract string GetFontPath();

        /// <summary>
        /// Gets the actual font name
        /// </summary>
        /// <returns>The name of the font</returns>
        public abstract string GetFontName();
    }
}
