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
    [AssetCompiler(typeof(TextureAssetCompiler))]
    [Display(1055, "Texture")]
    [CategoryOrder(10, "Size")]
    [CategoryOrder(20, "Format")]
    [CategoryOrder(30, "Transparency")]
    [AssetFormatVersion(XenkoConfig.PackageName, TextureAssetVersion)]
    [AssetUpgrader(XenkoConfig.PackageName, 0, 1, typeof(TransformSRgbToColorSpace))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.1", "1.4.0-beta", typeof(EmptyAssetUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.4.0-beta", "1.10.0-alpha01", typeof(DescriptionUpgrader))]
    public sealed class TextureAsset : AssetWithSource, IAssetCompileTimeDependencies
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
        public ITextureDescription Description { get; set; } = new ColorTextureDescription();

        public IEnumerable<IReference> EnumerateCompileTimeDependencies(PackageSession session)
        {
            var gameSettings = session.CurrentPackage?.Assets.Find(GameSettingsAsset.GameSettingsLocation);
            if (gameSettings != null)
            {
                yield return new AssetReference(gameSettings.Id, gameSettings.Location);
            }
        }

        private class TransformSRgbToColorSpace : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.SRgb != null)
                {
                    // By default transform to auto
                    asset.ColorSpace = TextureColorSpace.Auto;
                    asset.RemoveChild("SRgb");
                }
            }
        }

        private class DescriptionUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.Hint == "NormalMap")
                {
                    dynamic description = asset.Description = new DynamicYamlMapping(new YamlMappingNode());
                    description.Node.Tag = "!NormalMapDescription";
                }
                else if (asset.Hint == "Grayscale")
                {
                    dynamic description = asset.Description = new DynamicYamlMapping(new YamlMappingNode());
                    description.Node.Tag = "!GrayscaleTextureDescription";
                }
                else
                {
                    dynamic description = asset.Description = new DynamicYamlMapping(new YamlMappingNode());
                    description.Node.Tag = "!ColorTextureDescription";
                    description.ColorSpace = asset.ColorSpace;
                    description.ColorKeyEnabled = asset.ColorKeyEnabled;
                    description.ColorKeyColor = asset.ColorKeyColor;
                    description.Alpha = asset.Alpha;
                    description.PremultiplyAlpha = asset.PremultiplyAlpha;
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

    public interface ITextureDescription
    {
        TextureColorSpace ColorSpace { get; }

        bool ColorKeyEnabled { get; }

        Color ColorKeyColor { get; }

        AlphaFormat Alpha { get; }

        bool PremultiplyAlpha { get; }

        TextureHint Hint { get; }
    }

    [DataContract("NormalMapDescription")]
    [Display("Normal Map")]
    public class NormapMapDescription : ITextureDescription
    {
        /// <summary>
        /// Indicating whether the Y-component of normals should be inverted, to compensate for a flipped tangent-space.
        /// </summary>
        /// <userdoc>
        /// Indicates that a positive Y-component (green) faces up in tangent space. This options depends on your normal maps generation tools.
        /// </userdoc>
        [DataMember(10)]
        [DefaultValue(true)]
        public bool InvertY { get; set; } = true;

        TextureColorSpace ITextureDescription.ColorSpace => TextureColorSpace.Gamma;

        bool ITextureDescription.ColorKeyEnabled => false;

        Color ITextureDescription.ColorKeyColor => new Color();

        AlphaFormat ITextureDescription.Alpha => AlphaFormat.None;

        bool ITextureDescription.PremultiplyAlpha => false;

        TextureHint ITextureDescription.Hint => TextureHint.NormalMap;
    }

    /// <summary>
    /// A single channel texture which can be used for luminance, height map, specular texture, etc.
    /// </summary>
    /// <userdoc>
    /// A single channel texture which can be used for luminance, height map, specular texture, etc.
    /// </userdoc>
    [DataContract("GrayscaleTextureDescription")]
    [Display("Grayscale")]
    public class GrayscaleTextureDescription : ITextureDescription
    {
        TextureColorSpace ITextureDescription.ColorSpace => TextureColorSpace.Gamma;

        bool ITextureDescription.ColorKeyEnabled => false;

        Color ITextureDescription.ColorKeyColor => new Color();

        AlphaFormat ITextureDescription.Alpha => AlphaFormat.None;

        bool ITextureDescription.PremultiplyAlpha => false;

        TextureHint ITextureDescription.Hint => TextureHint.Grayscale;
    }


    [DataContract("ColorTextureDescription")]
    [Display("Color")]
    public class ColorTextureDescription : ITextureDescription
    {
        /// <summary>
        /// Gets or sets the value indicating whether the output texture is encoded into the standard RGB color space.
        /// </summary>
        /// <userdoc>
        /// If checked, the input image is considered as an sRGB image. This should be default for colored texture
        /// with a HDR/gamma correct rendering.
        /// </userdoc>
        [DataMember(70)]
        [DefaultValue(TextureColorSpace.Auto)]
        [Display("ColorSpace")]
        public TextureColorSpace ColorSpace { get; set; } = TextureColorSpace.Auto;

        /// <summary>
        /// Gets or sets a value indicating whether to enable color key. Default is false.
        /// </summary>
        /// <value><c>true</c> to enable color key; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, all pixels of the color set in the ColorKeyColor property will be replaced by transparent black.
        /// </userdoc>
        [DataMember(43)]
        [DefaultValue(false)]
        [Display(null, "Transparency")]
        public bool ColorKeyEnabled { get; set; }

        /// <summary>
        /// Gets or sets the color key used when color keying for a texture is enabled. When color keying, all pixels of a specified color are replaced with transparent black.
        /// </summary>
        /// <value>The color key.</value>
        /// <userdoc>
        /// If ColorKeyEnabled is true, All pixels of the color set to this property are replaced with transparent black.
        /// </userdoc>
        [DataMember(45)]
        [Display(null, "Transparency")]
        public Color ColorKeyColor { get; set; } = new Color(255, 0, 255);

        /// <summary>
        /// Gets or sets the alpha format.
        /// </summary>
        /// <value>The alpha format.</value>
        /// <userdoc>
        /// The format to use for alpha in the texture.
        /// </userdoc>
        [DataMember(55)]
        [DefaultValue(AlphaFormat.Auto)]
        [Display(null, "Transparency")]
        public AlphaFormat Alpha { get; set; } = AlphaFormat.Auto;

        /// <summary>
        /// Gets or sets a value indicating whether to convert the texture in premultiply alpha.
        /// </summary>
        /// <value><c>true</c> to convert the texture in premultiply alpha.; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, The color values will be pre-multiplied by the alpha value.
        /// </userdoc>
        [DataMember(80)]
        [DefaultValue(true)]
        [Display(null, "Transparency")]
        public bool PremultiplyAlpha { get; set; } = true;

        TextureHint ITextureDescription.Hint => TextureHint.Color;
    }
}
