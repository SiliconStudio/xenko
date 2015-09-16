// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.Assets.Textures;

namespace SiliconStudio.Paradox.Assets.Sprite
{
    /// <summary>
    /// This asset represents a sheet (group) of sprites.
    /// </summary>
    [DataContract("SpriteSheet")]
    [CategoryOrder(10, "Parameters")]
    [CategoryOrder(50, "Atlas Packing")]
    [CategoryOrder(150, "Sprites")]
    [AssetFormatVersion(2)]
    [AssetUpgrader(0, 1, typeof(RenameImageGroupsUpgrader))]
    [AssetUpgrader(1, 2, typeof(RemoveMaxSizeUpgrader))]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(SpriteSheetAssetCompiler))]
    [ObjectFactory(typeof(SpriteSheetFactory))]
    [ThumbnailCompiler(PreviewerCompilerNames.SpriteSheetThumbnailCompilerQualifiedName, true)]
    [Display(160, "Sprite Sheet", "A sheet of sprites")]
    public class SpriteSheetAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="SpriteSheetAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxsheet;.pdxsprite;.pdxuiimage";
        
        /// <summary>
        /// Create an empty sprite sheet asset.
        /// </summary>
        public SpriteSheetAsset()
        {
            SetDefaults();
        }

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
        public Color ColorKeyColor { get; set; }

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
        public TextureFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the output texture is encoded into the standard RGB color space.
        /// </summary>
        /// <userdoc>
        /// If checked, the input image is considered as an sRGB image. This should be default for colored texture
        /// with a HDR/gamma correct rendering.
        /// </userdoc>
        [DataMember(45)]
        [DefaultValue(TextureColorSpace.Auto)]
        [Display("ColorSpace", null, "Parameters")]
        public TextureColorSpace ColorSpace { get; set; }

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
        public AlphaFormat Alpha { get; set; }

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
        public bool PremultiplyAlpha { get; set; }

        /// <summary>
        /// Gets or sets the sprites of the sheet.
        /// </summary>
        /// <userdoc>
        /// The parameters used to pack the sprites into atlas.
        /// </userdoc>
        [NotNull]
        [DataMember(100)]
        [Category("Atlas Packing")]
        public PackingAttributes Packing { get; set; }

        /// <summary>
        /// Gets or sets the sprites of the sheet.
        /// </summary>
        /// <userdoc>
        /// The list of sprites composing the sheet.
        /// </userdoc>
        [DataMember(150)]
        [Category]
        public List<SpriteInfo> Sprites { get; set; }
        
        /// <summary>
        /// Sets default value of SpriteSheetAsset.
        /// </summary>
        public override void SetDefaults()
        {
            Sprites = new List<SpriteInfo>();
            Format = TextureFormat.Compressed;
            ColorSpace = TextureColorSpace.Auto;
            Alpha = AlphaFormat.Auto;
            ColorKeyColor = new Color(255, 0, 255);
            ColorKeyEnabled = false;
            GenerateMipmaps = false;
            PremultiplyAlpha = true;
            Packing = new PackingAttributes();
        }

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

        private class SpriteSheetFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new SpriteSheetAsset();
            }
        }

        class RenameImageGroupsUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, int currentVersion, int targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
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
            protected override void UpgradeAsset(AssetMigrationContext context, int currentVersion, int targetVersion, dynamic asset, PackageLoadingAssetFile assetFile)
            {
                var packing = asset.Packing;
                if (packing != null)
                {
                    packing.AtlasMaximumSize = DynamicYamlEmpty.Default;
                }
            }
        }
    }
}