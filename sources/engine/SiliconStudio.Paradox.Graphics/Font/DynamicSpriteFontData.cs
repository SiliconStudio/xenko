// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics.Font
{
    /// <summary>
    /// Data for a dynamic SpriteFont object.
    /// </summary>
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<DynamicSpriteFontData>))]
    public class DynamicSpriteFontData : SpriteFontData
    {
        /// <summary>
        /// Input the family name of the (TrueType) font.
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        ///  Size and style for TrueType fonts in points.
        /// </summary>
        public float DefaultSize { get; set; }

        /// <summary>
        /// Style for the font. 'regular', 'bold' or 'italic'. Default is 'regular
        /// </summary>
        public FontStyle Style { get; set; }

        /// <summary>
        /// Specifies whether to use kerning information when rendering the font. Default value is false (NOT SUPPORTED YET).
        /// </summary>
        public bool UseKerning { get; set; }

        /// <summary>
        /// By default, font textures is a grey. To generate ClearType textures, turn this flag to true 
        /// </summary>
        public FontAntiAliasMode AntiAlias { get; set; }
    }
}