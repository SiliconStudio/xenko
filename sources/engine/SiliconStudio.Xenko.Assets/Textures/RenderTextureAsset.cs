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
        /// Texture will be stored in sRGB format (standard for color textures) and converted to linear space when sampled. Only relevant when working in Linear color space.
        /// </summary>
        /// <userdoc>
        /// Should be checked for all color textures, unless they are explicitly in linear space. Texture will be stored in sRGB format (standard for color textures) and converted to linear space when sampled. Only relevant when working in Linear color space.
        /// </userdoc>
        [DefaultValue(true)]
        [Display("sRGB sampling")]
        public bool UseSRgbSampling { get; set; } = true;

        public bool IsSRgb(ColorSpace colorSpaceReference) => ((colorSpaceReference == ColorSpace.Linear) && UseSRgbSampling);
    }
}