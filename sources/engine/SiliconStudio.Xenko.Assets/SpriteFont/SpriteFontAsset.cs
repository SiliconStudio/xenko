// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    /// <summary>
    /// Description of a font.
    /// </summary>
    [DataContract("SpriteFont")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(SpriteFontAssetCompiler))]
    [ObjectFactory(typeof(SpriteFontFactory))]
    [Display(140, "Sprite Font")]
    [CategoryOrder(10, "Font")]
    [CategoryOrder(20, "Characters")]
    [CategoryOrder(30, "Rendering")]
    public class SpriteFontAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="SpriteFontAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkfnt;.pdxfnt";

        /// <summary>
        /// Gets or sets the source file containing the font data. This can be a TTF file or a bitmap file.
        /// If null, <see cref="FontName"/> is used to determine the font source.
        /// </summary>
        /// <value>The source.</value>
        /// <userdoc>
        /// The path to the file containing the font data to use.
        /// </userdoc>
        [DataMember(10)]
        [Display(null, "Font")]
        public UFile Source { get; set; }

        /// <summary>
        /// Gets or sets the name of the font family to use when the <see cref="Source"/> is not specified.
        /// </summary>
        /// <userdoc>
        /// The name of the font family to use. Only the fonts installed on the system can be used here.
        /// </userdoc>
        [DataMember(20)]
        [Display(null, "Font")]
        public string FontName { get; set; }

        /// <summary>
        ///  Gets or sets the size in points of the font (ignored when converting a bitmap font).
        /// </summary>
        /// <userdoc>
        /// The size of the font (in points) for static fonts, the default size for dynamic fonts. This property is ignored when the font source is a bitmap.
        /// </userdoc>
        [DataMember(30)]
        [Display(null, "Font")]
        public float Size { get; set; }
        
        /// <summary>
        /// Gets or sets the style of the font. A combination of 'regular', 'bold', 'italic'. Default is 'regular'.
        /// </summary>
        /// <userdoc>
        /// The style of the font (regular / bold / italic). Note that this property is ignored is the desired style is not available in the font's source file.
        /// </userdoc>
        [DataMember(40)]
        [Display(null, "Font")]
        public FontStyle Style { get; set; }

        /// <summary>
        ///  Gets or sets the value determining if the characters are pre-generated off-line or at run-time.
        /// </summary>
        /// <userdoc>
        /// If checked, the font textures are generated at execution time. If not, at project build time.
        /// Note that it is not possible to resize at execution time a font that is not dynamic.
        /// </userdoc>
        [DataMember(50)]
        [Display(null, "Font")]
        public bool IsDynamic { get; set; }

        /// <summary>
        /// Gets or sets the fallback character used when asked to render a character that is not
        /// included in the font. If zero, missing characters throw exceptions.
        /// </summary>
        /// <userdoc>
        /// The fallback character to use when a given character is not available in the font file data.
        /// </userdoc>
        [DataMember(60)]
        [Display(null, "Characters")]
        public char DefaultCharacter { get; set; }
        
        /// <summary>
        ///  Gets or sets the text file referencing which characters to include when generating the static fonts (eg. "ABCDEF...")
        /// </summary>
        /// <userdoc>
        /// The path to a file containing the characters to import from the font source file. This property is ignored when 'IsDynamic' is checked.
        /// </userdoc>
        [DataMember(70)]
        [Display(null, "Characters")]
        public UFile CharacterSet { get; set; }

        /// <summary>
        /// Gets or set the additional character ranges to include when generating the static fonts (eg. "/CharacterRegion:0x20-0x7F /CharacterRegion:0x123")
        /// </summary>
        /// <userdoc>
        /// The list of series of character to import from the font source file. This property is ignored when 'IsDynamic' is checked.
        /// Note that this property only represents an alternative way of indicating character to import, the result is the same as using the 'CharacterSet' property.
        /// </userdoc>
        [DataMember(80)]
        [Category]
        [Display(null, "Characters")]
        public List<CharacterRegion> CharacterRegions { get; set; }

        /// <summary>
        /// Gets or sets format of the texture used to render the font.
        /// </summary>
        /// <userdoc>
        /// The format of the texture used to render the Font. This property is currently ignored for dynamic fonts.
        /// </userdoc>
        [DataMember(100)]
        [DefaultValue(FontTextureFormat.Rgba32)]
        [Display(null, "Rendering")]
        public FontTextureFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the font anti-aliasing mode. By default, levels of grays are used.
        /// </summary>
        /// <userdoc>
        /// The type of anti-aliasing to use when rendering the font. 
        /// </userdoc>
        [DataMember(110)]
        [Display(null, "Rendering")]
        public FontAntiAliasMode AntiAlias { get; set; }

        /// <summary>
        /// Gets or sets the value indicating if the font texture should be generated pre-multiplied by alpha component. 
        /// </summary>
        /// <userdoc>
        /// If checked, the texture generated for this font is not pre-multiplied by the alpha component.
        /// Check this property if you prefer to use interpolative alpha blending when rendering the font.
        /// </userdoc>
        [DataMember(120)]
        [Display(null, "Rendering")]
        public bool NoPremultiply { get; set; }

        /// <summary>
        /// Gets or sets the extra character spacing in pixels (relative to the font size). Zero is default spacing, negative closer together, positive further apart
        /// </summary>
        ///  <userdoc>
        /// The extra spacing to add between characters in pixels. Zero is default spacing, negative closer together, positive further apart.
        /// </userdoc>
        [DataMember(130)]
        [DataMemberRange(-500, 500, 1, 10)]
        [Display(null, "Rendering")]
        public float Spacing { get; set; }

        /// <summary>
        /// Gets or sets the extra line spacing in pixels (relative to the font size). Zero is default spacing, negative closer together, positive further apart.
        /// </summary>
        /// <userdoc>
        /// The extra interline space to add at each return of line (in pixels). Zero is default spacing, negative closer together, positive further apart.
        /// </userdoc>
        [DataMember(140)]
        [DataMemberRange(-500, 500, 1, 10)]
        [Display(null, "Rendering")]
        public float LineSpacing { get; set; }

        /// <summary>
        /// Gets or sets the factor to apply to the default line gap that separate each line. Default is <c>1.0f</c>
        /// </summary>
        /// <userdoc>
        /// The factor to use when calculating the LineGap of the font. 
        /// The LineGap affects both the space between two lines and the space at the top of the first line.
        /// </userdoc>
        [DataMember(150)]
        [DefaultValue(1.0f)]
        [DataMemberRange(-500, 500, 1, 10)]
        [Display(null, "Rendering")]
        public float LineGapFactor { get; set; }

        /// <summary>
        /// Gets or sets the factor to apply to LineGap when calculating the font base line. See remarks. Default is <c>1.0f</c>
        /// </summary>
        /// <remarks>
        /// A Font total height = LineGap * LineGapFactor + Ascent + Descent
        /// A Font baseline = LineGap * LineGapFactor * LineGapBaseLineFactor + Ascent
        /// The <see cref="LineGapBaseLineFactor"/> specify where the line gap should start. A value of 1.0 means that the line gap
        /// should appear completely at the top of the line, while 0.0 would mean that the line gap would appear at the bottom
        /// of the line.
        /// </remarks>
        /// <userdoc>
        /// The factor to use when calculating the font base line. Moving the base line of font changes the repartition of the space at the top/bottom of the line.
        /// </userdoc>
        [DataMember(160)]
        [DefaultValue(1.0f)]
        [DataMemberRange(-500, 500, 1, 10)]
        [Display(null, "Rendering")]
        public float LineGapBaseLineFactor { get; set; }

        /// <summary>
        /// Gets or sets the value specifying whether to use kerning information when rendering the font. Default value is false (NOT SUPPORTED YET).
        /// </summary>
        /// <userdoc>
        /// If checked, kerning information is imported from the font. (NOT SUPPORTED YET)
        /// </userdoc>
        [DataMember(170)]
        [Display(null, "Rendering")]
        public bool UseKerning { get; set; }

        public SpriteFontAsset()
        {
            DefaultCharacter = ' ';
            Style = FontStyle.Regular;
            CharacterRegions = new List<CharacterRegion>();
            LineGapFactor = 1.0f;
            LineGapBaseLineFactor = 1.0f;
            Source = new UFile("");
            CharacterSet = new UFile("");
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
        
        private class SpriteFontFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return Default();
            }
        }
    }
}