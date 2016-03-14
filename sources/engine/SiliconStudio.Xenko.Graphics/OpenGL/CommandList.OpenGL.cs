// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL

using System;
using System.Threading;
using OpenTK.Graphics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;
using Color4 = SiliconStudio.Core.Mathematics.Color4;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using PrimitiveTypeGl = OpenTK.Graphics.ES30.PrimitiveType;
using PixelFormatGl = OpenTK.Graphics.ES30.PixelFormat;
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
using BeginMode = OpenTK.Graphics.ES30.BeginMode;
#else
using BeginMode = OpenTK.Graphics.ES30.PrimitiveType;
#endif
using DebugSourceExternal = OpenTK.Graphics.ES30.All;
#else
using OpenTK.Graphics.OpenGL;
using PrimitiveTypeGl = OpenTK.Graphics.OpenGL.PrimitiveType;
#endif

// TODO: remove these when OpenTK API is consistent between OpenGL, mobile OpenGL ES and desktop OpenGL ES
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
using PixelInternalFormat_TextureComponentCount = OpenTK.Graphics.ES30.PixelInternalFormat;
using TextureTarget_TextureTarget2d = OpenTK.Graphics.ES30.TextureTarget;
#else
using PixelInternalFormat_TextureComponentCount = OpenTK.Graphics.ES30.TextureComponentCount;
using TextureTarget_TextureTarget2d = OpenTK.Graphics.ES30.TextureTarget2d;
#endif
#else
using PixelInternalFormat_TextureComponentCount = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using TextureTarget_TextureTarget2d = OpenTK.Graphics.OpenGL.TextureTarget;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    public partial class CommandList
    {
        // How many frames to wait before allowing non-blocking texture readbacks
        private const int ReadbackFrameDelay = 2;
        private const int MaxBoundRenderTargets = 16;

        internal uint enabledVertexAttribArrays;
        private int boundProgram = 0;

        internal int BoundStencilReference;
        internal int NewStencilReference;

        private bool vboDirty = true;

        private Texture boundDepthStencilBuffer;
        private Texture[] boundRenderTargets = new Texture[MaxBoundRenderTargets];
        internal Texture[] boundTextures = new Texture[64];
        private Texture[] textures = new Texture[64];
        private SamplerState[] samplerStates = new SamplerState[64];

        internal DepthStencilBoundState DepthStencilBoundState;
        internal RasterizerBoundState RasterizerBoundState;

        private Buffer[] constantBuffers = new Buffer[64];

        private int boundFBO;
        private bool needUpdateFBO = true;

        private PipelineState newPipelineState;
        private PipelineState currentPipelineState;

        private DescriptorSet[] currentDescriptorSets = new DescriptorSet[32];

        internal int activeTexture = 0;

        private IndexBufferView indexBuffer;

        private VertexBufferView[] vertexBuffers = new VertexBufferView[8];

        private Rectangle[] currentScissorRectangles = new Rectangle[MaxBoundRenderTargets];

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private float[] _currentViewportsSetBuffer = new float[4 * MaxBoundRenderTargets];
        private int[] _currentScissorsSetBuffer = new int[4 * MaxBoundRenderTargets];
#endif

        public CommandList(GraphicsDevice device) : base(device)
        {
            device.MainCommandList = this;

            // Default state
            DepthStencilBoundState.DepthBufferWriteEnable = true;
            DepthStencilBoundState.StencilWriteMask = 0xFF;
            RasterizerBoundState.FrontFaceDirection = FrontFaceDirection.Ccw;
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            RasterizerBoundState.PolygonMode = PolygonMode.Fill;
#endif

            ClearState();
        }

        public void Reset()
        {
        }

        public void Close()
        {
        }

        public void Clear(Texture depthStencilBuffer, DepthStencilClearOptions options, float depth = 1, byte stencil = 0)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_PLATFORM_ANDROID
            // Device with no background loading context: check if some loading is pending
            if (GraphicsDevice.AsyncPendingTaskWaiting)
                GraphicsDevice.ExecutePendingTasks();
#endif

            var clearFBO = GraphicsDevice.FindOrCreateFBO(depthStencilBuffer);
            if (clearFBO != boundFBO)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, clearFBO);

            ClearBufferMask clearBufferMask =
                ((options & DepthStencilClearOptions.DepthBuffer) == DepthStencilClearOptions.DepthBuffer ? ClearBufferMask.DepthBufferBit : 0)
                | ((options & DepthStencilClearOptions.Stencil) == DepthStencilClearOptions.Stencil ? ClearBufferMask.StencilBufferBit : 0);
            GL.ClearDepth(depth);
            GL.ClearStencil(stencil);

            // Check if we need to change depth mask
            var currentDepthMask = DepthStencilBoundState.DepthBufferWriteEnable;

            if (!currentDepthMask)
                GL.DepthMask(true);
            GL.Clear(clearBufferMask);
            if (!currentDepthMask)
                GL.DepthMask(false);

            if (clearFBO != boundFBO)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
        }

        public void Clear(Texture renderTarget, Color4 color)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            var clearFBO = GraphicsDevice.FindOrCreateFBO(renderTarget);
            if (clearFBO != boundFBO)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, clearFBO);

            // Check if we need to change color mask
            var blendState = currentPipelineState.BlendState;
            var needColorMaskOverride = blendState.ColorWriteChannels != ColorWriteChannels.All;

            if (needColorMaskOverride)
                GL.ColorMask(true, true, true, true);

            GL.ClearColor(color.R, color.G, color.B, color.A);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // revert the color mask value as it was before
            if (needColorMaskOverride)
                blendState.RestoreColorMask();

            if (clearFBO != boundFBO)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
        }

        public unsafe void ClearReadWrite(Buffer buffer, Vector4 value)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            if ((buffer.ViewFlags & BufferFlags.UnorderedAccess) != BufferFlags.UnorderedAccess)
                throw new ArgumentException("Buffer does not support unordered access");

            GL.BindBuffer(buffer.bufferTarget, buffer.resourceId);
            GL.ClearBufferData(buffer.bufferTarget, buffer.internalFormat, buffer.glPixelFormat, All.UnsignedInt8888, ref value);
            GL.BindBuffer(buffer.bufferTarget, 0);
#endif
        }

        public unsafe void ClearReadWrite(Buffer buffer, Int4 value)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            if ((buffer.ViewFlags & BufferFlags.UnorderedAccess) != BufferFlags.UnorderedAccess)
                throw new ArgumentException("Buffer does not support unordered access");

            GL.BindBuffer(buffer.bufferTarget, buffer.resourceId);
            GL.ClearBufferData(buffer.bufferTarget, buffer.internalFormat, buffer.glPixelFormat, All.UnsignedInt8888, ref value);
            GL.BindBuffer(buffer.bufferTarget, 0);
