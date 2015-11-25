// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("PregeneratedSpriteFont")]
    [AssetDescription(FileExtension, false)]
    [AssetCompiler(typeof(PrecompiledSpriteFontAssetCompiler))]
    [Display(105, "Sprite Font (Precompiled)")]
    public class PrecompiledSpriteFontAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="PrecompiledSpriteFontAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkpcfnt";

        /// <summary>
        /// The reference to the original source asset.
        /// </summary>
        /// <userdoc>The sprite font asset that has been used to generate this precompiled font.</userdoc>
        [DataMember(0)]
        public AssetReference<SpriteFontAsset> Source;

        /// <summary>
        /// The file containing the font data.
        /// </summary>
        /// <userdoc>The image file containing the extracted font data.</userdoc>
        [DataMember(10)]
        public UFile FontDataFile;

        [Display(Browsable = false)]
        public string FontName; // Note: this field is used only for thumbnail.

        [DefaultValue(FontStyle.Regular)]
        [Display(Browsable = false)]
        public FontStyle Style; // Note: this field is used only for thumbnail.

        [DefaultValue(false)]
        [Display(Browsable = false)]
        public bool IsNotPremultiplied; // Note: this field is used only for thumbnail / preview.

        /// <summary>
        /// The size in points (pt).
        /// </summary>
        [Display(Browsable = false)]
        public float Size;

        [Display(Browsable = false)]
        public List<Glyph> Glyphs;

        [Display(Browsable = false)]
        public float BaseOffset;

        [Display(Browsable = false)]
        public float DefaultLineSpacing;

        [Display(Browsable = false)]
        public List<Kerning> Kernings;

        [DefaultValue(0)]
        [Display(Browsable = false)]
        public float ExtraSpacing;

        [DefaultValue(0)]
        [Display(Browsable = false)]
        public float ExtraLineSpacing;

        [DefaultValue(' ')]
        [Display(Browsable = false)]
        public char DefaultCharacter;
    }
}