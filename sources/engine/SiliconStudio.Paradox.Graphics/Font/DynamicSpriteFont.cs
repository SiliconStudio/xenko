// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics.Font
{
    /// <summary>
    /// A dynamic font. That is a font that generate its character bitmaps at execution.
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<DynamicSpriteFont>))]
    internal class DynamicSpriteFont : SpriteFont
    {
        /// <summary>
        /// Input the family name of the (TrueType) font.
        /// </summary>
        private readonly string fontName;

        /// <summary>
        /// Style for the font. 'regular', 'bold' or 'italic'. Default is 'regular
        /// </summary>
        private readonly FontStyle style;

        /// <summary>
        /// Specifies whether to use kerning information when rendering the font. Default value is false (NOT SUPPORTED YET).
        /// </summary>
        private readonly bool useKerning;

        /// <summary>
        /// The alias mode of the font
        /// </summary>
        private readonly FontAntiAliasMode antiAlias;

        /// <summary>
        /// The character specifications cached to avoid re-allocations
        /// </summary>
        private readonly Dictionary<CharacterKey, CharacterSpecification> sizedCharacterToCharacterData = new Dictionary<CharacterKey, CharacterSpecification>();

        internal FontManager FontManager
        {
            get { return FontSystem.FontManager; }
        }

        internal FontCacheManager FontCacheManager
        {
            get { return FontSystem.FontCacheManager; }
        }

        internal int FrameCount
        {
            get { return FontSystem.FrameCount; }
        }

        public DynamicSpriteFont(FontSystem fontSystem, DynamicSpriteFontData fontData)
            : base(fontSystem, fontData, true)
        {
            // import font properties from font data
            style = fontData.Style;
            fontName = fontData.FontName;
            useKerning = fontData.UseKerning;
            antiAlias = fontData.AntiAlias;

            // retrieve needed info from the font
            float relativeLineSpacing;
            float relativeBaseOffsetY;
            float relativeMaxWidth;
            float relativeMaxHeight;
            FontManager.GetFontInfo(fontData.FontName, fontData.Style, out relativeLineSpacing, out relativeBaseOffsetY, out relativeMaxWidth, out relativeMaxHeight);

            // set required base properties
            DefaultLineSpacing = relativeLineSpacing * Size;
            BaseOffsetY = relativeBaseOffsetY * Size;
            Textures = FontCacheManager.Textures;
            Swizzle = SwizzleMode.RRRR;
        }

        public override bool IsCharPresent(char c)
        {
            return FontManager.DoesFontContains(fontName, style, c);
        }

        protected override Glyph GetGlyph(char character, ref Vector2 fontSize, bool uploadGpuResources)
        {
            // get the character data associated to the provided character and size
            var characterData = GetOrCreateCharacterData(fontSize, character);
            
            // generate the bitmap if it does not exist
            if(characterData.Bitmap == null)
                FontManager.GenerateBitmap(characterData, false);

            // upload the character to the GPU font texture and create the glyph if does not exists
            if (uploadGpuResources && characterData.Bitmap != null && !characterData.IsBitmapUploaded)
                FontCacheManager.UploadCharacterBitmap(characterData);

            // update the character usage info
            FontCacheManager.NotifyCharacterUtilization(characterData);

            return characterData.Glyph;
        }

        internal override void PreGenerateGlyphs(ref StringProxy text, ref Vector2 size)
        {
            for (int i = 0; i < text.Length; i++)
            {
                // get the character data associated to the provided character and size
                var characterData = GetOrCreateCharacterData(size, text[i]);

                // force asynchronous generation of the bitmap if it does not exist
                if (characterData.Bitmap == null)
                    FontManager.GenerateBitmap(characterData, true);
            }
        }

        private CharacterSpecification GetOrCreateCharacterData(Vector2 size, char character)
        {
            // build the dictionary look up key
            var lookUpKey = new CharacterKey(character, size);

            // get the entry (creates it if it does not exist)
            CharacterSpecification characterData;
            if (!sizedCharacterToCharacterData.TryGetValue(lookUpKey, out characterData))
            {
                characterData = new CharacterSpecification(character, fontName, size, style, antiAlias);
                sizedCharacterToCharacterData[lookUpKey] = characterData;
            }
            return characterData;
        }

        private struct CharacterKey : IEquatable<CharacterKey>
        {
            private readonly char character;

            private readonly Vector2 size;

            public CharacterKey(char character, Vector2 size)
            {
                this.character = character;
                this.size = size;
            }

            public bool Equals(CharacterKey other)
            {
                return character == other.character && size == other.size;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is CharacterKey && Equals((CharacterKey)obj);
            }

            public override int GetHashCode()
            {
                return character.GetHashCode();
            }
        }
    }
}