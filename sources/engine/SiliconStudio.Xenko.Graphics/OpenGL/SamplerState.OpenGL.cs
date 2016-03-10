// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
using System;
using SiliconStudio.Core.Mathematics;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using TextureCompareMode = OpenTK.Graphics.ES30.All;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    public partial class SamplerState
    {
        private TextureWrapMode textureWrapS;
        private TextureWrapMode textureWrapT;
        private TextureWrapMode textureWrapR;

        private TextureMinFilter minFilter;
        private TextureMagFilter magFilter;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private TextureMinFilter minFilterNoMipmap;
#endif

        private float[] borderColor;

        private DepthFunction compareFunc;
        private TextureCompareMode compareMode;

        private SamplerState(GraphicsDevice device, SamplerStateDescription samplerStateDescription) : base(device)
        {
            Description = samplerStateDescription;

            textureWrapS = samplerStateDescription.AddressU.ToOpenGL();
            textureWrapT = samplerStateDescription.AddressV.ToOpenGL();
            textureWrapR = samplerStateDescription.AddressW.ToOpenGL();

            compareMode = TextureCompareMode.None;

            // ComparisonPoint can act as a mask for Comparison filters (0x80)
            if ((samplerStateDescription.Filter & TextureFilter.ComparisonPoint) != 0)
                compareMode = TextureCompareMode.CompareRefToTexture;

            compareFunc = samplerStateDescription.CompareFunction.ToOpenGLDepthFunction();
            borderColor = samplerStateDescription.BorderColor.ToArray();
            // TODO: How to do MipLinear vs MipPoint?
            switch (samplerStateDescription.Filter)
            {
                case TextureFilter.ComparisonMinMagLinearMipPoint:
                case TextureFilter.MinMagLinearMipPoint:
                    minFilter = TextureMinFilter.Linear;
                    magFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.Anisotropic:
                case TextureFilter.Linear:
                    minFilter = TextureMinFilter.LinearMipmapLinear;
                    magFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.MinPointMagMipLinear:
                case TextureFilter.ComparisonMinPointMagMipLinear:
                    minFilter = TextureMinFilter.NearestMipmapLinear;
                    magFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.Point:
                    minFilter = TextureMinFilter.Nearest;
                    magFilter = TextureMagFilter.Nearest;
                    break;
                default:
                    throw new NotImplementedException();
            }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
    // On OpenGL ES, we need to choose the appropriate min filter ourself if the texture doesn't contain mipmaps (done at PreDraw)
            minFilterNoMipmap = minFilter;
            if (minFilterNoMipmap == TextureMinFilter.LinearMipmapLinear)
                minFilterNoMipmap = TextureMinFilter.Linear;
            else if (minFilterNoMipmap == TextureMinFilter.NearestMipmapLinear)
                minFilterNoMipmap = TextureMinFilter.Nearest;
#endif
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            base.OnRecreate();
            return true;
        }

        internal void Apply(bool hasMipmap, SamplerState oldSamplerState, TextureTarget target)
        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
    // TODO: support texture array, 3d and cube
            if (!GraphicsDevice.IsOpenGLES2)
#endif
            {
                if (Description.MinMipLevel != oldSamplerState.Description.MinMipLevel)
                    GL.TexParameter(target, TextureParameterName.TextureMinLod, Description.MinMipLevel);
                if (Description.MaxMipLevel != oldSamplerState.Description.MaxMipLevel)
                    GL.TexParameter(target, TextureParameterName.TextureMaxLod, Description.MaxMipLevel);
                if (textureWrapR != oldSamplerState.textureWrapR)
                    GL.TexParameter(target, TextureParameterName.TextureWrapR, (int)textureWrapR);
                if (compareMode != oldSamplerState.compareMode)
                    GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int)compareMode);
                if (compareFunc != oldSamplerState.compareFunc)
                    GL.TexParameter(target, TextureParameterName.TextureCompareFunc, (int)compareFunc);
            }

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (borderColor != oldSamplerState.borderColor)
                GL.TexParameter(target, TextureParameterName.TextureBorderColor, borderColor);
            if (Description.MipMapLevelOfDetailBias != oldSamplerState.Description.MipMapLevelOfDetailBias)
                GL.TexParameter(target, TextureParameterName.TextureLodBias, Description.MipMapLevelOfDetailBias);
            if (minFilter != oldSamplerState.minFilter)
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)minFilter);
#else
    // On OpenGL ES, we need to choose the appropriate min filter ourself if the texture doesn't contain mipmaps (done at PreDraw)
            if (minFilter != oldSamplerState.minFilter)
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, hasMipmap ? (int)minFilter : (int)minFilterNoMipmap);
#endif

#if !SILICONSTUDIO_PLATFORM_IOS
            if (Description.MaxAnisotropy != oldSamplerState.Description.MaxAnisotropy)
                GL.TexParameter(target, (TextureParameterName)OpenTK.Graphics.ES20.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, Description.MaxAnisotropy);
#endif
            if (magFilter != oldSamplerState.magFilter)
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)magFilter);
            if (textureWrapS != oldSamplerState.textureWrapS)
                GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)textureWrapS);
            if (textureWrapT != oldSamplerState.textureWrapT)
                GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)textureWrapT);
        }
    }
}

#endif 