#endif
        }

        public unsafe void ClearReadWrite(Buffer buffer, UInt4 value)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            if ((buffer.ViewFlags & BufferFlags.UnorderedAccess) != BufferFlags.UnorderedAccess)
                throw new ArgumentException("Buffer does not support unordered access");

            GL.BindBuffer(buffer.bufferTarget, buffer.resourceId);
            GL.ClearBufferData(buffer.bufferTarget, buffer.internalFormat, buffer.glPixelFormat, All.UnsignedInt8888, ref value);
            GL.BindBuffer(buffer.bufferTarget, 0);
#endif
        }

        public unsafe void ClearReadWrite(Texture texture, Vector4 value)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.BindTexture(texture.Target, texture.resourceId);

            GL.ClearTexImage(texture.resourceId, 0, texture.FormatGl, texture.Type, ref value);

            GL.BindTexture(texture.Target, 0);
            boundTextures[0] = null;
#endif
        }

        public unsafe void ClearReadWrite(Texture texture, Int4 value)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.BindTexture(texture.Target, texture.resourceId);

            GL.ClearTexImage(texture.resourceId, 0, texture.FormatGl, texture.Type, ref value);

            GL.BindTexture(texture.Target, 0);
            boundTextures[0] = null;
#endif
        }

        public unsafe void ClearReadWrite(Texture texture, UInt4 value)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.BindTexture(texture.Target, texture.resourceId);

            GL.ClearTexImage(texture.resourceId, 0, texture.FormatGl, texture.Type, ref value);

            GL.BindTexture(texture.Target, 0);
            boundTextures[0] = null;
#endif
        }

        private void ClearStateImpl()
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            // Clear sampler states
            for (int i = 0; i < samplerStates.Length; ++i)
                samplerStates[i] = null;

            for (int i = 0; i < boundTextures.Length; ++i)
            {
                textures[i] = null;
            }

            // Clear active texture state
            activeTexture = 0;
            GL.ActiveTexture(TextureUnit.Texture0);

            // set default states
            currentPipelineState = GraphicsDevice.DefaultPipelineState;
            newPipelineState = GraphicsDevice.DefaultPipelineState;

            // Actually reset states
            //currentPipelineState.BlendState.Apply();
            GL.Disable(EnableCap.Blend);
            GL.ColorMask(true, true, true, true);
            currentPipelineState.DepthStencilState.Apply(this);
            currentPipelineState.RasterizerState.Apply(this);

            // Set default render targets
            SetRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLCORE
            GL.Enable(EnableCap.FramebufferSrgb);
#endif
        }

        /// <summary>
        /// Copy a region of a <see cref="GraphicsResource"/> into another.
        /// </summary>
        /// <param name="source">The source from which to copy the data</param>
        /// <param name="regionSource">The region of the source <see cref="GraphicsResource"/> to copy.</param>
        /// <param name="destination">The destination into which to copy the data</param>
        /// <remarks>This might alter some states such as currently bound texture.</remarks>
        public void CopyRegion(GraphicsResource source, int sourceSubresource, ResourceRegion? regionSource, GraphicsResource destination, int destinationSubResource, int dstX = 0, int dstY = 0, int dstZ = 0)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            var sourceTexture = source as Texture;
            var destTexture = destination as Texture;

            if (sourceTexture == null || destTexture == null)
                throw new NotImplementedException("Copy is only implemented for ITexture2D objects.");

            if (sourceSubresource != 0 || destinationSubResource != 0)
                throw new NotImplementedException("Copy is only implemented for subresource 0 in OpenGL.");

            var sourceRegion = regionSource.HasValue ? regionSource.Value : new ResourceRegion(0, 0, 0, sourceTexture.Description.Width, sourceTexture.Description.Height, 0);
            var sourceRectangle = new Rectangle(sourceRegion.Left, sourceRegion.Top, sourceRegion.Right - sourceRegion.Left, sourceRegion.Bottom - sourceRegion.Top);

            if (sourceRectangle.Width == 0 || sourceRectangle.Height == 0)
                return;

            if (destTexture.Description.Usage == GraphicsResourceUsage.Staging)
            {
                if (dstX != 0 || dstY != 0 || dstZ != 0)
                    throw new NotSupportedException("ReadPixels from staging texture using non-zero destination is not supported");

                GL.Viewport(0, 0, destTexture.Description.Width, destTexture.Description.Height);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, GraphicsDevice.FindOrCreateFBO(source));

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (GraphicsDevice.IsOpenGLES2)
                {
                    var format = destTexture.FormatGl;
                    var type = destTexture.Type;

                    var srcFormat = sourceTexture.Description.Format;
                    var destFormat = destTexture.Description.Format;

                    if (srcFormat == destFormat && destFormat.SizeInBytes() == 4)   // in this case we just want to copy the data we don't care about format conversion. 
                    {                                                               // RGBA/Unsigned-byte is always a working combination whatever is the internal format (sRGB, etc...)
                        format = PixelFormatGl.Rgba;
                        type = PixelType.UnsignedByte;
                    }

                    GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, format, type, destTexture.StagingData);
                }
                else
#endif
                {
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, destTexture.PixelBufferObjectId);
                    GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, destTexture.FormatGl, destTexture.Type, IntPtr.Zero);
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);

                    destTexture.PixelBufferFrame = GraphicsDevice.FrameCounter;
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
                GL.Viewport((int)viewports[0].X, (int)viewports[0].Y, (int)viewports[0].Width, (int)viewports[0].Height);
                return;
            }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (GraphicsDevice.IsOpenGLES2)
            {
                CopyScaler2D(sourceTexture, destTexture, sourceRectangle, new Rectangle(dstX, dstY, sourceRectangle.Width, sourceRectangle.Height));
            }
            else
