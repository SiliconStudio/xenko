// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Textures
{
    /// <summary>
    /// Describes a texture asset.
    /// </summary>
    [DataContract("Texture")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(Texture))]
    [CategoryOrder(10, "Size")]
    [CategoryOrder(20, "Format")]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "1.4.0-beta")]
    [AssetUpgrader(XenkoConfig.PackageName, "1.4.0-beta", "1.10.0-alpha01", typeof(DescriptionUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.10.0-alpha01", "1.11.1.2", typeof(CompressionUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.11.1.2", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public sealed partial class TextureAsset : AssetWithSource
    {
        private const string CurrentVersion = "2.0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="TextureAsset"/>.
        /// </summary>
        public const string FileExtension = ".xktex";

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        /// <userdoc>
        /// The width of the texture in-game. Depending on the value of the IsSizeInPercentage property, the value might represent either percent (%) or actual pixel.
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(100.0f)]
        [DataMemberRange(0, 100, 1, 10, 1)]
        [Display(null, "Size")]
        public float Width { get; set; } = 100.0f;

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        /// <userdoc>
        /// The height of the texture in-game. Depending on the value of the IsSizeInPercentage property, the value might represent either percent (%) or actual pixel.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(100.0f)]
        [DataMemberRange(0, 100, 1, 10, 1)]
        [Display(null, "Size")]
        public float Height { get; set; } = 100.0f;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using size in percentage. Default is true. See remarks.
        /// </summary>
        /// <value><c>true</c> if this instance is dimension absolute; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// When this property is true (by default), <see cref="Width"/> and <see cref="Height"/> are epxressed 
        /// in percentage, with 100.0f being 100% of the current size, and 50.0f half of the current size, otherwise
        /// the size is in absolute pixels.
        /// </remarks>
        /// <userdoc>
        /// If checked, the values of the Width and Height properties will represent percent (%). Otherwise they would represent actual pixel.
        /// </userdoc>
        [DataMember(40)]
        [DefaultValue(true)]
        [Display(null, "Size")]
        public bool IsSizeInPercentage { get; set; } = true;

        /// <summary>
        /// If Compressed, the final texture will be compressed to an appropriate format based on the target platform. The final texture size must be a multiple of 4.
        /// </summary>
        /// <userdoc>
        /// If Compressed, the final texture will be compressed to an appropriate format based on the target platform. The final texture size must be a multiple of 4.
        /// </userdoc>
        [DataMember(50)]
        [DefaultValue(true)]
        [Display("Compress")]
        public bool IsCompressed { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate mipmaps.
        /// </summary>
        /// <value><c>true</c> if mipmaps are generated; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, Mipmaps will be pre-generated for this texture.
        /// </userdoc>
        [DataMember(70)]
        [DefaultValue(true)]
        [Display(null, "Format")]
        public bool GenerateMipmaps { get; set; } = true;

        /// <summary>
        /// The description of the data contained in the texture. See remarks.
        /// </summary>
        /// <remarks>This description helps the texture compressor to select the appropriate format based on the HW Level and 
        /// platform.</remarks>
        /// <userdoc>A hint to indicate the usage/type of texture. This hint helps the texture compressor to select the 
        /// appropriate format based on the HW Level and platform.</userdoc>
        [DataMember(60)]
        [NotNull]
        [Display(null, "Format", Expand = ExpandRule.Always)]
        public ITextureType Type { get; set; } = new ColorTextureType();

        private class CompressionUpgrader : AssetUpgraderBase
        {
            // public TextureFormat Format { get; set; } = TextureFormat.Compressed;
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.ContainsChild("Format"))
                {
                    if (asset.Format == "Compressed")
                    {
                        asset.IsCompressed = true;
                    }
                    else
                    {
                        asset.IsCompressed = false;
                    }

                    asset.RemoveChild("Format");
                }
                else
                {
                    asset.IsCompressed = true;
                }
            }
        }

        private class DescriptionUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.Hint == "NormalMap")
                {
                    dynamic textureType = asset.Type = new DynamicYamlMapping(new YamlMappingNode());
                    textureType.Node.Tag = "!NormalMapTextureType";
                }
                else if (asset.Hint == "Grayscale")
                {
                    dynamic textureType = asset.Type = new DynamicYamlMapping(new YamlMappingNode());
                    textureType.Node.Tag = "!GrayscaleTextureType";
                }
                else
                {
                    dynamic textureType = asset.Type = new DynamicYamlMapping(new YamlMappingNode());
                    textureType.Node.Tag = "!ColorTextureType";

                    if (asset.ContainsChild("ColorSpace"))
                        textureType.UseSRgbSampling = (asset.ColorSpace != "Gamma"); // This is correct. It converts some legacy code with ambiguous meaning.
                    if (asset.ContainsChild("ColorKeyEnabled"))
                        textureType.ColorKeyEnabled = asset.ColorKeyEnabled;
                    if (asset.ContainsChild("ColorKeyColor"))
                        textureType.ColorKeyColor = asset.ColorKeyColor;
                    if (asset.ContainsChild("Alpha"))
                        textureType.Alpha = asset.Alpha;
                    if (asset.ContainsChild("PremultiplyAlpha"))
                        textureType.PremultiplyAlpha = asset.PremultiplyAlpha;
                }

                asset.RemoveChild("ColorSpace");
                asset.RemoveChild("ColorKeyEnabled");
                asset.RemoveChild("ColorKeyColor");
                asset.RemoveChild("Alpha");
                asset.RemoveChild("PremultiplyAlpha");
                asset.RemoveChild("Hint");
            }
        }
    }
}
