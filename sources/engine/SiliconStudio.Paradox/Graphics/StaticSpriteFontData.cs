// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Graphics.Font;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Data for a static SpriteFont object that supports kerning.
    /// </summary>
    /// <remarks>
    /// Loading of SpriteFontData supports DirectXTk "MakeSpriteFont" format and AngelCode Bitmap Font Maker (binary format).
    /// </remarks>
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<StaticSpriteFontData>))]
    public class StaticSpriteFontData : SpriteFontData
    {
        /// <summary>
        /// The number of pixels from the absolute top of the line to the base of the characters.
        /// </summary>
        public float BaseOffset;

        /// <summary>
        /// The default line spacing of the font.
        /// </summary>
        public float FontDefaultLineSpacing;

        /// <summary>
        /// An array of <see cref="Glyph"/> data.
        /// </summary>
        public Glyph[] Glyphs;

        /// <summary>
        /// An array of <see cref="Image"/> data.
        /// </summary>
        public ContentReference<Image>[] Bitmaps;

        /// <summary>
        /// An array of <see cref="Kerning"/> data.
        /// </summary>
        public Kerning[] Kernings;
    }
}