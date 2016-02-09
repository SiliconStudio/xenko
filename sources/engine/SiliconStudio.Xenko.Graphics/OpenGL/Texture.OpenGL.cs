// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Runtime.InteropServices;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
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
using OpenTK.Graphics.OpenGL;
using PixelFormatGl = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

// TODO: remove these when OpenTK API is consistent between OpenGL, mobile OpenGL ES and desktop OpenGL ES
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
using CompressedInternalFormat2D = OpenTK.Graphics.ES30.CompressedInternalFormat;
using CompressedInternalFormat3D = OpenTK.Graphics.ES30.CompressedInternalFormat;
using TextureComponentCount2D = OpenTK.Graphics.ES30.TextureComponentCount;
using TextureComponentCount3D = OpenTK.Graphics.ES30.TextureComponentCount;
#else
using CompressedInternalFormat2D = OpenTK.Graphics.ES30.PixelInternalFormat;
using CompressedInternalFormat3D = OpenTK.Graphics.ES30.CompressedInternalFormat;
using TextureComponentCount2D = OpenTK.Graphics.ES30.PixelInternalFormat;
using TextureComponentCount3D = OpenTK.Graphics.ES30.TextureComponentCount;
#endif
#else
using CompressedInternalFormat2D = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using CompressedInternalFormat3D = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using TextureComponentCount2D = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using TextureComponentCount3D = OpenTK.Graphics.OpenGL.PixelInternalFormat;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Abstract class for all textures
    /// </summary>
    public partial class Texture
    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES && SILICONSTUDIO_PLATFORM_MONO_MOBILE
        private const BufferUsageHint BufferUsageHintStreamRead = (BufferUsageHint)0x88E1;
#else
        private const BufferUsageHint BufferUsageHintStreamRead = BufferUsageHint.StreamRead;
#endif
        internal const TextureFlags TextureFlagsCustomResourceId = (TextureFlags)0x1000;

        internal SamplerState BoundSamplerState;
        private int pixelBufferObjectId;
        private int resourceIdStencil;

        internal PixelInternalFormat InternalFormat { get; set; }
        internal PixelFormatGl FormatGl { get; set; }
        internal PixelType Type { get; set; }
        internal TextureTarget Target { get; set; }
        internal int DepthPitch { get; set; }
        internal int RowPitch { get; set; }
        internal bool IsDepthBuffer { get; private set; }
        internal bool HasStencil { get; private set; }
        internal bool IsRenderbuffer { get; private set; }
        
        internal int PixelBufferObjectId
        {
            get { return pixelBufferObjectId; }
        }

        internal int ResourceIdStencil
        {
            get { return resourceIdStencil; }
        }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        public IntPtr StagingData { get; set; }
