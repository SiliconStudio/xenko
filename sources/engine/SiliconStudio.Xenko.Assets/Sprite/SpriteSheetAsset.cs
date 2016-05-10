// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Assets.Textures;

namespace SiliconStudio.Xenko.Assets.Sprite
{
    /// <summary>
    /// This asset represents a sheet (group) of sprites.
    /// </summary>
    [DataContract("SpriteSheet")]
    [CategoryOrder(10, "Parameters")]
    [CategoryOrder(50, "Atlas Packing")]
    [CategoryOrder(150, "Sprites")]
    [AssetFormatVersion(XenkoConfig.PackageName, "1.5.0-alpha01")]
    [AssetUpgrader(XenkoConfig.PackageName, 0, 1, typeof(RenameImageGroupsUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 1, 2, typeof(RemoveMaxSizeUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.2", "1.5.0-alpha01", typeof(BorderSizeOrderUpgrader))]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(SpriteSheetAssetCompiler))]
    [Display(160, "Sprite Sheet")]
    public class SpriteSheetAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="SpriteSheetAsset"/>.
        /// </summary>
        public const string FileExtension = ".xksheet;.pdxsheet;.pdxsprite;.pdxuiimage";
        
        /// <summary>
        /// Gets or sets the type of the current sheet
        /// </summary>
        /// <userdoc>
        /// The type of the sprite sheet.
        /// </userdoc>
        [DataMember(10)]
        [Display("Sheet Type", category: "Parameters")]
        public SpriteSheetType Type { get; set; }

        /// <summary>
        /// Gets or sets the color key used when color keying for a texture is enabled. When color keying, all pixels of a specified color are replaced with transparent black.
        /// </summary>
        /// <value>The color key.</value>
        /// <userdoc>
        /// The color that should be made transparent in all images of the group.
        /// </userdoc>
        [DataMember(20)]
        [Display(category: "Parameters")]
        public Color ColorKeyColor { get; set; } = new Color(255, 0, 255);

        /// <summary>
        /// Gets or sets a value indicating whether to enable color key. Default is false.
        /// </summary>
        /// <value><c>true</c> to enable color key; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, the color specified by 'ColorKeyColor' is made transparent in all images of the group during the asset build.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(false)]
        [Display(category: "Parameters")]
        public bool ColorKeyEnabled { get; set; }

        /// <summary>
        /// Gets or sets the texture format.
        /// </summary>
        /// <value>The texture format.</value>
        /// <userdoc>
        /// The texture format in which all the images of the group should be converted to.
        /// </userdoc>
        [DataMember(40)]
        [DefaultValue(TextureFormat.Compressed)]
        [Display(category: "Parameters")]
        public TextureFormat Format { get; set; } = TextureFormat.Compressed;

        /// <summary>
        /// Gets or sets the value indicating whether the output texture is encoded into the standard RGB color space.
        /// </summary>
        /// <userdoc>
        /// If checked, the input image is considered as an sRGB image. This should be default for colored texture
        /// with a HDR/gamma correct rendering.
        /// </userdoc>
        [DataMember(45)]
        [DefaultValue(TextureColorSpace.Auto)]
        [Display("ColorSpace", "Parameters")]
        public TextureColorSpace ColorSpace { get; set; } = TextureColorSpace.Auto;

        /// <summary>
        /// Gets or sets the alpha format.
        /// </summary>
        /// <value>The alpha format.</value>
        /// <userdoc>
        /// The texture alpha format in which all the images of the group should be converted to.
        /// </userdoc>
        [DataMember(50)]
        [DefaultValue(AlphaFormat.Auto)]
        [Display(category: "Parameters")]
        public AlphaFormat Alpha { get; set; } = AlphaFormat.Auto;

        /// <summary>
        /// Gets or sets a value indicating whether [generate mipmaps].
        /// </summary>
        /// <value><c>true</c> if [generate mipmaps]; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, mipmaps are generated for all the images of the group.
        /// </userdoc>
        [DataMember(60)]
        [DefaultValue(false)]
        [Display(category: "Parameters")]
        public bool GenerateMipmaps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to convert the texture in pre-multiply alpha.
        /// </summary>
        /// <value><c>true</c> to convert the texture in pre-multiply alpha.; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, pre-multiply all color components of the images by their alpha-component.
        /// Use this when elements are rendered with standard blending (and not transitive blending).
        /// </userdoc>
        [DataMember(70)]
        [DefaultValue(true)]
        [Display(category: "Parameters")]
        public bool PremultiplyAlpha { get; set; } = true;

        /// <summary>
        /// Gets or sets the sprites of the sheet.
        /// </summary>
        /// <userdoc>
        /// The parameters used to pack the sprites into atlas.
        /// </userdoc>
        [NotNull]
        [DataMember(100)]
        [Category("Atlas Packing")]
        public PackingAttributes Packing { get; set; } = new PackingAttributes();

        /// <summary>
        /// Gets or sets the sprites of the sheet.
        /// </summary>
        /// <userdoc>
        /// The list of sprites composing the sheet.
        /// </userdoc>
        [DataMember(150)]
        [Category]
        [NotNullItems]
        public List<SpriteInfo> Sprites { get; set; } = new List<SpriteInfo>();

        /// <summary>
        /// Retrieves Url for a texture given absolute path and sprite index
        /// </summary>
        /// <param name="textureAbsolutePath">Absolute Url of a texture</param>
        /// <param name="spriteIndex">Sprite index</param>
        public static string BuildTextureUrl(UFile textureAbsolutePath, int spriteIndex)
        {
            return textureAbsolutePath + "__IMAGE_TEXTURE__" + spriteIndex;
        }

        /// <summary>
        /// Retrieves Url for an atlas texture given absolute path and atlas index
        /// </summary>
        /// <param name="textureAbsolutePath">Absolute Url of an atlas texture</param>
        /// <param name="atlasIndex">Atlas index</param>
        public static string BuildTextureAtlasUrl(UFile textureAbsolutePath, int atlasIndex)
        {
            return textureAbsolutePath + "__ATLAS_TEXTURE__" + atlasIndex;
        }

        class RenameImageGroupsUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var images = asset.Images;
                if (images != null)
                {
                    asset.Sprites = images;
                    asset.Images = DynamicYamlEmpty.Default;
                }
            }
        }
        class RemoveMaxSizeUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var packing = asset.Packing;
                if (packing != null)
                {
                    packing.AtlasMaximumSize = DynamicYamlEmpty.Default;
                }
            }
        }
        class BorderSizeOrderUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                // SerializedVersion format changed during renaming upgrade. However, before this was merged back in master, some asset upgrader still with older version numbers were developed.
                // As a result, upgrade is not needed for version 3
                var sprites = asset.Sprites;
                if (sprites == null || currentVersion == PackageVersion.Parse("0.0.3"))
                    return;

                foreach (var sprite in asset.Sprites)
                {
                    if (sprite.Borders == null)
                    {
                        continue;
                    }
                    var y = sprite.Borders.Y ?? 0.0f;
                    sprite.Borders.Y = sprite.Borders.Z ?? 0.0f;
                    sprite.Borders.Z = y;
                }
            }
        }
    }
}
