// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL 
using System;
using System.Runtime.InteropServices;

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using RenderbufferStorage = OpenTK.Graphics.ES30.RenderbufferInternalFormat;
using PixelFormatGl = OpenTK.Graphics.ES30.PixelFormat;
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
using BufferUsageHint = OpenTK.Graphics.ES30.BufferUsage;
#endif
#if SILICONSTUDIO_PLATFORM_IOS
using ExtTextureFormatBgra8888 = OpenTK.Graphics.ES30.All;
using ImgTextureCompressionPvrtc = OpenTK.Graphics.ES30.All;
using OesPackedDepthStencil = OpenTK.Graphics.ES30.All;
#elif SILICONSTUDIO_PLATFORM_ANDROID
using ExtTextureFormatBgra8888 = OpenTK.Graphics.ES20.ExtTextureFormatBgra8888;
using OesCompressedEtc1Rgb8Texture = OpenTK.Graphics.ES20.OesCompressedEtc1Rgb8Texture;
#endif
#else
using System;
using OpenTK.Graphics.OpenGL;
using PixelFormatGl = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Abstract class for all textures
    /// </summary>
    public partial class Texture
    {
        internal SamplerState BoundSamplerState;

        public PixelInternalFormat InternalFormat { get; set; }
        public PixelFormatGl FormatGl { get; set; }
        public PixelType Type { get; set; }
        public TextureTarget Target { get; set; }
        public int DepthPitch { get; set; }
        public int RowPitch { get; set; }
        public bool IsDepthBuffer { get; private set; }
        public bool IsStencilBuffer { get; private set; }
        public bool IsRenderbuffer { get; private set; }
        internal int PixelBufferObjectId { get; set; }

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        public IntPtr StagingData { get; set; }
#endif

        public static bool IsDepthStencilReadOnlySupported(GraphicsDevice device)
        {
            // TODO: check that
            return true;
        }

        public void Recreate(DataBox[] dataBoxes = null)
        {
            InitializeFromImpl(dataBoxes);
        }

        private void OnRecreateImpl()
        {
            throw new NotImplementedException();
        }

        private void InitializeFromImpl(DataBox[] dataBoxes = null)
        {
            // TODO: how to use ParentTexture?
            // TODO: texture used as depth buffer should be a render buffer for optimization purposes
            if (ParentTexture != null)
            {
                resourceId = ParentTexture.ResourceId;
            }

            if (resourceId == 0)
            {
                switch (Dimension)
                {
                    case TextureDimension.Texture1D:
                        Target = TextureTarget.Texture1D;
                        break;
                    case TextureDimension.Texture2D:
                        Target = TextureTarget.Texture2D;
                        break;
                    case TextureDimension.Texture3D:
                        Target = TextureTarget.Texture3D;
                        break;
                    case TextureDimension.TextureCube:
                        Target = TextureTarget.TextureCubeMap;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
                DepthPitch = Description.Width * Description.Height * pixelSize;
                RowPitch = Description.Width * pixelSize;

                // TODO: review staging
                /*
                if (Description.Usage == GraphicsResourceUsage.Staging)
                {
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                    GL.GenBuffers(1, out resourceId);
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, resourceId);
                    GL.BufferData(BufferTarget.PixelPackBuffer, (IntPtr)DepthPitch, IntPtr.Zero,
                                    BufferUsageHint.StreamRead);
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
#else
                    StagingData = Marshal.AllocHGlobal(DepthPitch);
#endif
                }
                */

                // Depth texture are render buffer for now
                // TODO: enable switch
                if ((Description.Flags & TextureFlags.DepthStencil) != 0 && (Description.Flags & TextureFlags.ShaderResource) == 0)
                {
                    RenderbufferStorage depth, stencil;
                    ConvertDepthFormat(GraphicsDevice, Description.Format, out depth, out stencil);

                    GL.GenRenderbuffers(1, out resourceId);
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, resourceId);
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, depth, Width, Height);

                    // separate stencil
                    if (stencil != 0)
                    {
                        int resouceIdStencil;
                        GL.GenRenderbuffers(1, out resouceIdStencil);
                        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, resouceIdStencil);
                        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, stencil, Width, Height);
                    }

                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                    IsRenderbuffer = true;
                    IsDepthBuffer = true;
                    IsStencilBuffer = HasStencil(Format);
                }
                else
                {
                    GL.GenTextures(1, out resourceId);
                    GL.BindTexture(Target, resourceId);

                    IsRenderbuffer = false;
                    IsDepthBuffer = false;
                    IsStencilBuffer = false;
                }

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
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                else if (Description.MipLevels <= 1)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
#endif

