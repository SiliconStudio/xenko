// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    /// <summary>
    /// Description of a font.
    /// </summary>
    [DataContract("SpriteFont")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(SpriteFontAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, "1.7.0-beta04")]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "1.5.0-alpha09", typeof(PremultiplyUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.5.0-alpha09", "1.7.0-beta02", typeof(FontTypeUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta02", "1.7.0-beta03", typeof(FontClassUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta03", "1.7.0-beta04", typeof(FontSizeUpgrader))]
    [Display(140, "Sprite Font")]
    [CategoryOrder(10, "Font")]
    [CategoryOrder(30, "Rendering")]
    public partial class SpriteFontAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="SpriteFontAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkfnt;.pdxfnt";

        [NotNull]
        [DataMember(10)]
        [Display(null, "Font")]
        public FontProviderBase FontSource { get; set; } = new SystemFontProvider();

        /// <summary>
        ///  Gets or sets the value determining if and how the characters are pre-generated off-line or at run-time.
        /// </summary>
        /// <userdoc>
        /// Static font has fixed font size and is pre-compiled
        /// Dynamic font which can change its font size at runtime and is also compiled at runtime
        /// Signed Distance Field font is pre-compiled but can still be scaled at runtime
        /// </userdoc>
        [DataMember(50)]
        [NotNull]
        [Display(null, "Font")]
        public SpriteFontTypeBase FontType { get; set; } = new OfflineRasterizedSpriteFontType();

        /// <summary>
        /// Gets or sets the fallback character used when asked to render a character that is not
        /// included in the font. If zero, missing characters throw exceptions.
        /// </summary>
        /// <userdoc>
        /// The fallback character to use when a given character is not available in the font file data.
        /// </userdoc>
        [DataMember(60)]
        [DefaultValue(' ')]
        [Display(null, "Font")]
        public char DefaultCharacter { get; set; } = ' ';

        /// <summary>
        /// Gets or sets the extra character spacing in pixels (relative to the font size). Zero is default spacing, negative closer together, positive further apart
        /// </summary>
        ///  <userdoc>
        /// The extra spacing to add between characters in pixels. Zero is default spacing, negative closer together, positive further apart.
        /// </userdoc>
        [DataMember(130)]
        [DefaultValue(0.0f)]
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
        [DefaultValue(0.0f)]
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
        public float LineGapFactor { get; set; } = 1.0f;

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
        public float LineGapBaseLineFactor { get; set; } = 1.0f;

        class PremultiplyUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.NoPremultiply != null)
                {
                    asset.IsPremultiplied = !(bool)asset.NoPremultiply;
                    asset.NoPremultiply = DynamicYamlEmpty.Default;
                }
                if (asset.IsNotPremultiply != null)
                {
                    asset.IsPremultiplied = !(bool)asset.IsNotPremultiply;
                    asset.IsNotPremultiply = DynamicYamlEmpty.Default;
                }
            }
        }
    }
}
