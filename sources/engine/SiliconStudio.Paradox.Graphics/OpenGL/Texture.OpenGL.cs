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
        private RenderTarget cachedRenderTarget;

        internal SamplerState BoundSamplerState;

        public PixelInternalFormat InternalFormat { get; set; }
        public PixelFormatGl FormatGl { get; set; }
        public PixelType Type { get; set; }
        public TextureTarget Target { get; set; }
        public int DepthPitch { get; set; }
        public int RowPitch { get; set; }
        internal int PixelBufferObjectId { get; set; }

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        public IntPtr StagingData { get; set; }
#endif

        public virtual void Recreate(DataBox[] dataBoxes = null)
        {
            throw new NotImplementedException();
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

        public RenderTarget ToRenderTarget(ViewType viewType, int arraySlize, int mipSlice)
        {
            return new RenderTarget(GraphicsDevice, this, ViewType.Single, 0, 0);
        }

        internal RenderTarget GetCachedRenderTarget()
        {
            if (cachedRenderTarget == null)
            {
                cachedRenderTarget = new RenderTarget(GraphicsDevice, this, ViewType.Single, 0, 0);
            }

            return cachedRenderTarget;
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

        private bool IsFlippedTexture()
        {
            return GraphicsDevice.BackBuffer.Texture == this || GraphicsDevice.DepthStencilBuffer.Texture == this;
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