#endif
            {
                // "FindOrCreateFBO" set the frameBuffer on FBO creation -> those 2 calls cannot be made directly in the following "GL.BindFramebuffer" function calls (side effects)
                var sourceFBO = GraphicsDevice.FindOrCreateFBO(source);
                var destinationFBO = GraphicsDevice.FindOrCreateFBO(destination);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFBO);
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, destinationFBO);
                GL.BlitFramebuffer(sourceRegion.Left, sourceRegion.Top, sourceRegion.Right, sourceRegion.Bottom,
                    dstX, dstY, dstX + sourceRegion.Right - sourceRegion.Left, dstY + sourceRegion.Bottom - sourceRegion.Top,
                    ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
            }
        }

        internal void CopyScaler2D(Texture sourceTexture, Texture destTexture, Rectangle sourceRectangle, Rectangle destRectangle, bool flipY = false)
        {
            // Use rendering
            GL.Viewport(0, 0, destTexture.Description.Width, destTexture.Description.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GraphicsDevice.FindOrCreateFBO(destTexture));

            var sourceRegionSize = new Vector2(sourceRectangle.Width, sourceRectangle.Height);
            var destRegionSize = new Vector2(destRectangle.Width, destRectangle.Height);

            // Source
            var sourceSize = new Vector2(sourceTexture.Width, sourceTexture.Height);
            var sourceRegionLeftTop = new Vector2(sourceRectangle.Left, sourceRectangle.Top);
            var sourceScale = new Vector2(sourceRegionSize.X / sourceSize.X, sourceRegionSize.Y / sourceSize.Y);
            var sourceOffset = new Vector2(sourceRegionLeftTop.X / sourceSize.X, sourceRegionLeftTop.Y / sourceSize.Y);

            // Dest
            var destSize = new Vector2(destTexture.Width, destTexture.Height);
            var destRegionLeftTop = new Vector2(destRectangle.X, flipY ? destRectangle.Bottom : destRectangle.Y);
            var destScale = new Vector2(destRegionSize.X / destSize.X, destRegionSize.Y / destSize.Y);
            var destOffset = new Vector2(destRegionLeftTop.X / destSize.X, destRegionLeftTop.Y / destSize.Y);

            if (flipY)
                destScale.Y = -destScale.Y;

            var enabledColors = new bool[4];
            GL.GetBoolean(GetPName.ColorWritemask, enabledColors);
            var isDepthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
            var isCullFaceEnabled = GL.IsEnabled(EnableCap.CullFace);
            var isBlendEnabled = GL.IsEnabled(EnableCap.Blend);
            var isStencilEnabled = GL.IsEnabled(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.StencilTest);
            GL.ColorMask(true, true, true, true);

            // TODO find a better way to detect if sRGB conversion is needed (need to detect if main frame buffer is sRGB or not at init time)
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            // If we are copying from an SRgb texture to a non SRgb texture, we use a special SRGb copy shader
            bool needSRgbConversion = sourceTexture.Description.Format.IsSRgb() && destTexture == GraphicsDevice.WindowProvidedRenderTexture;
#else
            bool needSRgbConversion = false;
#endif
            int offsetLocation, scaleLocation;
            var program = GraphicsDevice.GetCopyProgram(needSRgbConversion, out offsetLocation, out scaleLocation);

            GL.UseProgram(program);

            activeTexture = 0;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sourceTexture.resourceId);
            boundTextures[0] = null;
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            sourceTexture.BoundSamplerState = GraphicsDevice.SamplerStates.PointClamp;

            vboDirty = true;
            enabledVertexAttribArrays |= 1 << 0;
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, GraphicsDevice.GetSquareBuffer().ResourceId);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.Uniform4(offsetLocation, sourceOffset.X, sourceOffset.Y, destOffset.X, destOffset.Y);
            GL.Uniform4(scaleLocation, sourceScale.X, sourceScale.Y, destScale.X, destScale.Y);
            GL.Viewport(0, 0, destTexture.Width, destTexture.Height);
            GL.DrawArrays((BeginMode)PrimitiveTypeGl.TriangleStrip, 0, 4);
            GL.UseProgram(boundProgram);

            // Restore context
            if (isDepthTestEnabled)
                GL.Enable(EnableCap.DepthTest);
            if (isCullFaceEnabled)
                GL.Enable(EnableCap.CullFace);
            if (isBlendEnabled)
                GL.Enable(EnableCap.Blend);
            if (isStencilEnabled)
                GL.Enable(EnableCap.StencilTest);
            GL.ColorMask(enabledColors[0], enabledColors[1], enabledColors[2], enabledColors[3]);

            // Restore FBO and viewport
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
            GL.Viewport((int)viewports[0].X, (int)viewports[0].Y, (int)viewports[0].Width, (int)viewports[0].Height);
        }

        /// <summary>
        /// Copy a <see cref="GraphicsResource"/> into another.
        /// </summary>
        /// <param name="source">The source from which to copy the data</param>
        /// <param name="destination">The destination into which to copy the data</param>
        /// <remarks>This might alter some states such as currently bound texture.</remarks>
        public void Copy(GraphicsResource source, GraphicsResource destination)
        {
            CopyRegion(source, 0, null, destination, 0);
        }

        public void CopyMultiSample(Texture sourceMsaaTexture, int sourceSubResource, Texture destTexture, int destSubResource, PixelFormat format = PixelFormat.None)
        {
            throw new NotImplementedException();
        }

        public void CopyCount(Buffer sourceBuffer, Buffer destBuffer, int offsetToDest)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        public void Dispatch(int threadCountX, int threadCountY, int threadCountZ)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            GL.DispatchCompute(threadCountX, threadCountY, threadCountZ);
#else
            throw new NotImplementedException();
#endif
        }

        public void Dispatch(Buffer indirectBuffer, int offsetInBytes)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            GL.BindBuffer(BufferTarget.DispatchIndirectBuffer, indirectBuffer.resourceId);

            GL.DispatchComputeIndirect((IntPtr)offsetInBytes);

            GL.BindBuffer(BufferTarget.DispatchIndirectBuffer, 0);
#else
            throw new NotImplementedException();
#endif
        }

        public void Draw(int vertexCount, int startVertex = 0)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            PreDraw();

            GL.DrawArrays((BeginMode)newPipelineState.PrimitiveType, startVertex, vertexCount);

            GraphicsDevice.FrameTriangleCount += (uint)vertexCount;
            GraphicsDevice.FrameDrawCalls++;
        }

        public void DrawAuto(PrimitiveType primitiveType)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            PreDraw();

            //GL.DrawArraysIndirect(newPipelineState.PrimitiveType, (IntPtr)0);
            //GraphicsDevice.FrameDrawCalls++;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draw indexed, non-instanced primitives.
        /// </summary>
        /// <param name="indexCount">Number of indices to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        public void DrawIndexed(int indexCount, int startIndexLocation = 0, int baseVertexLocation = 0)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            PreDraw();

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if(baseVertexLocation != 0)
                throw new NotSupportedException("DrawIndexed with no null baseVertexLocation is not supported on OpenGL ES.");
            GL.DrawElements((BeginMode)newPipelineState.PrimitiveType, indexCount, indexBuffer.Type, indexBuffer.Buffer.StagingData + indexBuffer.Offset + (startIndexLocation * indexBuffer.ElementSize)); // conversion to IntPtr required on Android
