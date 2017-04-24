// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using PixelFormatGl = OpenTK.Graphics.ES30.PixelFormat;
using RenderbufferStorage = OpenTK.Graphics.ES30.RenderbufferInternalFormat;
#else
using OpenTK.Graphics.OpenGL;
using PixelFormatGl = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Represents a 2D grid of texels.
    /// </summary>
    public partial class Texture2DBase
    {
        // For depth stencil buffer, we might need to separate depth from stencil storage, so we will have one additional resource id.
        internal int ResouceIdStencil;

        internal bool IsRenderbuffer
        {
            get
            {
                return (Description.Flags & TextureFlags.ShaderResource) == 0
                       && (Description.Flags & TextureFlags.DepthStencil) == TextureFlags.DepthStencil;
            }
        }

        protected internal Texture2DBase(GraphicsDevice device, TextureDescription description2D, TextureTarget textureTarget, DataBox[] dataBoxes = null, bool initialize = true) : base(device, description2D, ViewType.Full, 0, 0)
        {
            Target = textureTarget;
            if (initialize)
                Init(dataBoxes);
        }

        protected internal Texture2DBase(GraphicsDevice device, Texture2DBase texture) : base(device, texture, ViewType.Full, 0, 0)
        {
            this.Target = texture.Target;
            this.resourceId = texture.ResourceId;
        }


        /// <summary>
        /// Inits this instance with the specified texture datas.
        /// </summary>
        /// <param name="textureDatas">The texture datas.</param>
        protected virtual void Init(DataBox[] textureDatas)
        {
            if (Target != TextureTarget.Texture2D && Target != TextureTarget.TextureCubeMap)
                throw new Exception("incorrect type of Texture2D, it should be TextureTarget.Texture2D or TextureTarget.TextureCubeMap");

            using (var creationContext = GraphicsDevice.UseOpenGLCreationContext())
            {
                // Can we just do a renderbuffer?
                if ((Description.Flags & TextureFlags.ShaderResource) == 0)
                {
                    if ((Description.Flags & TextureFlags.DepthStencil) == TextureFlags.DepthStencil)
                    {
                        RenderbufferStorage depth, stencil;
                        ConvertDepthFormat(GraphicsDevice, Description.Format, out depth, out stencil);

                        // Create depth render buffer (might contain stencil data too)
                        GL.GenRenderbuffers(1, out resourceId);
                        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, resourceId);
                        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, depth, Description.Width, Description.Height);
                        if (OpenGLConvertExtensions.GetErrorCode() != ErrorCode.NoError)
                            throw new InvalidOperationException("Could not create render buffer");

                        // If stencil buffer is separate, create it as well
                        if (stencil != 0)
                        {
                            GL.GenRenderbuffers(1, out ResouceIdStencil);
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, ResouceIdStencil);
                            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, stencil, Description.Width, Description.Height);
                            if (OpenGLConvertExtensions.GetErrorCode() != ErrorCode.NoError)
                                throw new InvalidOperationException("Could not create render buffer");
                        }

                        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                        return;
                    }
                }

                PixelInternalFormat internalFormat;
                PixelFormatGl format;
                PixelType type;
                int pixelSize;
                bool compressed;
                ConvertPixelFormat(GraphicsDevice, Description.Format, out internalFormat, out format, out type, out pixelSize, out compressed);

                InternalFormat = internalFormat;
                FormatGl = format;
                Type = type;
                DepthPitch = Description.Width*Description.Height*pixelSize;
                RowPitch = Description.Width*pixelSize;

                if (Description.Usage == GraphicsResourceUsage.Staging)
                {
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    GL.GenBuffers(1, out resourceId);
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, resourceId);
                    GL.BufferData(BufferTarget.PixelPackBuffer, (IntPtr)DepthPitch, IntPtr.Zero,
                                    BufferUsageHint.StreamRead);
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
#else
                    StagingData = Marshal.AllocHGlobal(DepthPitch);
#endif
                }
                else
                {
                    int textureId;

                    // If we're on main context, change internal states so that texture is bound again
                    if (!creationContext.UseDeviceCreationContext)
                        GraphicsDevice.UseTemporaryFirstTexture();

                    GL.GenTextures(1, out textureId);
                    GL.BindTexture(TextureTarget.Texture2D, textureId);

                    // No filtering on depth buffer
                    if ((Description.Flags & (TextureFlags.RenderTarget | TextureFlags.DepthStencil)) !=
                        TextureFlags.None)
                    {
                        GL.TexParameter(Target, TextureParameterName.TextureMinFilter,
                                        (int)TextureMinFilter.Nearest);
                        GL.TexParameter(Target, TextureParameterName.TextureMagFilter,
                                        (int)TextureMagFilter.Nearest);
                        GL.TexParameter(Target, TextureParameterName.TextureWrapS,
                                        (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(Target, TextureParameterName.TextureWrapT,
                                        (int)TextureWrapMode.ClampToEdge);
                    }
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    else if (Description.MipLevels <= 1)
                    {
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    }
#endif

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    GL.TexParameter(Target, TextureParameterName.TextureBaseLevel, 0);
                    GL.TexParameter(Target, TextureParameterName.TextureMaxLevel, Description.MipLevels - 1);
#endif

                    if (Description.MipLevels == 0)
                        throw new NotImplementedException();

                    for (int i = 0; i < Description.MipLevels; ++i)
                    {
                        IntPtr data = IntPtr.Zero;
                        var width = CalculateMipSize(Description.Width, i);
                        var height = CalculateMipSize(Description.Height, i);
                        if (textureDatas != null && i < textureDatas.Length)
                        {
                            if (!compressed && textureDatas[i].RowPitch != width * pixelSize)
                                throw new NotSupportedException("Can't upload texture with pitch in glTexImage2D.");
                                    // Might be possible, need to check API better.
                            data = textureDatas[i].DataPointer;
                        }
                        if (compressed)
                        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES && !SILICONSTUDIO_PLATFORM_MONO_MOBILE
                            throw new NotSupportedException("Can't use compressed textures on desktop OpenGL ES.");
#else
                            GL.CompressedTexImage2D(Target, i, internalFormat,
                                width, height, 0, textureDatas[i].SlicePitch, data);
#endif
                        }
                        else
                        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES && !SILICONSTUDIO_PLATFORM_MONO_MOBILE
                            GL.TexImage2D(TextureTarget2d.Texture2D, i, internalFormat.ToOpenGL(),
                                            width, height, 0, format, type, data);
#else
                            GL.TexImage2D(Target, i, internalFormat,
                                            width, height, 0, format, type, data);
#endif
                        }
                    }
                    GL.BindTexture(Target, 0);

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
        }

        public override void Recreate(DataBox[] dataBoxes = null)
        {
            Init(dataBoxes);
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            // Dependency: wait for underlying texture to be recreated
            if (ParentTexture != null && ParentTexture.LifetimeState != GraphicsResourceLifetimeState.Active)
                return false;

            if (ParentTexture != null)
            {
                // TODO: Test
                throw new NotImplementedException();

                resourceId = ParentTexture.ResourceId;
            }
            else
            {
                // Render Target / Depth Stencil are considered as "dynamic"
                if ((Description.Usage == GraphicsResourceUsage.Immutable
                     || Description.Usage == GraphicsResourceUsage.Default)
                    && (Description.Flags & (TextureFlags.RenderTarget | TextureFlags.DepthStencil)) == 0)
                    return false;

                Init(null);
            }

            return true;
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
            return (int)Math.Ceiling(Math.Log(Math.Max(Description.Width, Description.Height)) / Math.Log(2.0));
        }
    }
}
 
#endif 
