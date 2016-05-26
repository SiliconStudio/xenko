// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Runtime.InteropServices;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using RenderbufferStorage = OpenTK.Graphics.ES30.RenderbufferInternalFormat;
using PixelFormatGl = OpenTK.Graphics.ES30.PixelFormat;
using PixelInternalFormat = OpenTK.Graphics.ES30.TextureComponentCount;
#else
using OpenTK.Graphics.OpenGL;
using PixelFormatGl = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

// TODO: remove these when OpenTK API is consistent between OpenGL, mobile OpenGL ES and desktop OpenGL ES
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using CompressedInternalFormat2D = OpenTK.Graphics.ES30.CompressedInternalFormat;
using CompressedInternalFormat3D = OpenTK.Graphics.ES30.CompressedInternalFormat;
using TextureComponentCount2D = OpenTK.Graphics.ES30.TextureComponentCount;
using TextureComponentCount3D = OpenTK.Graphics.ES30.TextureComponentCount;
#else
using CompressedInternalFormat2D = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using CompressedInternalFormat3D = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using TextureComponentCount2D = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using TextureComponentCount3D = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using TextureTarget2d = OpenTK.Graphics.OpenGL.TextureTarget;
using TextureTarget3d = OpenTK.Graphics.OpenGL.TextureTarget;
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
        internal int PixelBufferFrame;
        private int pixelBufferObjectId;
        private int stencilId;

        internal int DepthPitch { get; set; }
        internal int RowPitch { get; set; }
        internal bool IsDepthBuffer { get; private set; }
        internal bool HasStencil { get; private set; }
        internal bool IsRenderbuffer { get; private set; }
        
        internal int PixelBufferObjectId
        {
            get { return pixelBufferObjectId; }
        }

        internal int StencilId
        {
            get { return stencilId; }
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
            // Dependency: wait for underlying texture to be recreated
            if (ParentTexture != null && ParentTexture.LifetimeState != GraphicsResourceLifetimeState.Active)
                return;

            // Render Target / Depth Stencil are considered as "dynamic"
            if ((Usage == GraphicsResourceUsage.Immutable
                    || Usage == GraphicsResourceUsage.Default)
                && !IsRenderTarget && !IsDepthStencil)
                return;

            if (ParentTexture == null && GraphicsDevice != null)
            {
                GraphicsDevice.TextureMemory -= (Depth * DepthStride) / (float)0x100000;
            }

            InitializeFromImpl();
        }

        private void InitializeFromImpl(DataBox[] dataBoxes = null)
        {
            if (ParentTexture != null)
            {
                TextureId = ParentTexture.TextureId;

                // copy parameters
                TextureInternalFormat = ParentTexture.TextureInternalFormat;
                TextureFormat = ParentTexture.TextureFormat;
                TextureType = ParentTexture.TextureType;
                TextureTarget = ParentTexture.TextureTarget;
                DepthPitch = ParentTexture.DepthPitch;
                RowPitch = ParentTexture.RowPitch;
                IsDepthBuffer = ParentTexture.IsDepthBuffer;
                HasStencil = ParentTexture.HasStencil;
                IsRenderbuffer = ParentTexture.IsRenderbuffer;

                stencilId = ParentTexture.StencilId;
                pixelBufferObjectId = ParentTexture.PixelBufferObjectId;
            }

            if (TextureId == 0)
            {
                switch (Dimension)
                {
                    case TextureDimension.Texture1D:
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
                        TextureTarget = TextureTarget.Texture1D;
                        break;
#endif
                    case TextureDimension.Texture2D:
                        TextureTarget = TextureTarget.Texture2D;
                        break;
                    case TextureDimension.Texture3D:
                        TextureTarget = TextureTarget.Texture3D;
                        break;
                    case TextureDimension.TextureCube:
                        TextureTarget = TextureTarget.TextureCubeMap;
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

                TextureInternalFormat = internalFormat;
                TextureFormat = format;
                TextureType = type;
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

                using (var openglContext = GraphicsDevice.UseOpenGLCreationContext())
                {
                    // Depth texture are render buffer for now
                    // TODO: enable switch
                    if ((Description.Flags & TextureFlags.DepthStencil) != 0 && (Description.Flags & TextureFlags.ShaderResource) == 0)
                    {
                        RenderbufferStorage depth, stencil;
                        ConvertDepthFormat(GraphicsDevice, Description.Format, out depth, out stencil);

                        GL.GenRenderbuffers(1, out TextureId);
                        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, TextureId);
                        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, depth, Width, Height);

                        if (stencil != 0)
                        {
                            // separate stencil
                            GL.GenRenderbuffers(1, out stencilId);
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, stencilId);
                            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, stencil, Width, Height);
                        }
                        else if (HasStencil)
                        {
                            // depth+stencil in a single texture
                            stencilId = TextureId;
                        }

                        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

                        IsRenderbuffer = true;
                        return;
                    }

                    GL.GenTextures(1, out TextureId);
                    GL.BindTexture(TextureTarget, TextureId);

                    IsRenderbuffer = false;

                    // No filtering on depth buffer
                    if ((Description.Flags & (TextureFlags.RenderTarget | TextureFlags.DepthStencil)) != TextureFlags.None)
                    {
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        BoundSamplerState = GraphicsDevice.SamplerStates.PointClamp;
                    }
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    else if (Description.MipLevels <= 1)
                    {
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    }
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (!GraphicsDevice.IsOpenGLES2)
#endif
                    {
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureBaseLevel, 0);
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureMaxLevel, Description.MipLevels - 1);
                    }

                    if (Description.MipLevels == 0)
                        throw new NotImplementedException();

                    var setSize = TextureSetSize(TextureTarget);

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
                                var dataSetTarget = GetTextureTargetForDataSet2D(TextureTarget, arrayIndex);
                                if (compressed)
                                {
                                    GL.CompressedTexImage2D(dataSetTarget, i, (CompressedInternalFormat2D)internalFormat,
                                        width, height, 0, dataBoxes[offsetArray + i].SlicePitch, data);
                                }
                                else
                                {
                                    GL.TexImage2D(dataSetTarget, i, (TextureComponentCount2D)internalFormat, width, height, 0, format, type, data);
                                }
                            }
                            else if (setSize == 3)
                            {
                                var dataSetTarget = GetTextureTargetForDataSet3D(TextureTarget);
                                var depth = TextureTarget == TextureTarget.Texture2DArray ? Description.Depth : CalculateMipSize(Description.Depth, i); // no depth mipmaps in Texture2DArray
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
                    GL.BindTexture(TextureTarget, 0);
                    if (openglContext.CommandList != null)
                    {
                        // If we messed up with some states of a command list, mark dirty states
                        openglContext.CommandList.boundShaderResourceViews[openglContext.CommandList.activeTexture] = null;
                    }

                    InitializePixelBufferObject();
                }

                GraphicsDevice.TextureMemory += (Depth * DepthStride) / (float)0x100000;
            }
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
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
                if (TextureId != 0)
                {
                    if (IsRenderbuffer)
                        GL.DeleteRenderbuffers(1, ref TextureId);
                    else
                        GL.DeleteTextures(1, ref TextureId);

                    GraphicsDevice.TextureMemory -= (Depth * DepthStride) / (float)0x100000;
                }

                if (stencilId != 0)
                    GL.DeleteRenderbuffers(1, ref stencilId);

                if (pixelBufferObjectId != 0)
                    GL.DeleteBuffers(1, ref pixelBufferObjectId);
            }

            TextureId = 0;
            stencilId = 0;
            pixelBufferObjectId = 0;

            base.OnDestroyed();
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
                    depthFormat = RenderbufferInternalFormat.DepthComponent32f;
                    break;
                case PixelFormat.D32_Float_S8X24_UInt:
                    if (graphicsDevice.IsOpenGLES2)
                        throw new NotSupportedException("Only 16 bits depth buffer or 24-8 bits depth-stencil buffer is supported on OpenGLES2");
                    // no need to check graphicsDevice.HasPackedDepthStencilExtension since supported 32F depth means OpenGL ES 3, so packing is available.
                    depthFormat = RenderbufferInternalFormat.Depth32fStencil8;
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