#else
            GL.DrawElementsBaseVertex(newPipelineState.PrimitiveType, indexCount, indexBuffer.Type, indexBuffer.Buffer.StagingData + indexBuffer.Offset + (startIndexLocation * indexBuffer.ElementSize), baseVertexLocation);
#endif

            GraphicsDevice.FrameDrawCalls++;
            GraphicsDevice.FrameTriangleCount += (uint)indexCount;
        }

        /// <summary>
        /// Draw indexed, instanced primitives.
        /// </summary>
        /// <param name="indexCountPerInstance">Number of indices read from the index buffer for each instance.</param>
        /// <param name="instanceCount">Number of instances to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        /// <param name="startInstanceLocation">A value added to each index before reading per-instance data from a vertex buffer.</param>
        public void DrawIndexedInstanced(int indexCountPerInstance, int instanceCount, int startIndexLocation = 0, int baseVertexLocation = 0, int startInstanceLocation = 0)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            PreDraw();
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            GraphicsDevice.FrameDrawCalls++;
            GraphicsDevice.FrameTriangleCount += (uint)(indexCountPerInstance * instanceCount);
            throw new NotImplementedException();
#else
            GL.DrawElementsInstancedBaseVertex(newPipelineState.PrimitiveType, indexCountPerInstance, indexBuffer.Type, indexBuffer.Buffer.StagingData + indexBuffer.Offset + (startIndexLocation * indexBuffer.ElementSize), instanceCount, baseVertexLocation);

            GraphicsDevice.FrameDrawCalls++;
            GraphicsDevice.FrameTriangleCount += (uint)(indexCountPerInstance * instanceCount);
#endif
        }

        /// <summary>
        /// Draw indexed, instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="argumentsBuffer">A buffer containing the GPU generated primitives.</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawIndexedInstanced(Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {

            if (argumentsBuffer == null) throw new ArgumentNullException(nameof(argumentsBuffer));

#if DEBUG
            //GraphicsDevice.EnsureContextActive();
#endif
            //PreDraw();

            //GraphicsDevice.FrameDrawCalls++;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Draw non-indexed, instanced primitives.
        /// </summary>
        /// <param name="vertexCountPerInstance">Number of vertices to draw.</param>
        /// <param name="instanceCount">Number of instances to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex.</param>
        /// <param name="startInstanceLocation">A value added to each index before reading per-instance data from a vertex buffer.</param>
        public void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation = 0, int startInstanceLocation = 0)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            PreDraw();

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (GraphicsDevice.IsOpenGLES2)
                throw new NotSupportedException("DrawArraysInstanced is not supported on OpenGL ES 2");
#endif
            GL.DrawArraysInstanced(newPipelineState.PrimitiveType, startVertexLocation, vertexCountPerInstance, instanceCount);

            GraphicsDevice.FrameDrawCalls++;
            GraphicsDevice.FrameTriangleCount += (uint)(vertexCountPerInstance * instanceCount);
        }

        /// <summary>
        /// Draw instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="argumentsBuffer">An arguments buffer</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawInstanced(Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {
            if (argumentsBuffer == null)
                throw new ArgumentNullException(nameof(argumentsBuffer));

#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            PreDraw();

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            GraphicsDevice.FrameDrawCalls++;
            throw new NotImplementedException();
#else
            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, argumentsBuffer.resourceId);

            GL.DrawArraysIndirect(newPipelineState.PrimitiveType, (IntPtr)alignedByteOffsetForArgs);

            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, 0);

            GraphicsDevice.FrameDrawCalls++;
#endif
        }

        public void BeginProfile(Color profileColor, string name)
        {
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
            if (GraphicsDevice.ProfileEnabled)
                GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 1, -1, name);
#endif
        }

        public void EndProfile()
        {
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
            if (GraphicsDevice.ProfileEnabled)
                GL.PopDebugGroup();
#endif
        }

        public MappedResource MapSubresource(GraphicsResource resource, int subResourceIndex, MapMode mapMode, bool doNotWait = false, int offsetInBytes = 0, int lengthInBytes = 0)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            var buffer = resource as Buffer;
            if (buffer != null)
            {
                if (lengthInBytes == 0)
                    lengthInBytes = buffer.Description.SizeInBytes;

                if (buffer.StagingData != IntPtr.Zero)
                {
                    // Specific case for constant buffers
                    return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = buffer.StagingData + offsetInBytes, SlicePitch = 0, RowPitch = 0 }, offsetInBytes, lengthInBytes);
                }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                // OpenGL ES 2 needs Staging Data
                if (GraphicsDevice.IsOpenGLES2)
                    throw new NotImplementedException();
#endif

                IntPtr mapResult = IntPtr.Zero;

                //UnbindVertexArrayObject();
                GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                //if (mapMode != MapMode.WriteDiscard && mapMode != MapMode.WriteNoOverwrite)
                //    mapResult = GL.MapBuffer(buffer.bufferTarget, mapMode.ToOpenGL());
                //else
#endif
                {
                    // Orphan the buffer (let driver knows we don't need it anymore)
                    if (mapMode == MapMode.WriteDiscard)
                    {
                        doNotWait = true;
                        GL.BufferData(buffer.bufferTarget, (IntPtr)buffer.Description.SizeInBytes, IntPtr.Zero, buffer.bufferUsageHint);
                    }

                    var unsynchronized = doNotWait && mapMode != MapMode.Read && mapMode != MapMode.ReadWrite;

                    mapResult = GL.MapBufferRange(buffer.bufferTarget, (IntPtr)offsetInBytes, (IntPtr)lengthInBytes, mapMode.ToOpenGLMask() | (unsynchronized ? BufferAccessMask.MapUnsynchronizedBit : 0));
                }

                return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = mapResult, SlicePitch = 0, RowPitch = 0 });
            }

            var texture = resource as Texture;
            if (texture != null)
            {
                if (lengthInBytes == 0)
                    lengthInBytes = texture.DepthPitch;

                if (mapMode == MapMode.Read)
                {
                    if (texture.Description.Usage != GraphicsResourceUsage.Staging)
                        throw new NotSupportedException("Only staging textures can be mapped.");

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (GraphicsDevice.IsOpenGLES2 || texture.StagingData != IntPtr.Zero)
                    {
                        return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = texture.StagingData + offsetInBytes, SlicePitch = texture.DepthPitch, RowPitch = texture.RowPitch }, offsetInBytes, lengthInBytes);
                    }
                    else
