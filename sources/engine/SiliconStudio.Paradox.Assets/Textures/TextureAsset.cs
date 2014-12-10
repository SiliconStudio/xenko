// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Assets.Textures
{
    /// <summary>
    /// Describes a texture asset.
    /// </summary>
    [DataContract("Texture")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(TextureAssetCompiler))]
    [ThumbnailCompiler(PreviewerCompilerNames.TextureThumbnailCompilerQualifiedName)]
    [AssetDescription("Texture", "A texture", true)]
    public sealed class TextureAsset : AssetImport
    {
        /// <summary>
        /// The default file extension used by the <see cref="TextureAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxtex";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureAsset"/> class.
        /// </summary>
        public TextureAsset()
        {
            SetDefaults();
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        /// <userdoc>
        /// The width of the texture in-game. Depending on the value of the IsSizeInPercentage property, the value might represent either percent (%) or actual pixel.
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(100.0f)]
        [StepRange(0, 10000, 1, 10)]
        public float Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        /// <userdoc>
        /// The height of the texture in-game. Depending on the value of the IsSizeInPercentage property, the value might represent either percent (%) or actual pixel.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(100.0f)]
        [StepRange(0, 10000, 1, 10)]
        public float Height { get; set; }

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
        public bool IsSizeInPercentage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable color key. Default is false.
        /// </summary>
        /// <value><c>true</c> to enable color key; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, all pixels of the color set in the ColorKeyColor property will be replaced by transparent black.
        /// </userdoc>
        [DataMember(43)]
        [DefaultValue(false)]
        public bool ColorKeyEnabled { get; set; }

        /// <summary>
        /// Gets or sets the color key used when color keying for a texture is enabled. When color keying, all pixels of a specified color are replaced with transparent black.
        /// </summary>
        /// <value>The color key.</value>
        /// <userdoc>
        /// If ColorKeyEnabled is true, All pixels of the color set to this property are replaced with transparent black.
        /// </userdoc>
        [DataMember(45)]
        public Color ColorKeyColor { get; set; }

        /// <summary>
        /// Gets or sets the texture format.
        /// </summary>
        /// <value>The texture format.</value>
        /// <userdoc>
        /// The format to use for the texture. If Compressed, the final texture size must be a multiple of 4.
        /// </userdoc>
        [DataMember(50)]
        [DefaultValue(TextureFormat.Compressed)]
        public TextureFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the alpha format.
        /// </summary>
        /// <value>The alpha format.</value>
        /// <userdoc>
        /// The format to use for alpha in the texture.
        /// </userdoc>
        [DataMember(55)]
        [DefaultValue(AlphaFormat.None)]
        public AlphaFormat Alpha { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate mipmaps.
        /// </summary>
        /// <value><c>true</c> if mipmaps are generated; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, Mipmaps will be pre-generated for this texture.
        /// </userdoc>
        [DataMember(60)]
        [DefaultValue(true)]
        public bool GenerateMipmaps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to convert the texture in premultiply alpha.
        /// </summary>
        /// <value><c>true</c> to convert the texture in premultiply alpha.; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, The color values will be pre-multiplied by the alpha value.
        /// </userdoc>
        [DataMember(70)]
        [DefaultValue(true)]
        public bool PremultiplyAlpha { get; set; }

        public override void SetDefaults()
        {
            Width = 100.0f;
            Height = 100.0f;
            Format = TextureFormat.Compressed;
            Alpha = AlphaFormat.None;
            ColorKeyColor = new Color(255, 0, 255);
            ColorKeyEnabled = false;
            IsSizeInPercentage = true;
            GenerateMipmaps = true;
            PremultiplyAlpha = true;
        }
    }
}