// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
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
    [Display(1055, "Texture")]
    [CategoryOrder(10, "Size")]
    [CategoryOrder(20, "Format")]
    [AssetFormatVersion(XenkoConfig.PackageName, TextureAssetVersion)]
    [AssetUpgrader(XenkoConfig.PackageName, 0, 1, typeof(TransformSRgbToColorSpace))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.1", "1.4.0-beta", typeof(EmptyAssetUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.4.0-beta", "1.10.0-alpha01", typeof(DescriptionUpgrader))]
    public sealed class TextureAsset : AssetWithSource
    {
        private const string TextureAssetVersion = "1.10.0-alpha01";

        /// <summary>
        /// The default file extension used by the <see cref="TextureAsset"/>.
        /// </summary>
        public const string FileExtension = ".xktex;.pdxtex";

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        /// <userdoc>
        /// The width of the texture in-game. Depending on the value of the IsSizeInPercentage property, the value might represent either percent (%) or actual pixel.
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(100.0f)]
        [DataMemberRange(0, 10000, 1, 10)]
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
        [DataMemberRange(0, 10000, 1, 10)]
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
        /// Gets or sets the texture format.
        /// </summary>
        /// <value>The texture format.</value>
        /// <userdoc>
        /// The format to use for the texture. If Compressed, the final texture size must be a multiple of 4.
        /// </userdoc>
        [DataMember(50)]
        [DefaultValue(TextureFormat.Compressed)]
        [Display(null, "Format")]
        public TextureFormat Format { get; set; } = TextureFormat.Compressed;

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

        private class TransformSRgbToColorSpace : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                // Code was removed intentionally. Backward compatibility before 1.4.0-beta is no longer supported
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