#endif
                    {
                        if (doNotWait)
                        {
                            // Wait at least 2 frames after last operation
                            if (GraphicsDevice.FrameCounter < texture.PixelBufferFrame + ReadbackFrameDelay)
                            {
                                return new MappedResource(resource, subResourceIndex, new DataBox(), offsetInBytes, lengthInBytes);
                            }
                        }

                        return MapTexture(texture, BufferTarget.PixelPackBuffer, subResourceIndex, mapMode, offsetInBytes, lengthInBytes);
                    }
                }
                else if (mapMode == MapMode.WriteDiscard)
                {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (GraphicsDevice.IsOpenGLES2)
                        throw new NotImplementedException();
#endif
                    if (texture.Description.Usage != GraphicsResourceUsage.Dynamic)
                        throw new NotSupportedException("Only dynamic texture can be mapped.");

                    return MapTexture(texture, BufferTarget.PixelUnpackBuffer, subResourceIndex, mapMode, offsetInBytes, lengthInBytes);
                }
            }

            throw new NotImplementedException("MapSubresource not implemented for type " + resource.GetType());
        }

        private MappedResource MapTexture(Texture texture, BufferTarget pixelPackUnpack, int subResourceIndex, MapMode mapMode, int offsetInBytes, int lengthInBytes)
        {
            GL.BindBuffer(pixelPackUnpack, texture.PixelBufferObjectId);
            var mapResult = GL.MapBufferRange(pixelPackUnpack, (IntPtr)offsetInBytes, (IntPtr)lengthInBytes, mapMode.ToOpenGLMask());
            GL.BindBuffer(pixelPackUnpack, 0);

            return new MappedResource(texture, subResourceIndex, new DataBox { DataPointer = mapResult, SlicePitch = texture.DepthPitch, RowPitch = texture.RowPitch }, offsetInBytes, lengthInBytes);
        }

        internal unsafe void PreDraw()
        {
#if SILICONSTUDIO_PLATFORM_ANDROID
            // Device with no background loading context: check if some loading is pending
            if (GraphicsDevice.AsyncPendingTaskWaiting)
                GraphicsDevice.ExecutePendingTasks();
#endif
            // Bind program
            var program = newPipelineState.EffectProgram.ResourceId;
            if (program != boundProgram)
            {
                boundProgram = program;
                GL.UseProgram(boundProgram);
            }

            // Setup index buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer.Buffer != null ? indexBuffer.Buffer.ResourceId : 0);

            int vertexBufferSlot = -1;
            var vertexBufferView = default(VertexBufferView);
            Buffer vertexBuffer = null;
            var vertexBufferBase = IntPtr.Zero;

            // TODO OPENGL compare newPipelineState.VertexAttribs directly
            if (newPipelineState.VertexAttribs != currentPipelineState.VertexAttribs)
            {
                vboDirty = true;
            }

            // Setup vertex buffers and vertex attributes
            if (vboDirty)
            {
                foreach (var vertexAttrib in newPipelineState.VertexAttribs)
                {
                    if (vertexAttrib.VertexBufferSlot != vertexBufferSlot)
                    {
                        vertexBufferSlot = vertexAttrib.VertexBufferSlot;
                        vertexBufferView = vertexBuffers[vertexBufferSlot];
                        vertexBuffer = vertexBufferView.Buffer;
                        if (vertexBuffer != null)
                        {
                            var vertexBufferResource = vertexBufferView.Buffer.ResourceId;
                            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferResource);

                            vertexBufferBase = vertexBufferView.Buffer.StagingData;
                        }
                    }

                    var vertexAttribMask = 1U << vertexAttrib.AttributeIndex;
                    if (vertexBuffer == null)
                    {
                        // No VB bound, turn off this attribute
                        if ((enabledVertexAttribArrays & vertexAttribMask) != 0)
                        {
                            enabledVertexAttribArrays &= ~vertexAttribMask;
                            GL.DisableVertexAttribArray(vertexAttrib.AttributeIndex);
                        }
                        continue;
                    }

                    // Enable this attribute if not previously enabled
                    if ((enabledVertexAttribArrays & vertexAttribMask) == 0)
                    {
                        enabledVertexAttribArrays |= vertexAttribMask;
                        GL.EnableVertexAttribArray(vertexAttrib.AttributeIndex);
                    }

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (vertexAttrib.IsInteger && !vertexAttrib.Normalized)
                        GL.VertexAttribIPointer(vertexAttrib.AttributeIndex, vertexAttrib.Size, (VertexAttribIntegerType)vertexAttrib.Type, vertexBufferView.Stride, vertexBufferBase + vertexBufferView.Offset + vertexAttrib.Offset);
                    else
#endif
                        GL.VertexAttribPointer(vertexAttrib.AttributeIndex, vertexAttrib.Size, vertexAttrib.Type, vertexAttrib.Normalized, vertexBufferView.Stride, vertexBufferBase + vertexBufferView.Offset + vertexAttrib.Offset);
                }

                vboDirty = false;
            }

            // Resources
            newPipelineState.ResourceBinder.BindResources(this, currentDescriptorSets);

            // States
            newPipelineState.Apply(this, currentPipelineState);

            foreach (var textureInfo in newPipelineState.EffectProgram.Textures)
            {
                var boundTexture = boundTextures[textureInfo.TextureUnit];
                var texture = textures[textureInfo.TextureUnit];

                if (texture != null)
                {
                    var boundSamplerState = texture.BoundSamplerState ?? GraphicsDevice.DefaultSamplerState;
                    var samplerState = samplerStates[textureInfo.TextureUnit] ?? GraphicsDevice.SamplerStates.LinearClamp;

                    bool hasMipmap = texture.Description.MipLevels > 1;

                    bool textureChanged = texture != boundTexture;
                    bool samplerStateChanged = samplerState != boundSamplerState;

                    // TODO: Lazy update for texture
                    if (textureChanged || samplerStateChanged)
                    {
                        if (activeTexture != textureInfo.TextureUnit)
                        {
                            activeTexture = textureInfo.TextureUnit;
                            GL.ActiveTexture(TextureUnit.Texture0 + textureInfo.TextureUnit);
                        }

                        // Lazy update for texture
                        if (textureChanged)
                        {
                            boundTextures[textureInfo.TextureUnit] = texture;
                            GL.BindTexture(texture.Target, texture.resourceId);
                        }

                        // Lazy update for sampler state
                        if (samplerStateChanged)
                        {
                            samplerState.Apply(hasMipmap, boundSamplerState, texture.Target);
                            texture.BoundSamplerState = samplerState;
                        }
                    }
                }
            }

            // Update viewports
            SetViewportImpl();

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (GraphicsDevice.IsOpenGLES2)
            {
                fixed(byte* boundUniforms = newPipelineState.EffectProgram.BoundUniforms)
                {
                    foreach (var uniform in newPipelineState.EffectProgram.Uniforms)
                    {
                        var constantBuffer = constantBuffers[uniform.ConstantBufferSlot];
                        if (constantBuffer == null)
                            continue;

                        var constantBufferOffsetStart = newPipelineState.EffectProgram.ConstantBufferOffsets[uniform.ConstantBufferSlot];

                        var constantBufferData = constantBuffer.StagingData;
                        var firstUniformIndex = uniform.UniformIndex;
                        var lastUniformIndex = firstUniformIndex + uniform.Count;
                        var offset = uniform.Offset;
                        var boundData = (IntPtr)boundUniforms + offset + constantBufferOffsetStart;
                        var currentData = constantBufferData + offset;

                        // Already updated? Early exit.
                        // TODO: Not optimal for float1/float2 arrays (rare?)
                        // Better to do "sparse" comparison, not sure if C# code would behave well though
                        if (SiliconStudio.Core.Utilities.CompareMemory(boundData, currentData, uniform.CompareSize))
                            continue;

                        // Update bound cache for early exit
                        SiliconStudio.Core.Utilities.CopyMemory(boundData, currentData, uniform.CompareSize);

                        switch (uniform.Type)
                        {
                            case ActiveUniformType.Float:
                                for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                {
                                    GL.Uniform1(uniformIndex, 1, (float*)currentData);
                                    currentData += 16; // Each array element is spaced by 16 bytes
                                }
                                break;
                            case ActiveUniformType.FloatVec2:
                                for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                {
                                    GL.Uniform2(uniformIndex, 1, (float*)currentData);
                                    currentData += 16; // Each array element is spaced by 16 bytes
                                }
                                break;
                            case ActiveUniformType.FloatVec3:
                                for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                {
                                    GL.Uniform3(uniformIndex, 1, (float*)currentData);
                                    currentData += 16; // Each array element is spaced by 16 bytes
                                }
                                break;
                            case ActiveUniformType.FloatVec4:
                                GL.Uniform4(firstUniformIndex, uniform.Count, (float*)currentData);
                                break;
                            case ActiveUniformType.FloatMat4:
                                GL.UniformMatrix4(uniform.UniformIndex, uniform.Count, false, (float*)currentData);
                                break;
                            case ActiveUniformType.Bool:
                            case ActiveUniformType.Int:
                                for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                {
                                    GL.Uniform1(uniformIndex, 1, (int*)currentData);
                                    currentData += 16; // Each array element is spaced by 16 bytes
                                }
                                break;
                            case ActiveUniformType.BoolVec2:
                            case ActiveUniformType.IntVec2:
                                for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                {
                                    GL.Uniform2(uniformIndex, 1, (int*)currentData);
                                    currentData += 16; // Each array element is spaced by 16 bytes
                                }
                                break;
                            case ActiveUniformType.BoolVec3:
                            case ActiveUniformType.IntVec3:
                                for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                {
                                    GL.Uniform3(uniformIndex, 1, (int*)currentData);
                                    currentData += 16; // Each array element is spaced by 16 bytes
                                }
                                break;
                            case ActiveUniformType.BoolVec4:
                            case ActiveUniformType.IntVec4:
                                GL.Uniform4(firstUniformIndex, uniform.Count, (int*)currentData);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }                
            }
#endif

            currentPipelineState = newPipelineState;
        }

        /// <summary>
        /// Sets a constant buffer to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="buffer">The constant buffer to set.</param>
        internal void SetConstantBuffer(ShaderStage stage, int slot, Buffer buffer)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            if (constantBuffers[slot] != buffer)
            {
                // TODO OPENGL if OpenGL ES 2, might be good to have some dirty flags to explain if cbuffer changed
                constantBuffers[slot] = buffer;
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, slot, buffer != null ? buffer.resourceId : 0);
            }
        }

        private void SetRenderTargetsImpl(Texture depthStencilBuffer, int renderTargetCount, params Texture[] renderTargets)
        {
            var renderTargetsLength = 0;
            if (renderTargetCount > 0)
            {
                renderTargetsLength = renderTargets.Length;
                // ensure size is coherent
                var expectedWidth = renderTargets[0].Width;
                var expectedHeight = renderTargets[0].Height;
                if (depthStencilBuffer != null)
                {
                    if (expectedWidth != depthStencilBuffer.Width || expectedHeight != depthStencilBuffer.Height)
                        throw new Exception("Depth buffer is not the same size as the render target");
                }
                for (int i = 1; i < renderTargets.Length; ++i)
                {
                    if (renderTargets[i] != null && (expectedWidth != renderTargets[i].Width || expectedHeight != renderTargets[i].Height))
                        throw new Exception("Render targets do nt have the same size");
                }
            }

#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            for (int i = 0; i < renderTargetsLength; ++i)
                boundRenderTargets[i] = renderTargets[i];
            for (int i = renderTargetsLength; i < boundRenderTargets.Length; ++i)
                boundRenderTargets[i] = null;

            boundDepthStencilBuffer = depthStencilBuffer;

            needUpdateFBO = true;

            SetupTargets();

            var renderTarget = renderTargetsLength > 0 ? renderTargets[0] : null;
            if (renderTarget != null)
            {
                SetViewport(new Viewport(0, 0, renderTarget.Width, renderTarget.Height));
            }
            else if (depthStencilBuffer != null)
            {
                SetViewport(new Viewport(0, 0, depthStencilBuffer.Description.Width, depthStencilBuffer.Description.Height));
            }
        }

        private void ResetTargetsImpl()
        {
            for (int i = 0; i < boundRenderTargets.Length; ++i)
                boundRenderTargets[i] = null;
        }

        /// <summary>
        /// Sets a sampler state to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="samplerState">The sampler state to set.</param>
        public void SetSamplerState(ShaderStage stage, int slot, SamplerState samplerState)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            samplerStates[slot] = samplerState;
        }

        /// <summary>
        /// Binds a single scissor rectangle to the rasterizer stage.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        /// <param name="right">The right.</param>
        /// <param name="bottom">The bottom.</param>
        public void SetScissorRectangles(int left, int top, int right, int bottom)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            currentScissorRectangles[0].Left = left;
            currentScissorRectangles[0].Top = top;
            currentScissorRectangles[0].Width = right - left;
            currentScissorRectangles[0].Height = bottom - top;

            UpdateScissor(currentScissorRectangles[0]);
        }

        /// <summary>
        /// Binds a set of scissor rectangles to the rasterizer stage.
        /// </summary>
        /// <param name="scissorRectangles">The set of scissor rectangles to bind.</param>
        public void SetScissorRectangles(params Rectangle[] scissorRectangles)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            var scissorCount = scissorRectangles.Length > currentScissorRectangles.Length ? currentScissorRectangles.Length : scissorRectangles.Length;

            for (var i = 0; i < scissorCount; ++i)
                currentScissorRectangles[i] = scissorRectangles[i];

            for (int i = 0; i < scissorCount; ++i)
            {
                _currentScissorsSetBuffer[4 * i] = scissorRectangles[i].X;
                _currentScissorsSetBuffer[4 * i + 1] = scissorRectangles[i].Y;
                _currentScissorsSetBuffer[4 * i + 2] = scissorRectangles[i].Width;
                _currentScissorsSetBuffer[4 * i + 3] = scissorRectangles[i].Height;
            }

            GL.ScissorArray(0, scissorCount, _currentScissorsSetBuffer);
