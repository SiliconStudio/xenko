using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.ProceduralModels;
using SiliconStudio.Xenko.Rendering.RenderTextures;

namespace SiliconStudio.Xenko.Assets.Textures
{
    [DataContract("RenderTexture")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(Texture))]
    [AssetCompiler(typeof(RenderTextureAssetCompiler))]
    [Display(1058, "Render Texture")]
    public sealed class RenderTextureAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="RenderTextureAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkrendertex";

        /// <summary>
        /// The width in pixel.
        /// </summary>
        [DefaultValue(512)]
        [DataMemberRange(0, 10000, 1, 10)]
        [Display(null, "Size")]
        public int Width { get; set; } = 512;

        /// <summary>
        /// The height in pixel.
        /// </summary>
        [DefaultValue(512)]
        [DataMemberRange(0, 10000, 1, 10)]
        [Display(null, "Size")]
        public int Height { get; set; } = 512;

        /// <summary>
        /// The format.
        /// </summary>
        [DefaultValue(RenderTextureFormat.LDR)]
        [Display("Format", "Format")]
        public RenderTextureFormat Format { get; set; } = RenderTextureFormat.LDR;

        /// <summary>
        /// Gets or sets the value indicating whether the output texture is encoded into the standard RGB color space.
        /// </summary>
        /// <userdoc>
        /// If checked, the input image is considered as an sRGB image. This should be default for colored texture
        /// with a HDR/gamma correct rendering.
        /// </userdoc>
        [DefaultValue(TextureColorSpace.Auto)]
        [Display("ColorSpace", "Format")]
        public TextureColorSpace ColorSpace { get; set; } = TextureColorSpace.Auto;
    }
}