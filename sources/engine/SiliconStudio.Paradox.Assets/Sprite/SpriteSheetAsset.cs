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
    /// This asset reprensents a sheet (group) of sprites.
    /// </summary>
    [DataContract("SpriteSheet")]
    [CategoryOrder(10, "Parameters")]
    [CategoryOrder(150, "Sprites")]
    [AssetFormatVersion(1)]
    [AssetUpgrader(0, 1, typeof(RenameImageGroupsUpgrader))]
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
        public const string FileExtension = ".pdxsheet";
        
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
        /// Gets or sets the alpha format.
        /// </summary>
        /// <value>The alpha format.</value>
        /// <userdoc>
        /// The texture alpha format in which all the images of the group should be converted to.
        /// </userdoc>
        [DataMember(50)]
        [DefaultValue(AlphaFormat.Interpolated)]
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
        /// The list of images of the sheet to import.
        /// </userdoc>
        [DataMember(150)]
        [Category]
        public List<SpriteInfo> Sprites { get; set; }

        public override void SetDefaults()
        {
            Sprites = new List<SpriteInfo>();
            Format = TextureFormat.Compressed;
            Alpha = AlphaFormat.Interpolated;
            ColorKeyColor = new Color(255, 0, 255);
            ColorKeyEnabled = false;
            GenerateMipmaps = false;
            PremultiplyAlpha = true;
        }

        public static string BuildTextureUrl(UFile textureAbsolutePath, int spriteIndex)
        {
            return textureAbsolutePath + "__IMAGE_TEXTURE__" + spriteIndex;
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
            protected override void UpgradeAsset(int currentVersion, int targetVersion, ILogger log, dynamic asset)
            {
                var images = asset.Images;
                if (images != null)
                {
                    asset.Sprites = images;
                    asset.Images = DynamicYamlEmpty.Default;
                }
            }
        }
    }
}