#endif
        }

        private void UpdateScissor(Rectangle scissorRect)
        {
            GL.Scissor(scissorRect.Left, scissorRect.Top, scissorRect.Width, scissorRect.Height);
        }

        /// <summary>
        /// Sets a shader resource view to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="shaderResourceView">The shader resource view.</param>
        internal void SetShaderResourceView(ShaderStage stage, int slot, GraphicsResource shaderResourceView)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            if (textures[slot] != shaderResourceView)
            {
                textures[slot] = shaderResourceView as Texture;
            }
        }

        /// <inheritdoc/>
        public void SetStreamTargets(params Buffer[] buffers)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets an unordered access view to the shader pipeline.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="slot">The slot.</param>
        /// <param name="unorderedAccessView">The unordered access view.</param>
        /// <exception cref="System.ArgumentException">Invalid stage.;stage</exception>
        internal void SetUnorderedAccessView(ShaderStage stage, int slot, GraphicsResource unorderedAccessView)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            if (stage != ShaderStage.Compute)
                throw new ArgumentException("Invalid stage.", nameof(stage));

            throw new NotImplementedException();
        }

        internal void SetupTargets()
        {
            if (needUpdateFBO)
            {
                boundFBO = GraphicsDevice.FindOrCreateFBO(boundDepthStencilBuffer, boundRenderTargets);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);

            // TODO: support multiple viewports and scissors?
            UpdateViewport(viewports[0]);
            UpdateScissor(currentScissorRectangles[0]);
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            newPipelineState = pipelineState ?? GraphicsDevice.DefaultPipelineState;
        }

        public void SetVertexBuffer(int index, Buffer buffer, int offset, int stride)
        {
            var newVertexBuffer = new VertexBufferView(buffer, offset, stride);
            if (vertexBuffers[index] != newVertexBuffer)
            {
                vboDirty = true;
                vertexBuffers[index] = newVertexBuffer;
            }
        }

        public void SetIndexBuffer(Buffer buffer, int offset, bool is32bits)
        {
            var newIndexBuffer = new IndexBufferView(buffer, offset, is32bits);
            if (indexBuffer != newIndexBuffer)
            {
                // Setup index buffer
                indexBuffer = newIndexBuffer;
            }
        }

        public void ResourceBarrierTransition(GraphicsResource resource, GraphicsResourceState newState)
        {
            // Nothing to do
        }

        public void SetDescriptorSets(int index, DescriptorSet[] descriptorSets)
        {
            for (int i = 0; i < descriptorSets.Length; ++i)
            {
                currentDescriptorSets[index++] = descriptorSets[i];
            }
        }

        public void SetStencilReference(int stencilReference)
        {
            NewStencilReference = stencilReference;
        }

        private void SetViewportImpl()
        {
            if (!viewportDirty)
                return;

            viewportDirty = false;

#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            // TODO: Check all non-empty viewports are identical and match what is active in FBO!
            UpdateViewport(viewports[0]);
#else
            UpdateViewports();
#endif
        }

        private void UpdateViewport(Viewport viewport)
        {
            GL.DepthRange(viewport.MinDepth, viewport.MaxDepth);
            GL.Viewport((int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height);
        }

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private void UpdateViewports()
        {
            int nbViewports = viewports.Length;
            for (int i = 0; i < nbViewports; ++i)
            {
                var currViewport = viewports[i];
                _currentViewportsSetBuffer[4 * i] = currViewport.X;
                _currentViewportsSetBuffer[4 * i + 1] = currViewport.Y;
                _currentViewportsSetBuffer[4 * i + 2] = currViewport.Width;
                _currentViewportsSetBuffer[4 * i + 3] = currViewport.Height;
            }
            GL.DepthRange(viewports[0].MinDepth, viewports[0].MaxDepth);
            GL.ViewportArray(0, nbViewports, _currentViewportsSetBuffer);
        }
#endif

        public void UnmapSubresource(MappedResource unmapped)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            var texture = unmapped.Resource as Texture;
            if (texture != null)
            {
                if (texture.Description.Usage == GraphicsResourceUsage.Staging)
                {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    // unmapping on OpenGL ES 2 means doing nothing since the buffer is on the CPU memory
                    if (!GraphicsDevice.IsOpenGLES2)
#endif
                    {
                        GL.BindBuffer(BufferTarget.PixelPackBuffer, texture.PixelBufferObjectId);
                        GL.UnmapBuffer(BufferTarget.PixelPackBuffer);
                        GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
                    }
                }
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                else if (!GraphicsDevice.IsOpenGLES2 && texture.Description.Usage == GraphicsResourceUsage.Dynamic)
#else
                else if (texture.Description.Usage == GraphicsResourceUsage.Dynamic)
#endif
                {
                    GL.BindBuffer(BufferTarget.PixelUnpackBuffer, texture.PixelBufferObjectId);
                    GL.UnmapBuffer(BufferTarget.PixelUnpackBuffer);

                    GL.BindTexture(texture.Target, texture.ResourceId);

                    // Bind buffer to texture
                    switch (texture.Target)
                    {
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                        case TextureTarget.Texture1D:
                            GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, texture.Width, texture.FormatGl, texture.Type, IntPtr.Zero);
                            break;
#endif
                        case TextureTarget.Texture2D:
                            GL.TexSubImage2D(GraphicsDevice.TextureTargetTexture2D, 0, 0, 0, texture.Width, texture.Height, texture.FormatGl, texture.Type, IntPtr.Zero);
                            break;
                        case TextureTarget.Texture3D:
                            GL.TexSubImage3D(GraphicsDevice.TextureTargetTexture3D, 0, 0, 0, 0, texture.Width, texture.Height, texture.Depth, texture.FormatGl, texture.Type, IntPtr.Zero);
                            break;
                        default:
                            throw new NotSupportedException("Invalid texture target: " + texture.Target);
                    }
                    GL.BindTexture(texture.Target, 0);
                    boundTextures[0] = null;
                    GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
                }
                else
                {
                    throw new NotSupportedException("Not supported mapper operation for Usage: " + texture.Description.Usage);
                }
            }
            else
            {
                var buffer = unmapped.Resource as Buffer;
                if (buffer != null)
                {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (GraphicsDevice.IsOpenGLES2 || buffer.StagingData != IntPtr.Zero)
#else
                    if (buffer.StagingData != IntPtr.Zero)
#endif
                    {
                        // Only buffer with StagingData (fake cbuffer) could be mapped
                        if (buffer.StagingData == IntPtr.Zero)
                            throw new InvalidOperationException();

                        // Is it a real buffer? (fake cbuffer have no real GPU counter-part in OpenGL ES 2.0
                        if (buffer.ResourceId != 0)
                        {
                            //UnbindVertexArrayObject();
                            GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                            GL.BufferSubData(buffer.bufferTarget, (IntPtr)unmapped.OffsetInBytes, (IntPtr)unmapped.SizeInBytes, unmapped.DataBox.DataPointer);
                        }
                    }
                    else
                    {
                        //UnbindVertexArrayObject();
                        GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                        GL.UnmapBuffer(buffer.bufferTarget);
                    }
                }
                else // neither texture nor buffer
                {
                    throw new NotImplementedException("UnmapSubresource not implemented for type " + unmapped.Resource.GetType());
                }
            }
        }

        public void UnsetReadWriteBuffers()
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
        }

        public void UnsetRenderTargets()
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif

            SetRenderTargets(null, null);
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            var buffer = resource as Buffer;
            if (buffer != null)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (buffer.StagingData != IntPtr.Zero)
                {
                    // Specific case for constant buffers
                    SiliconStudio.Core.Utilities.CopyMemory(buffer.StagingData, databox.DataPointer, buffer.Description.SizeInBytes);
                    return;
                }
