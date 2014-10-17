// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Graphics.Font;

namespace SiliconStudio.Paradox.Assets.SpriteFont
{
    /// <summary>
    /// Description of a font.
    /// </summary>
    [DataContract("SpriteFont")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(SpriteFontAssetCompiler))]
    [ThumbnailCompiler(PreviewerCompilerNames.FontThumbnailCompilerQualifiedName)]
    [AssetFactory(typeof(SpriteFontFactory))]
    [AssetDescription("Sprite Font", "A sprite containing a rendered font", true)]
    public class SpriteFontAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="SpriteFontAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxfnt";

        /// <summary>
        /// Gets or sets the source file containing the font data if required.  
        /// </summary>
        /// <value>The source.</value>
        [DataMember(10)]
        public UFile Source { get; set; }

        /// <summary>
        /// Input can be either a system (TrueType) font or a specially marked bitmap file.
        /// </summary>
        [DataMember(20)]
        public string FontName { get; set; }

        /// <summary>
        ///  Size in points for TrueType fonts (ignored when converting a bitmap font).
        /// </summary>
        [DataMember(30)]
        [StepRangeAttribute(1, 500, 1, 10)]
        public float Size { get; set; }
        
        /// <summary>
        /// Style for the font. 'regular', 'bold', 'italic', 'underline', 'strikeout'. Default is 'regular
        /// </summary>
        [DataMember(40)]
        public FontStyle Style { get; set; }

        /// <summary>
        ///  Determine if the characters are pre-generated off-line or at run-time.
        /// </summary>
        [DataMember(50)]
        public bool IsDynamic { get; set; }

        /// <summary>
        /// Fallback character used when asked to render a codepoint that is not
        /// included in the font. If zero, missing characters throw exceptions.
        /// </summary>
        [DataMember(60)]
        public char DefaultCharacter { get; set; }
        
        /// <summary>
        ///  A text file referencing which characters to include when generating the static fonts (eg. "ABCDEF...")
        /// </summary>
        [DataMember(70)]
        public UFile CharacterSet { get; set; }

        /// <summary>
        /// Which addition characters range to include when generating the static fonts (eg. "/CharacterRegion:0x20-0x7F /CharacterRegion:0x123")
        /// </summary>
        [DataMember(80)]
        public List<CharacterRegion> CharacterRegions { get; set; }

        /// <summary>
        /// Format of the output texture. Values: 'auto', 'rgba32', 'bgra4444', 'compressedmono'. Default is 'auto'
        /// </summary>
        [DataMember(100)]
        [DefaultValue(FontTextureFormat.Rgba32)]
        public FontTextureFormat Format { get; set; }

        /// <summary>
        /// By default, font textures is a grey. To generate ClearType textures, turn this flag to true 
        /// </summary>
        [DataMember(110)]
        public FontAntiAliasMode AntiAlias { get; set; }

        /// <summary>
        /// By default, font textures use premultiplied alpha format. Set this if you want interpolative alpha instead.
        /// </summary>
        [DataMember(120)]
        public bool NoPremultiply { get; set; }

        /// <summary>
        /// Extra character spacing in pixels (relative to the font size). Zero is default spacing, negative closer together, positive further apart
        /// </summary>
        [DataMember(130)]
        [StepRangeAttribute(-500, 500, 1, 10)]
        public float Spacing { get; set; }

        /// <summary>
        /// Extra line spacing in pixels (relative to the font size). Zero is default spacing, negative closer together, positive further apart
        /// </summary>
        [DataMember(140)]
        [StepRangeAttribute(-500, 500, 1, 10)]
        public float LineSpacing { get; set; }

        /// <summary>
        /// A factor to apply to the default line gap that separate each line. Default is <c>1.0f</c>
        /// </summary>
        [DataMember(150)]
        [DefaultValue(1.0f)]
        [StepRangeAttribute(-500, 500, 1, 10)]
        public float LineGapFactor { get; set; }

        /// <summary>
        /// A factor to position the space occupied by the line gap before and/or after the font. See remarks. Default is <c>1.0f</c>
        /// </summary>
        /// <remarks>
        /// A Font total height = LineGap * LineGapFactor + Ascent + Descent
        /// A Font baseline = LineGap * LineGapFactor * LineGapBaseLineFactor + Ascent
        /// The <see cref="LineGapBaseLineFactor"/> specify where the line gap should start. A value of 1.0 means that the linegap
        /// should appear completely at the top of the line, while 0.0 would mean that the line gap would appear at the bottom
        /// of the line.
        /// </remarks>
        [DataMember(160)]
        [DefaultValue(1.0f)]
        [StepRangeAttribute(-500, 500, 1, 10)]
        public float LineGapBaseLineFactor { get; set; }

        /// <summary>
        /// Specifies whether to use kerning information when rendering the font. Default value is false (NOT SUPPORTED YET).
        /// </summary>
        [DataMember(170)]
        public bool UseKerning { get; set; }

        public SpriteFontAsset()
        {
            DefaultCharacter = ' ';
            Style = FontStyle.Regular;
            CharacterRegions = new List<CharacterRegion>();
            LineGapFactor = 1.0f;
            LineGapBaseLineFactor = 1.0f;
        }

        /// <summary>
        /// Creates a default instance.
        /// </summary>
        /// <returns>A default instance of <see cref="SpriteFontAsset"/>.</returns>
        public static SpriteFontAsset Default()
        {
            var font = new SpriteFontAsset
                {
                    Format = FontTextureFormat.Rgba32,
                    FontName = "Arial",
                    Size = 16,
                };
            font.CharacterRegions.Add(new CharacterRegion(' ', (char)127));

            return font;
        }

        internal string SafeCharacterSet { get { return CharacterSet ?? ""; } }
        
        private class SpriteFontFactory : IAssetFactory
        {
            public Asset New()
            {
                return Default();
            }
        }
    }
}