#endif

        public static bool IsDepthStencilReadOnlySupported(GraphicsDevice device)
        {
            // always true on OpenGL
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
            if (ParentTexture != null)
            {
                resourceId = ParentTexture.ResourceId;

                // copy parameters
                InternalFormat = ParentTexture.InternalFormat;
                FormatGl = ParentTexture.FormatGl;
                Type = ParentTexture.Type;
                Target = ParentTexture.Target;
                DepthPitch = ParentTexture.DepthPitch;
                RowPitch = ParentTexture.RowPitch;
                IsDepthBuffer = ParentTexture.IsDepthBuffer;
                HasStencil = ParentTexture.HasStencil;
                IsRenderbuffer = ParentTexture.IsRenderbuffer;

                resourceIdStencil = ParentTexture.ResourceIdStencil;
                pixelBufferObjectId = ParentTexture.PixelBufferObjectId;
            }

            if (resourceId == 0)
            {
                switch (Dimension)
                {
                    case TextureDimension.Texture1D:
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
                        Target = TextureTarget.Texture1D;
                        break;
#endif
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
                OpenGLConvertExtensions.ConvertPixelFormat(GraphicsDevice, ref textureDescription.Format, out internalFormat, out format, out type, out pixelSize, out compressed);

                InternalFormat = internalFormat;
                FormatGl = format;
                Type = type;
                DepthPitch = Description.Width * Description.Height * pixelSize;
                RowPitch = Description.Width * pixelSize;

                if ((Description.Flags & TextureFlags.DepthStencil) != 0)
                {
                    IsDepthBuffer = true;
                    HasStencil = InternalHasStencil(Format);
                }
                else
                {
                    IsDepthBuffer = false;
                    HasStencil = false;
                }

                if ((Description.Flags & TextureFlagsCustomResourceId) != 0)
                    return;

                using (GraphicsDevice.UseOpenGLCreationContext())
                {
                    // Depth texture are render buffer for now
                    // TODO: enable switch
                    if ((Description.Flags & TextureFlags.DepthStencil) != 0 && (Description.Flags & TextureFlags.ShaderResource) == 0)
                    {
                        RenderbufferStorage depth, stencil;
                        ConvertDepthFormat(GraphicsDevice, Description.Format, out depth, out stencil);

                        GL.GenRenderbuffers(1, out resourceId);
                        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, resourceId);
                        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, depth, Width, Height);

                        if (stencil != 0)
                        {
                            // separate stencil
                            GL.GenRenderbuffers(1, out resourceIdStencil);
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, resourceIdStencil);
                            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, stencil, Width, Height);
                        }
                        else if (HasStencil)
                        {
                            // depth+stencil in a single texture
                            resourceIdStencil = resourceId;
                        }

                        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

                        IsRenderbuffer = true;
                        return;
                    }
                    else
                    {
                        GL.GenTextures(1, out resourceId);
                        GL.BindTexture(Target, resourceId);

                        IsRenderbuffer = false;
                    }

                    // No filtering on depth buffer
                    if ((Description.Flags & (TextureFlags.RenderTarget | TextureFlags.DepthStencil)) != TextureFlags.None)
                    {
                        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        BoundSamplerState = GraphicsDevice.SamplerStates.PointClamp;
                    }
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    else if (Description.MipLevels <= 1)
                    {
                        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    }
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (!GraphicsDevice.IsOpenGLES2)
#endif
                    {
                        GL.TexParameter(Target, TextureParameterName.TextureBaseLevel, 0);
                        GL.TexParameter(Target, TextureParameterName.TextureMaxLevel, Description.MipLevels - 1);
                    }

                    if (Description.MipLevels == 0)
                        throw new NotImplementedException();

                    var setSize = TextureSetSize(Target);

                    for (var arrayIndex = 0; arrayIndex < Description.ArraySize; ++arrayIndex)
                    {
                        var offsetArray = arrayIndex*Description.MipLevels;
                        for (int i = 0; i < Description.MipLevels; ++i)
                        {
                            IntPtr data = IntPtr.Zero;
                            var width = CalculateMipSize(Description.Width, i);
                            var height = CalculateMipSize(Description.Height, i);
                            if (dataBoxes != null && i < dataBoxes.Length)
                            {
                                if (setSize > 1 && !compressed && dataBoxes[i].RowPitch != width*pixelSize)
                                    throw new NotSupportedException("Can't upload texture with pitch in glTexImage2D.");
                                // Might be possible, need to check API better.
                                data = dataBoxes[offsetArray + i].DataPointer;
                            }

                            if (setSize == 2)
                            {
                                var dataSetTarget = GetTextureTargetForDataSet2D(Target, arrayIndex);
                                if (compressed)
                                {
                                    GL.CompressedTexImage2D(dataSetTarget, i, (CompressedInternalFormat2D)internalFormat,
                                        width, height, 0, dataBoxes[offsetArray + i].SlicePitch, data);
                                }
                                else
                                {
                                    GL.TexImage2D(dataSetTarget, i, internalFormat, width, height, 0, format, type, data);
                                }
                            }
                            else if (setSize == 3)
                            {
                                var dataSetTarget = GetTextureTargetForDataSet3D(Target);
                                var depth = Target == TextureTarget.Texture2DArray ? Description.Depth : CalculateMipSize(Description.Depth, i); // no depth mipmaps in Texture2DArray
                                if (compressed)
                                {
                                    GL.CompressedTexImage3D(dataSetTarget, i, (CompressedInternalFormat3D)internalFormat,
                                        width, height, depth, 0, dataBoxes[offsetArray + i].SlicePitch, data);
                                }
                                else
                                {
                                    GL.TexImage3D(dataSetTarget, i, (TextureComponentCount3D)internalFormat,
                                        width, height, depth, 0, format, type, data);
                                }
                            }
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                            else if (setSize == 1)
                            {
                                if (compressed)
                                {
                                    GL.CompressedTexImage1D(TextureTarget.Texture1D, i, internalFormat,
                                        width, 0, dataBoxes[offsetArray + i].SlicePitch, data);
                                }
                                else
                                {
                                    GL.TexImage1D(TextureTarget.Texture1D, i, internalFormat,
                                        width, 0, format, type, data);
                                }
                            }
#endif
                        }
                    }
                    GL.BindTexture(Target, 0);

                    InitializePixelBufferObject();
                }

                GraphicsDevice.TextureMemory += (Depth * DepthStride) / (float)0x100000;
            }
        }

        /// <inheritdoc/>
        protected override void DestroyImpl()
        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (StagingData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StagingData);
                StagingData = IntPtr.Zero;
            }