#endif

                //UnbindVertexArrayObject();

                GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                GL.BufferData(buffer.bufferTarget, (IntPtr)buffer.Description.SizeInBytes, databox.DataPointer, buffer.bufferUsageHint);
            }
            else
            {
                var texture = resource as Texture;
                if (texture != null)
                {
                    if (activeTexture != 0)
                    {
                        activeTexture = 0;
                        GL.ActiveTexture(TextureUnit.Texture0);
                    }

                    // TODO: Handle pitchs
                    // TODO: handle other texture formats
                    var desc = texture.Description;
                    GL.BindTexture(TextureTarget.Texture2D, texture.ResourceId);
                    boundTextures[0] = null; // bound active texture 0 has changed
                    GL.TexImage2D(GraphicsDevice.TextureTargetTexture2D, subResourceIndex, (PixelInternalFormat_TextureComponentCount)texture.InternalFormat, desc.Width, desc.Height, 0, texture.FormatGl, texture.Type, databox.DataPointer);
                }
                else // neither texture nor buffer
                {
                    throw new NotImplementedException("UpdateSubresource not implemented for type " + resource.GetType());
                }
            }
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox, ResourceRegion region)
        {
#if DEBUG
            GraphicsDevice.EnsureContextActive();
#endif
            var texture = resource as Texture;

            if (texture != null)
            {
                var width = region.Right - region.Left;
                var height = region.Bottom - region.Top;

                // determine the opengl read Unpack Alignment
                var packAlignment = 0;
                if ((databox.RowPitch & 1) != 0)
                {
                    if (databox.RowPitch == width)
                        packAlignment = 1;
                }
                else if ((databox.RowPitch & 2) != 0)
                {
                    var diff = databox.RowPitch - width;
                    if (diff >= 0 && diff < 2)
                        packAlignment = 2;
                }
                else if ((databox.RowPitch & 4) != 0)
                {
                    var diff = databox.RowPitch - width;
                    if (diff >= 0 && diff < 4)
                        packAlignment = 4;
                }
                else if ((databox.RowPitch & 8) != 0)
                {
                    var diff = databox.RowPitch - width;
                    if (diff >= 0 && diff < 8)
                        packAlignment = 8;
                }
                else if (databox.RowPitch == width)
                {
                    packAlignment = 4;
                }
                if (packAlignment == 0)
                    throw new NotImplementedException("The data box RowPitch is not compatible with the region width. This requires additional copy to be implemented.");

                // change the Unpack Alignment
                int previousPackAlignment;
                GL.GetInteger(GetPName.UnpackAlignment, out previousPackAlignment);
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, packAlignment);

                if (activeTexture != 0)
                {
                    activeTexture = 0;
                    GL.ActiveTexture(TextureUnit.Texture0);
                }

                // Update the texture region
                GL.BindTexture(texture.Target, texture.resourceId);
                GL.TexSubImage2D((TextureTarget_TextureTarget2d)texture.Target, subResourceIndex, region.Left, region.Top, width, height, texture.FormatGl, texture.Type, databox.DataPointer);
                boundTextures[0] = null; // bound active texture 0 has changed

                // reset the Unpack Alignment
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, previousPackAlignment);
            }
        }

        struct VertexBufferView
        {
            public readonly Buffer Buffer;
            public readonly int Offset;
            public readonly int Stride;

            public VertexBufferView(Buffer buffer, int offset, int stride)
            {
                Buffer = buffer;
                Offset = offset;
                Stride = stride;
            }

            public static bool operator ==(VertexBufferView left, VertexBufferView right)
            {
                return Equals(left.Buffer, right.Buffer) && left.Offset == right.Offset && left.Stride == right.Stride;
            }

            public static bool operator !=(VertexBufferView left, VertexBufferView right)
            {
                return !(left == right);
            }
        }

        struct IndexBufferView
        {
            public readonly Buffer Buffer;
            public readonly int Offset;
            public readonly DrawElementsType Type;
            public readonly int ElementSize;

            public IndexBufferView(Buffer buffer, int offset, bool is32Bits)
            {
                Buffer = buffer;
                Offset = offset;
                Type = is32Bits ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort;
                ElementSize = is32Bits ? 4 : 2;
            }

            public static bool operator ==(IndexBufferView left, IndexBufferView right)
            {
                return Equals(left.Buffer, right.Buffer) && left.Offset == right.Offset && left.Type == right.Type && left.ElementSize == right.ElementSize;
            }

            public static bool operator !=(IndexBufferView left, IndexBufferView right)
            {
                return !(left == right);
            }
        }
    }
}

#endif