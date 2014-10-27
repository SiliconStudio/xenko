using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Texture;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// Describes an image group asset.
    /// </summary>
    [DataContract("ImageGroupBase")]
    public abstract class ImageGroupAsset : Asset
    {
        protected ImageGroupAsset()
        {
            SetDefaults();
        }

        /// <summary>
        /// Gets or sets the color key used when color keying for a texture is enabled. When color keying, all pixels of a specified color are replaced with transparent black.
        /// </summary>
        /// <value>The color key.</value>
        [DataMember(20)]
        public Color ColorKeyColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable color key. Default is false.
        /// </summary>
        /// <value><c>true</c> to enable color key; otherwise, <c>false</c>.</value>
        [DataMember(30)]
        [DefaultValue(false)]
        public bool ColorKeyEnabled { get; set; }

        /// <summary>
        /// Gets or sets the texture format.
        /// </summary>
        /// <value>The texture format.</value>
        [DataMember(40)]
        [DefaultValue(TextureFormat.Compressed)]
        public TextureFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the alpha format.
        /// </summary>
        /// <value>The alpha format.</value>
        [DataMember(50)]
        [DefaultValue(AlphaFormat.Interpolated)]
        public AlphaFormat Alpha { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [generate mipmaps].
        /// </summary>
        /// <value><c>true</c> if [generate mipmaps]; otherwise, <c>false</c>.</value>
        [DataMember(60)]
        [DefaultValue(false)]
        public bool GenerateMipmaps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to convert the texture in pre-multiply alpha.
        /// </summary>
        /// <value><c>true</c> to convert the texture in pre-multiply alpha.; otherwise, <c>false</c>.</value>
        [DataMember(70)]
        [DefaultValue(true)]
        public bool PremultiplyAlpha { get; set; }

        [DataMember(80)]
        [DefaultValue(true)]
        public bool UseTextureAtlas { get; set; }

        public override void SetDefaults()
        {
            Format = TextureFormat.Compressed;
            Alpha = AlphaFormat.Interpolated;
            ColorKeyColor = new Color(255, 0, 255);
            ColorKeyEnabled = false;
            GenerateMipmaps = false;
            PremultiplyAlpha = true;
            UseTextureAtlas = true;
        }

        public static string BuildTextureUrl(UFile textureAbsolutePath, int spriteIndex)
        {
            return textureAbsolutePath + "__IMAGE_TEXTURE__" + spriteIndex;
        }

        public static string BuildTextureAtlasUrl(UFile textureAbsolutePath)
        {
            return textureAbsolutePath + "__ATLAS_IMAGE_GROUP";
        }
    }

    /// <summary>
    /// Describes an image group asset.
    /// </summary>
    [DataContract("ImageGroup")]
    public abstract class ImageGroupAsset<TImage> : ImageGroupAsset
    {
        protected ImageGroupAsset()
        {
            SetDefaults();
        }

        /// <summary>
        /// Gets or sets the sprites of the group.
        /// </summary>
        [DataMember(10)]
        public List<TImage> Images { get; set; }
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            Images = new List<TImage>();
        }
    }
}