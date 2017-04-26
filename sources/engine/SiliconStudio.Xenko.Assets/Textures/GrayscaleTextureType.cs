// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Textures
{
    /// <summary>
    /// A single channel texture which can be used for luminance, height map, specular texture, etc.
    /// </summary>
    /// <userdoc>
    /// A single channel texture which can be used for luminance, height map, specular texture, etc.
    /// </userdoc>
    [DataContract("GrayscaleTextureType")]
    [Display("Grayscale")]
    public class GrayscaleTextureType : ITextureType
    {
        public bool IsSRgb(ColorSpace colorSpaceReference) => false;

        bool ITextureType.ColorKeyEnabled => false;

        Color ITextureType.ColorKeyColor => new Color();

        AlphaFormat ITextureType.Alpha => AlphaFormat.None;

        bool ITextureType.PremultiplyAlpha => false;

        TextureHint ITextureType.Hint => TextureHint.Grayscale;
    }
}
