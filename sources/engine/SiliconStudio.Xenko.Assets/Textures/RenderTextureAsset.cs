// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.RenderTextures;

namespace SiliconStudio.Xenko.Assets.Textures
{
    [DataContract("RenderTexture")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(Texture))]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "0.0.0")]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public sealed partial class RenderTextureAsset : Asset
    {
        private const string CurrentVersion = "2.0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="RenderTextureAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkrendertex";

        /// <summary>
        /// The width in pixel.
        /// </summary>
        [DefaultValue(512)]
        [Display(null, "Size")]
        public int Width { get; set; } = 512;

        /// <summary>
        /// The height in pixel.
        /// </summary>
        [DefaultValue(512)]
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