#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
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
                    if (dataBoxes != null && i < dataBoxes.Length)
                    {
                        if (!compressed && dataBoxes[i].RowPitch != width * pixelSize)
                            throw new NotSupportedException("Can't upload texture with pitch in glTexImage2D.");
                        // Might be possible, need to check API better.
                        data = dataBoxes[i].DataPointer;
                    }
                    if (compressed)
                    {
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES && !SILICONSTUDIO_PLATFORM_MONO_MOBILE
                        throw new NotSupportedException("Can't use compressed textures on desktop OpenGL ES.");
#else
                        GL.CompressedTexImage2D(Target, i, internalFormat,
                            width, height, 0, dataBoxes[i].SlicePitch, data);
#endif
                    }
                    else
                    {
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES && !SILICONSTUDIO_PLATFORM_MONO_MOBILE
                        GL.TexImage2D(TextureTarget2d.Texture2D, i, internalFormat.ToOpenGL(),
                                        width, height, 0, format, type, data);
#else
                        GL.TexImage2D(Target, i, internalFormat,
                                        width, height, 0, format, type, data);
#endif
                    }
                }
                GL.BindTexture(Target, 0);

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                if (!GraphicsDevice.IsOpenGLES2)
#endif
                {
                    if (Description.Usage == GraphicsResourceUsage.Dynamic)
                        InitializePixelBufferObject();
                }
            }
        }

        /// <inheritdoc/>
        protected override void DestroyImpl()
        {
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            if (StagingData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StagingData);
                StagingData = IntPtr.Zero;
            }
