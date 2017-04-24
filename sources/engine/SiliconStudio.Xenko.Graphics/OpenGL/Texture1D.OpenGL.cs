// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
using System;
using System.IO;

using OpenTK.Graphics;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using PixelFormatGl = OpenTK.Graphics.ES30.PixelFormat;
#else
using OpenTK.Graphics.OpenGL;
using PixelFormatGl = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Represents a 1D grid of texels.
    /// </summary>
    public partial class Texture1D
    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private const TextureTarget TextureTarget1D = TextureTarget.Texture2D;
#else
        private const TextureTarget TextureTarget1D = TextureTarget.Texture1D;
#endif

        protected internal Texture1D(GraphicsDevice device, TextureDescription description1D, DataBox[] dataBox = null)
            : base(device, description1D, ViewType.Full, 0, 0)
        {
            Target = TextureTarget1D;
        }
        
        protected internal Texture1D(GraphicsDevice device, Texture1D texture) : base(device, texture, ViewType.Full, 0, 0)
        {
            Target = TextureTarget1D;
        }

        public override Texture ToTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            // Exists since OpenGL 4.3
            if (viewType != ViewType.Full || arraySlice != 0 || mipMapSlice != 0)
                throw new NotImplementedException();

            return new Texture1D(GraphicsDevice, this);
        }

        /// <summary>
        /// Inits this instance with the specified texture datas.
        /// </summary>
        /// <param name="textureDatas">The texture datas.</param>
        protected virtual void Init(DataRectangle[] textureDatas)
        {
            using (var creationContext = GraphicsDevice.UseOpenGLCreationContext())
            {
                PixelInternalFormat internalFormat;
                PixelFormatGl format;
                PixelType type;
                int pixelSize;
                bool compressed;
                ConvertPixelFormat(GraphicsDevice, Description.Format, out internalFormat, out format, out type, out pixelSize, out compressed);

                InternalFormat = internalFormat;
                FormatGl = format;
                Type = type;

                int textureId;

                // If we're on main context, change internal states so that texture is bound again
                if (!creationContext.UseDeviceCreationContext)
                    GraphicsDevice.UseTemporaryFirstTexture();

                GL.GenTextures(1, out textureId);
                GL.BindTexture(TextureTarget1D, textureId);

                // No filtering on depth buffer
                if (format == PixelFormatGl.DepthComponent)
                {
                    GL.TexParameter(TextureTarget1D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget1D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget1D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget1D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                }

                for (int i = 0; i < Description.MipLevels; ++i)
                {
                    IntPtr data = IntPtr.Zero;
                    var width = CalculateMipSize(Description.Width, i);
                    if (textureDatas != null && i < textureDatas.Length)
                    {
                        if (textureDatas[i].Pitch != width * pixelSize)
                            throw new NotSupportedException("Can't upload texture with pitch in glTexImage1D."); // Might be possible, need to check API better.
                        data = textureDatas[i].DataPointer;
                    }
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
                    GL.TexImage2D(TextureTarget1D, i, internalFormat, width, 1, 0, format, type, data);
#else
                    GL.TexImage2D(TextureTarget2d.Texture2D, i, internalFormat.ToOpenGL(), width, 1, 0, format, type, data);
#endif
#else
                    GL.TexImage1D(TextureTarget1D, i, internalFormat, width, 0, format, type, data);
#endif
                }
                GL.BindTexture(TextureTarget1D, 0);

                resourceId = textureId;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (!GraphicsDevice.IsOpenGLES2)
#endif
                {
                    if (Description.Usage == GraphicsResourceUsage.Dynamic)
                        InitializePixelBufferObject();
                }
            }
        }

        /// <summary>
        /// Computes the mip level count.
        /// </summary>
        /// <param name="mipLevels">The mip levels.</param>
        /// <returns></returns>
        protected int ComputeLevelCount(int mipLevels)
        {
            if (mipLevels > 0)
                return mipLevels;
            return (int)Math.Ceiling(Math.Log(Description.Width) / Math.Log(2.0));
        }
    }
}
 
#endif 
