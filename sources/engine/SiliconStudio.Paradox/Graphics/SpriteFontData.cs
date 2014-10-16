// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Data for a SpriteFont object.
    /// </summary>
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<SpriteFontData>))]
    public class SpriteFontData
    {
        /// <summary>
        /// The size of the font in pixels (default size for dynamic fonts).
        /// </summary>
        public float Size;

        /// <summary>
        /// The character extra spacing in pixels. Zero is default spacing, negative closer together, positive further apart.
        /// </summary>
        public float ExtraSpacing;

        /// <summary>
        /// This is the extra distance in pixels to add between each line of text. Zero is default spacing, negative closer together, positive further apart.
        /// </summary>
        public float ExtraLineSpacing;

        /// <summary>
        /// The default character fall-back.
        /// </summary>
        public char DefaultCharacter;
    }
}