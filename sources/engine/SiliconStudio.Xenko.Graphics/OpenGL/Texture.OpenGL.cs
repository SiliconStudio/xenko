// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
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
        private const int TextureRowPitchAlignment = 1;
        private const int TextureSubresourceAlignment = 1;

        internal const TextureFlags TextureFlagsCustomResourceId = (TextureFlags)0x1000;

        internal SamplerState BoundSamplerState;
        internal int PixelBufferFrame;
        internal int TextureTotalSize;
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
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                        if (ArraySize > 1)
                            throw new PlatformNotSupportedException("Texture1DArray is not implemented under OpenGL");
                        TextureTarget = TextureTarget.Texture1D;
                        break;
#endif
                    case TextureDimension.Texture2D:
                        TextureTarget = ArraySize > 1 ? TextureTarget.Texture2DArray : TextureTarget.Texture2D;
                        break;
                    case TextureDimension.Texture3D:
                        TextureTarget = TextureTarget.Texture3D;
                        break;
                    case TextureDimension.TextureCube:
                        if (ArraySize > 6)
                            throw new PlatformNotSupportedException("TextureCubeArray is not implemented under OpenGL");
                        TextureTarget = TextureTarget.TextureCubeMap;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                bool compressed;
                OpenGLConvertExtensions.ConvertPixelFormat(GraphicsDevice, ref textureDescription.Format, out TextureInternalFormat, out TextureFormat, out TextureType, out TexturePixelSize, out compressed);

                DepthPitch = Description.Width * Description.Height * TexturePixelSize;
                RowPitch = Description.Width * TexturePixelSize;

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

                    IsRenderbuffer = false;

                    TextureTotalSize = ComputeBufferTotalSize();

                    if (Description.Usage == GraphicsResourceUsage.Staging)
                    {
                        InitializeStagingPixelBufferObject(dataBoxes);
                        return;
                    }

                    GL.GenTextures(1, out TextureId);
                    GL.BindTexture(TextureTarget, TextureId);

                    // No filtering on depth buffer
                    if ((Description.Flags & (TextureFlags.RenderTarget | TextureFlags.DepthStencil)) != TextureFlags.None)
                    {
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        BoundSamplerState = GraphicsDevice.SamplerStates.PointClamp;

                        if (HasStencil)
                        {
                            // depth+stencil in a single texture
                            stencilId = TextureId;
                        }
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
                            DataBox dataBox;
                            var width = CalculateMipSize(Description.Width, i);
                            var height = CalculateMipSize(Description.Height, i);
                            var depth = CalculateMipSize(Description.Depth, i);
                            if (dataBoxes != null && i < dataBoxes.Length)
                            {
                                if (setSize > 1 && !compressed && dataBoxes[i].RowPitch != width*TexturePixelSize)
                                    throw new NotSupportedException("Can't upload texture with pitch in glTexImage2D/3D.");
                                // Might be possible, need to check API better.
                                dataBox = dataBoxes[offsetArray + i];
                            }
                            else
                            {
                                dataBox = new DataBox();
                            }

                            switch (TextureTarget)
                            {
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                                case TextureTarget.Texture1D:
                                    if (compressed)
                                    {
                                        GL.CompressedTexImage1D(TextureTarget, i, TextureInternalFormat, width, 0, dataBox.SlicePitch, dataBox.DataPointer);
                                    }
                                    else
                                    {
                                        GL.TexImage1D(TextureTarget, i, TextureInternalFormat, width, 0, TextureFormat, TextureType, dataBox.DataPointer);
                                    }
                                    break;
#endif
                                case TextureTarget.Texture2D:
                                case TextureTarget.TextureCubeMap:
                                {
                                    var dataSetTarget = GetTextureTargetForDataSet2D(TextureTarget, arrayIndex);
                                    if (compressed)
                                    {
                                        GL.CompressedTexImage2D(dataSetTarget, i, (CompressedInternalFormat2D)TextureInternalFormat, width, height, 0, dataBox.SlicePitch, dataBox.DataPointer);
                                    }
                                    else
                                    {
                                        GL.TexImage2D(dataSetTarget, i, (TextureComponentCount2D)TextureInternalFormat, width, height, 0, TextureFormat, TextureType, dataBox.DataPointer);
                                    }
                                    break;
                                }
                                case TextureTarget.Texture3D:
                                {
                                    if (compressed)
                                    {
                                        GL.CompressedTexImage3D((TextureTarget3d)TextureTarget, i, (CompressedInternalFormat3D)TextureInternalFormat, width, height, depth, 0, dataBox.SlicePitch, dataBox.DataPointer);
                                    }
                                    else
                                    {
                                        GL.TexImage3D((TextureTarget3d)TextureTarget, i, (TextureComponentCount3D)TextureInternalFormat, width, height, depth, 0, TextureFormat, TextureType, dataBox.DataPointer);
                                    }
                                    break;
                                }
                                case TextureTarget.Texture2DArray:
                                {
                                    // We create all array slices at once, but upload them one by one
                                    if (arrayIndex == 0)
                                    {
                                        if (compressed)
                                        {
                                            GL.CompressedTexImage3D((TextureTarget3d)TextureTarget, i, (CompressedInternalFormat3D)TextureInternalFormat, width, height, ArraySize, 0, 0, IntPtr.Zero);
                                        }
                                        else
                                        {
                                            GL.TexImage3D((TextureTarget3d)TextureTarget, i, (TextureComponentCount3D)TextureInternalFormat, width, height, ArraySize, 0, TextureFormat, TextureType, IntPtr.Zero);
                                        }
                                    }

                                    if (dataBox.DataPointer != IntPtr.Zero)
                                    {
                                        if (compressed)
                                        {
                                            GL.CompressedTexSubImage3D((TextureTarget3d)TextureTarget, i, 0, 0, arrayIndex, width, height, 1, TextureFormat, dataBox.SlicePitch, dataBox.DataPointer);
                                        }
                                        else
                                        {
                                            GL.TexSubImage3D((TextureTarget3d)TextureTarget, i, 0, 0, arrayIndex, width, height, 1, TextureFormat, TextureType, dataBox.DataPointer);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    GL.BindTexture(TextureTarget, 0);
                    if (openglContext.CommandList != null)
                    {
                        // If we messed up with some states of a command list, mark dirty states
                        openglContext.CommandList.boundShaderResourceViews[openglContext.CommandList.activeTexture] = null;
                    }
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
                if (TextureId != 0 && ParentTexture == null)
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

            TextureTotalSize = 0;
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

        internal static bool InternalIsDepthStencilFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.D16_UNorm:
                case PixelFormat.D32_Float:
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

        internal static TextureTarget2d GetTextureTargetForDataSet2D(TextureTarget target, int arrayIndex)
        {
            // TODO: Proxy from ES 3.1?
            if (target == TextureTarget.TextureCubeMap)
                return TextureTarget2d.TextureCubeMapPositiveX + arrayIndex;
            return (TextureTarget2d)target;
        }

        internal static TextureTarget3d GetTextureTargetForDataSet3D(TextureTarget target)
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

        private void InitializeStagingPixelBufferObject(DataBox[] dataBoxes)
        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (GraphicsDevice.IsOpenGLES2)
            {
                StagingData = Marshal.AllocHGlobal(TextureTotalSize);
            }
            else
#endif
            {
                pixelBufferObjectId = GeneratePixelBufferObject(BufferTarget.PixelPackBuffer, PixelStoreParameter.PackAlignment, BufferUsageHint.StreamRead, TextureTotalSize);
            }
            UploadInitialData(BufferTarget.PixelPackBuffer, dataBoxes);
        }

        private void UploadInitialData(BufferTarget bufferTarget, DataBox[] dataBoxes)
        {
            // Upload initial data
            int offset = 0;
            var bufferData = IntPtr.Zero;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            bufferData = StagingData;
#endif

            if (PixelBufferObjectId != 0)
            {
                GL.BindBuffer(bufferTarget, PixelBufferObjectId);
                bufferData = GL.MapBufferRange(bufferTarget, (IntPtr)0, (IntPtr)TextureTotalSize, BufferAccessMask.MapWriteBit | BufferAccessMask.MapUnsynchronizedBit);
            }

            if (bufferData != IntPtr.Zero)
            {
                for (var arrayIndex = 0; arrayIndex < Description.ArraySize; ++arrayIndex)
                {
                    var offsetArray = arrayIndex * Description.MipLevels;
                    for (int i = 0; i < Description.MipLevels; ++i)
                    {
                        IntPtr data = IntPtr.Zero;
                        var width = CalculateMipSize(Description.Width, i);
                        var height = CalculateMipSize(Description.Height, i);
                        var depth = CalculateMipSize(Description.Depth, i);
                        if (dataBoxes != null && i < dataBoxes.Length)
                        {
                            data = dataBoxes[offsetArray + i].DataPointer;
                        }

                        if (data != IntPtr.Zero)
                        {
                            Utilities.CopyMemory(bufferData + offset, data, width * height * depth * TexturePixelSize);
                        }

                        offset += width*height*TexturePixelSize;
                    }
                }

                if (PixelBufferObjectId != 0)
                {
                    GL.UnmapBuffer(bufferTarget);
                    GL.BindBuffer(bufferTarget, 0);
                }
            }
        }

        internal int GeneratePixelBufferObject(BufferTarget target, PixelStoreParameter alignment, BufferUsageHint bufferUsage, int totalSize)
        {
            int result;

            GL.GenBuffers(1, out result);
            GL.BindBuffer(target, result);
            if (RowPitch < 4)
                GL.PixelStore(alignment, 1);
            GL.BufferData(target, totalSize, IntPtr.Zero, bufferUsage);
            GL.BindBuffer(target, 0);

            return result;
        }
    }
}

#endif
