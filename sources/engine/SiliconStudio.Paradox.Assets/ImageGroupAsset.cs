using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Texture;
using SiliconStudio.Paradox.Graphics;

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

        /// <summary>
        /// Gets or Sets a value indicating whether to generate texture atlas
        /// </summary>
        [DataMember(80)]
        [DefaultValue(true)]
        public bool GenerateTextureAtlas { get; set; }

        /// <summary>
        /// Gets or Sets MaxRects rectangles placement algorithm
        /// </summary>
        [DataMember(90)]
        [DefaultValue(MaxRectanglesBinPack.HeuristicMethod.BestShortSideFit)]
        public MaxRectanglesBinPack.HeuristicMethod AtlasPackingAlgorithm { get; set; }

        /// <summary>
        /// Gets or Sets the use of Multipack atlas mode which allows more than one texture atlas to fit all given textures
        /// </summary>
        [DataMember(100)]
        [DefaultValue(false)]
        public bool UseMultipackAtlas { get; set; }

        /// <summary>
        /// Gets or Sets atlas border mode for images inside atlas texture
        /// </summary>
        [DataMember(110)]
        public TextureAddressMode AtlasBorderMode { get; set; }

        /// <summary>
        /// Gets or Sets atlas border size for images inside atlas texture
        /// </summary>
        [DataMember(120)]
        [DefaultValue(0)]
        public int AtlasBorderSize { get; set; }

        /// <summary>
        /// Gets or Sets atlas border color for images inside atlas texture where Border mode is used in AtlasBorderMode
        /// </summary>
        [DataMember(130)]
        public Color AtlasBorderColor { get; set; }

        /// <summary>
        /// Gets or Sets whether or not to use Rotation for images inside atlas texture
        /// </summary>
        [DataMember(140)]
        [DefaultValue(true)]
        public bool UseRotationInAtlas { get; set; }

        /// <summary>
        /// Gets or Sets max width for generated atlas textures
        /// </summary>
        [DataMember(150)]
        public int AtlasMaxWidth { get; set; }

        /// <summary>
        /// Gets or Sets max height for generated atlas textures
        /// </summary>
        [DataMember(160)]
        public int AtlasMaxHeight { get; set; }

        /// <summary>
        /// Sets default values for fields
        /// </summary>
        public override void SetDefaults()
        {
            Format = TextureFormat.Compressed;
            Alpha = AlphaFormat.Interpolated;
            ColorKeyColor = new Color(255, 0, 255);
            ColorKeyEnabled = false;
            GenerateMipmaps = false;
            PremultiplyAlpha = true;

            GenerateTextureAtlas = true;
            AtlasPackingAlgorithm = MaxRectanglesBinPack.HeuristicMethod.BestShortSideFit;
            UseMultipackAtlas = false;

            AtlasBorderMode = TextureAddressMode.Border;
            AtlasBorderSize = 0;
            AtlasBorderColor = Color.Transparent;
            UseRotationInAtlas = true;

            AtlasMaxWidth = 1024;
            AtlasMaxHeight = 1024;
        }

        /// <summary>
        /// Retrieves Url for a texture given absolute path and sprite index
        /// </summary>
        /// <param name="textureAbsolutePath">Absolute Url of a texture</param>
        /// <param name="spriteIndex">Sprite index</param>
        /// <returns></returns>
        public static string BuildTextureUrl(UFile textureAbsolutePath, int spriteIndex)
        {
            return textureAbsolutePath + "__IMAGE_TEXTURE__" + spriteIndex;
        }

        /// <summary>
        /// Retrieves Url for an atlas texture given absolute path and atlas index
        /// </summary>
        /// <param name="textureAbsolutePath">Absolute Url of an atlas texture</param>
        /// <param name="atlasIndex">Atlas index</param>
        /// <returns></returns>
        public static string BuildTextureAtlasUrl(UFile textureAbsolutePath, int atlasIndex)
        {
            return textureAbsolutePath + "__ATLAS_TEXTURE__" + atlasIndex;
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
        
        /// <summary>
        /// Sets default value of ImageGroupAsset
        /// </summary>
        public override void SetDefaults()
        {
            base.SetDefaults();
            Images = new List<TImage>();
        }
    }
}