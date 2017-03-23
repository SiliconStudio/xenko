// Copyright (c) 2014-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Textures
{
    [CategoryOrder(40, "Transparency", Expand = ExpandRule.Never)]
    [DataContract("ColorTextureType")]
    [Display("Color")]
    public class ColorTextureType : ITextureType
    {
        /// <summary>
        /// Texture will be stored in sRGB format (standard for color textures) and converted to linear space when sampled. Only relevant when working in Linear color space.
        /// </summary>
        /// <userdoc>
        /// Should be checked for all color textures, unless they are explicitly in linear space. Texture will be stored in sRGB format (standard for color textures) and converted to linear space when sampled. Only relevant when working in Linear color space.
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(true)]
        [Display("sRGB sampling")]
        public bool UseSRgbSampling { get; set; } = true;

        public bool IsSRgb(ColorSpace colorSpaceReference) => ((colorSpaceReference == ColorSpace.Linear) && UseSRgbSampling);

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

        TextureHint ITextureType.Hint => TextureHint.Color;
    }
}
