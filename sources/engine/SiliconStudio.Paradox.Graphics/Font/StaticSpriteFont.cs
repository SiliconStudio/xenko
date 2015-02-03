// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Graphics.Font
{
    [ContentSerializer(typeof(DataContentConverterSerializer<StaticSpriteFont>))]
    internal class StaticSpriteFont : SpriteFont
    {
        private readonly Dictionary<char, Glyph> characterToGlyph;

        internal Texture[] StaticTextures;

        public StaticSpriteFont(FontSystem fontSystem, StaticSpriteFontData spriteFontData)
            : base(fontSystem, spriteFontData, false)
        {
            characterToGlyph = new Dictionary<char, Glyph>(spriteFontData.Glyphs.Length);

            // build the character map
            foreach (var glyph in spriteFontData.Glyphs)
            {
                var character = (char)glyph.Character;
                characterToGlyph[character] = glyph;
            }

            // Prepare kernings if they are available.
            var kernings = spriteFontData.Kernings;
            if (kernings != null)
            {
                for (int i = 0; i < kernings.Length; i++)
                {
                    int key = (kernings[i].First << 16) | kernings[i].Second;
                    KerningMap.Add(key, kernings[i].Offset);
                }
            }

            // Read the texture data.
            StaticTextures = new Texture[spriteFontData.Bitmaps.Length];
            for (int i = 0; i < StaticTextures.Length; i++)
            {
                if (spriteFontData.Bitmaps[i].Value != null)
                    StaticTextures[i] = Texture.New(fontSystem.GraphicsDevice, spriteFontData.Bitmaps[i].Value).DisposeBy(this);
            }
            Textures = StaticTextures;

            BaseOffsetY = spriteFontData.BaseOffset;
            DefaultLineSpacing = spriteFontData.FontDefaultLineSpacing;
        }

        public override float GetExtraSpacing(float fontSize)
        {
            return ExtraSpacing;
        }

        public override float GetExtraLineSpacing(float fontSize)
        {
            return ExtraLineSpacing;
        }

        public override float GetFontDefaultLineSpacing(float fontSize)
        {
            return DefaultLineSpacing;
        }

        protected override float GetBaseOffsetY(float fontSize)
        {
            return BaseOffsetY;
        }

        public override bool IsCharPresent(char c)
        {
            return characterToGlyph.ContainsKey(c);
        }

        protected override Glyph GetGlyph(char character, ref Vector2 fontSize, bool dumb)
        {
            Glyph glyph = null;

            if (!characterToGlyph.ContainsKey(character))
                Logger.Warning("Character '{0}' is not available in the static font character map", character);
            else
                glyph = characterToGlyph[character];

            return glyph;
        }
    }
}