#endif

            using (GraphicsDevice.UseOpenGLCreationContext())
            {
                if (resourceId != 0)
                {
                    if (IsRenderbuffer)
                        GL.DeleteRenderbuffers(1, ref resourceId);
                    else
                        GL.DeleteTextures(1, ref resourceId);

                    GraphicsDevice.TextureMemory -= (Depth * DepthStride) / (float)0x100000;
                }

                if (resourceIdStencil != 0)
                    GL.DeleteRenderbuffers(1, ref resourceIdStencil);

                if (pixelBufferObjectId != 0)
                    GL.DeleteBuffers(1, ref pixelBufferObjectId);
            }

            resourceId = 0;
            resourceIdStencil = 0;
            pixelBufferObjectId = 0;

            base.DestroyImpl();
        }

        private static void ConvertDepthFormat(GraphicsDevice graphicsDevice, PixelFormat requestedFormat, out RenderbufferStorage depthFormat, out RenderbufferStorage stencilFormat)
        {
            // Default: non-separate depth/stencil
            stencilFormat = 0;

            switch (requestedFormat)
            {
                case PixelFormat.D16_UNorm:
                    depthFormat = RenderbufferStorage.DepthComponent16;
                    break;
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
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
                    if (graphicsDevice.IsOpenGLES2)
                        throw new NotSupportedException("Only 16 bits depth buffer or 24-8 bits depth-stencil buffer is supported on OpenGLES2");
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
                    depthFormat = RenderbufferInternalFormat.DepthComponent32F;
#else
                    depthFormat = RenderbufferInternalFormat.DepthComponent32f;
#endif
                    break;
                case PixelFormat.D32_Float_S8X24_UInt:
                    if (graphicsDevice.IsOpenGLES2)
                        throw new NotSupportedException("Only 16 bits depth buffer or 24-8 bits depth-stencil buffer is supported on OpenGLES2");
                    // no need to check graphicsDevice.HasPackedDepthStencilExtension since supported 32F depth means OpenGL ES 3, so packing is available.
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
                    depthFormat = RenderbufferInternalFormat.Depth32FStencil8;
#else
                    depthFormat = RenderbufferInternalFormat.Depth32fStencil8;
#endif
                    break;
#endif
                default:
                    throw new NotImplementedException();
            }
        }

        private static bool InternalHasStencil(PixelFormat format)
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

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES && !SILICONSTUDIO_PLATFORM_MONO_MOBILE
        private static TextureTarget2d GetTextureTargetForDataSet2D(TextureTarget target, int arrayIndex)
        {
            // TODO: Proxy from ES 3.1?
            if (target == TextureTarget.TextureCubeMap)
                return TextureTarget2d.TextureCubeMapPositiveX + arrayIndex;
            return (TextureTarget2d)target;
        }

        private static TextureTarget3d GetTextureTargetForDataSet3D(TextureTarget target)
        {
            return (TextureTarget3d)target;
        }
#else
        private static TextureTarget GetTextureTargetForDataSet2D(TextureTarget target, int arrayIndex)
        {
            // TODO: Proxy from ES 3.1?
            if (target == TextureTarget.TextureCubeMap)
                return TextureTarget.TextureCubeMapPositiveX + arrayIndex;
            return target;
        }
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private static TextureTarget3D GetTextureTargetForDataSet3D(TextureTarget target)
        {
            return (TextureTarget3D)target;
        }
#else
        private static TextureTarget GetTextureTargetForDataSet3D(TextureTarget target)
        {
            return target;
        }
#endif
#endif

        private static int TextureSetSize(TextureTarget target)
        {
            // TODO: improve that
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (target == TextureTarget.Texture1D)
                return 1;
#endif
            if (target == TextureTarget.Texture3D || target == TextureTarget.Texture2DArray)
                return 3;
            return 2;
        }

        internal void InternalSetSize(int width, int height)
        {
            // Set backbuffer actual size
            textureDescription.Width = width;
            textureDescription.Height = height;
        }

        internal static PixelFormat ComputeShaderResourceFormatFromDepthFormat(PixelFormat format)
        {
            return format;
        }

        private bool IsFlipped()
        {
            return GraphicsDevice.WindowProvidedRenderTexture == this;
        }

        private void InitializePixelBufferObject()
        {
            if (Description.Usage == GraphicsResourceUsage.Staging)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (GraphicsDevice.IsOpenGLES2)
                {
                    StagingData = Marshal.AllocHGlobal(DepthPitch);
                }
                else
#endif
                {
                    GeneratePixelBufferObject(BufferTarget.PixelPackBuffer, BufferUsageHintStreamRead); // enum not available on some platforms
                }
            }
            else if (Description.Usage == GraphicsResourceUsage.Dynamic)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                // unable to create PBO on OpenGL ES 2 but we do not throw an exception. It will be thrown if the code tries performs writes on the texture.
                if (!GraphicsDevice.IsOpenGLES2)
#endif
                {
                    GeneratePixelBufferObject(BufferTarget.PixelUnpackBuffer, BufferUsageHint.DynamicDraw);
                }
            }
        }

        private void GeneratePixelBufferObject(BufferTarget target, BufferUsageHint bufferUsage)
        {
            GL.GenBuffers(1, out pixelBufferObjectId);

            GL.BindBuffer(target, pixelBufferObjectId);
            if (RowPitch < 4)
                GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.BufferData(target, (IntPtr)DepthPitch, IntPtr.Zero, bufferUsage);
            GL.BindBuffer(target, 0);
        }
    }
}

#endif