#endif

            using (GraphicsDevice.UseOpenGLCreationContext())
            {
                if (Description.Usage == GraphicsResourceUsage.Staging)
                {
                    GL.DeleteBuffers(1, ref resourceId);
                }
                else
                {
                    GL.DeleteTextures(1, ref resourceId);
                }
            }

            resourceId = 0;

            base.DestroyImpl();
        }

        protected static void ConvertDepthFormat(GraphicsDevice graphicsDevice, PixelFormat requestedFormat, out RenderbufferStorage depthFormat, out RenderbufferStorage stencilFormat)
        {
            // Default: non-separate depth/stencil
            stencilFormat = 0;

            switch (requestedFormat)
            {
                case PixelFormat.D16_UNorm:
                    depthFormat = RenderbufferStorage.DepthComponent16;
                    break;
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                case PixelFormat.D24_UNorm_S8_UInt:
                    depthFormat = RenderbufferStorage.Depth24Stencil8;
                    break;
                case PixelFormat.D32_Float:
                    depthFormat = RenderbufferStorage.DepthComponent32;
                    break;
                case PixelFormat.D32_Float_S8X24_UInt:
                    depthFormat = RenderbufferStorage.Depth32fStencil8;
                    break;
#else
                case PixelFormat.D24_UNorm_S8_UInt:
                    if (graphicsDevice.HasPackedDepthStencilExtension)
                    {
                        depthFormat = RenderbufferStorage.Depth24Stencil8;
                    }
                    else
                    {
                        depthFormat = graphicsDevice.HasDepth24 ? RenderbufferStorage.DepthComponent24 : RenderbufferStorage.DepthComponent16;
                        stencilFormat = RenderbufferStorage.StencilIndex8;
                    }
                    break;
                case PixelFormat.D32_Float:
                case PixelFormat.D32_Float_S8X24_UInt:
                    throw new NotSupportedException("Only 16 bits depth buffer or 24-8 bits depth-stencil buffer is supported on OpenGLES2");
#endif
                default:
                    throw new NotImplementedException();
            }
        }

        private static bool HasStencil(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.D32_Float_S8X24_UInt:
                case PixelFormat.R32_Float_X8X24_Typeless:
                case PixelFormat.X32_Typeless_G8X24_UInt:
                case PixelFormat.D24_UNorm_S8_UInt:
                case PixelFormat.R24_UNorm_X8_Typeless:
                case PixelFormat.X24_Typeless_G8_UInt:
                    return true;
                default:
                    return false;
            }
        }

        protected static void ConvertPixelFormat(GraphicsDevice graphicsDevice, PixelFormat inputFormat, out PixelInternalFormat internalFormat, out PixelFormatGl format, out PixelType type, out int pixelSize, out bool compressed)
        {
            compressed = false;

            switch (inputFormat)
            {
                case PixelFormat.R8G8B8A8_UNorm:
                    internalFormat = PixelInternalFormat.Rgba;
                    format = PixelFormatGl.Rgba;
                    type = PixelType.UnsignedByte;
                    pixelSize = 4;
                    break;
                case PixelFormat.D16_UNorm:
                    internalFormat = PixelInternalFormat.DepthComponent16;
                    format = PixelFormatGl.DepthComponent;
                    type = PixelType.UnsignedShort;
                    pixelSize = 2;
                    break;
                case PixelFormat.A8_UNorm:
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                    internalFormat = PixelInternalFormat.Alpha;
                    format = PixelFormatGl.Alpha;
#else
                    internalFormat = PixelInternalFormat.R8;
                    format = PixelFormatGl.Red;
#endif
                    type = PixelType.UnsignedByte;
                    pixelSize = 1;
                    break;
                case PixelFormat.R8_UNorm:
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                    internalFormat = PixelInternalFormat.Luminance;
                    format = PixelFormatGl.Luminance;
#else
                    internalFormat = PixelInternalFormat.R8;
                    format = PixelFormatGl.Red;
#endif
                    type = PixelType.UnsignedByte;
                    pixelSize = 1;
                    break;
                case PixelFormat.B8G8R8A8_UNorm:
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                    if (!graphicsDevice.HasExtTextureFormatBGRA8888)
                        throw new NotSupportedException();

                    // It seems iOS and Android expects different things
#if SILICONSTUDIO_PLATFORM_IOS
                    internalFormat = PixelInternalFormat.Rgba;
#else
                    internalFormat = (PixelInternalFormat)ExtTextureFormatBgra8888.BgraExt;
#endif
                    format = (PixelFormatGl)ExtTextureFormatBgra8888.BgraExt;
#else
                    internalFormat = PixelInternalFormat.Rgba;
                    format = PixelFormatGl.Bgra;
#endif
                    type = PixelType.UnsignedByte;
                    pixelSize = 4;
                    break;
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                case PixelFormat.R32_UInt:
                    internalFormat = PixelInternalFormat.R32ui;
                    format = PixelFormatGl.RedInteger;
                    type = PixelType.UnsignedInt;
                    pixelSize = 4;
                    break;
                case PixelFormat.R16G16B16A16_Float:
                    internalFormat = PixelInternalFormat.Rgba16f;
                    format = PixelFormatGl.Rgba;
                    type = PixelType.HalfFloat;
                    pixelSize = 8;
                    break;
                case PixelFormat.R32_Float:
                    internalFormat = PixelInternalFormat.R32f;
                    format = PixelFormatGl.Red;
                    type = PixelType.Float;
                    pixelSize = 4;
                    break;
                case PixelFormat.R32G32_Float:
                    internalFormat = PixelInternalFormat.Rg32f;
                    format = PixelFormatGl.Rg;
                    type = PixelType.Float;
                    pixelSize = 8;
                    break;
                case PixelFormat.R32G32B32_Float:
                    internalFormat = PixelInternalFormat.Rgb32f;
                    format = PixelFormatGl.Rgb;
                    type = PixelType.Float;
                    pixelSize = 12;
                    break;
                case PixelFormat.R32G32B32A32_Float:
                    internalFormat = PixelInternalFormat.Rgba32f;
                    format = PixelFormatGl.Rgba;
                    type = PixelType.Float;
                    pixelSize = 16;
                    break;
                    // TODO: Temporary depth format (need to decide relation between RenderTarget1D and Texture)
                case PixelFormat.D32_Float:
                    internalFormat = PixelInternalFormat.DepthComponent32f;
                    format = PixelFormatGl.DepthComponent;
                    type = PixelType.Float;
                    pixelSize = 4;
                    break;
                case PixelFormat.D24_UNorm_S8_UInt:
                    internalFormat = PixelInternalFormat.DepthComponent24;
                    format = PixelFormatGl.DepthComponent;
                    type = PixelType.UnsignedInt248;
                    pixelSize = 4;
                    break;
#endif
#if SILICONSTUDIO_PLATFORM_ANDROID
                case PixelFormat.ETC1:
                    // TODO: Runtime check for extension?
                    internalFormat = (PixelInternalFormat)OesCompressedEtc1Rgb8Texture.Etc1Rgb8Oes;
                    format = (PixelFormatGl)OesCompressedEtc1Rgb8Texture.Etc1Rgb8Oes;
                    compressed = true;
                    pixelSize = 2;
                    type = PixelType.UnsignedByte;
                    break;
#elif SILICONSTUDIO_PLATFORM_IOS
                case PixelFormat.PVRTC_4bpp_RGB:
                    internalFormat = (PixelInternalFormat)ImgTextureCompressionPvrtc.CompressedRgbPvrtc4Bppv1Img;
                    format = (PixelFormatGl)ImgTextureCompressionPvrtc.CompressedRgbPvrtc4Bppv1Img;
                    compressed = true;
                    pixelSize = 4;
                    type = PixelType.UnsignedByte;
                    break;
                case PixelFormat.PVRTC_2bpp_RGB:
                    internalFormat = (PixelInternalFormat)ImgTextureCompressionPvrtc.CompressedRgbPvrtc2Bppv1Img;
                    format = (PixelFormatGl)ImgTextureCompressionPvrtc.CompressedRgbPvrtc2Bppv1Img;
                    compressed = true;
                    pixelSize = 2;
                    type = PixelType.UnsignedByte;
                    break;
                case PixelFormat.PVRTC_4bpp_RGBA:
                    internalFormat = (PixelInternalFormat)ImgTextureCompressionPvrtc.CompressedRgbaPvrtc4Bppv1Img;
                    format = (PixelFormatGl)ImgTextureCompressionPvrtc.CompressedRgbaPvrtc4Bppv1Img;
                    compressed = true;
                    pixelSize = 4;
                    type = PixelType.UnsignedByte;
                    break;
                case PixelFormat.PVRTC_2bpp_RGBA:
                    internalFormat = (PixelInternalFormat)ImgTextureCompressionPvrtc.CompressedRgbaPvrtc2Bppv1Img;
                    format = (PixelFormatGl)ImgTextureCompressionPvrtc.CompressedRgbaPvrtc2Bppv1Img;
                    compressed = true;
                    pixelSize = 2;
                    type = PixelType.UnsignedByte;
                    break;
#endif
                default:
                    throw new InvalidOperationException("Unsupported texture format");
            }
        }

        internal static PixelFormat ComputeShaderResourceFormatFromDepthFormat(PixelFormat format)
        {
            return format;
        }

        private bool IsFlippedTexture()
        {
            return GraphicsDevice.BackBuffer == this || GraphicsDevice.DepthStencilBuffer == this;
        }

        protected void InitializePixelBufferObject()
        {
            if (Description.Usage != GraphicsResourceUsage.Dynamic)
                throw new InvalidOperationException("Only Dynamic texture usage could initialize PBO");

            int bufferId;
            GL.GenBuffers(1, out bufferId);
            PixelBufferObjectId = bufferId;

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, PixelBufferObjectId);

            GL.BufferData(BufferTarget.PixelUnpackBuffer, (IntPtr)DepthPitch, IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
        }
    }
}
 
#endif 
