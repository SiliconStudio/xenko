// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D
using System;

using SharpDX.DXGI;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;
using SharpDX.Mathematics.Interop;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class GraphicsDevice
    {
        private bool simulateReset = false;
        private const int ConstantBufferCount = SharpDX.Direct3D11.CommonShaderStage.ConstantBufferApiSlotCount; // 14 actually
        private const int SamplerStateCount = SharpDX.Direct3D11.CommonShaderStage.SamplerSlotCount;
        private const int ShaderResourceViewCount = SharpDX.Direct3D11.CommonShaderStage.InputResourceSlotCount;
        private const int SimultaneousRenderTargetCount = SharpDX.Direct3D11.OutputMergerStage.SimultaneousRenderTargetCount;
        private const int StageCount = 6;
        private const int UnorderedAcccesViewCount = SharpDX.Direct3D11.ComputeShaderStage.UnorderedAccessViewSlotCount;
        private const GraphicsPlatform GraphicPlatform = GraphicsPlatform.Direct3D11;

        private readonly Buffer[] constantBuffers = new Buffer[StageCount*ConstantBufferCount];
        private readonly SharpDX.Direct3D11.RenderTargetView[] currentRenderTargetViews = new SharpDX.Direct3D11.RenderTargetView[SimultaneousRenderTargetCount];
        private readonly SamplerState[] samplerStates = new SamplerState[StageCount*SamplerStateCount];
        private readonly SharpDX.Direct3D11.CommonShaderStage[] shaderStages = new SharpDX.Direct3D11.CommonShaderStage[StageCount];
        private readonly GraphicsResourceBase[] unorderedAccessViews = new GraphicsResourceBase[UnorderedAcccesViewCount]; // Only CS

        private SharpDX.Direct3D11.Device nativeDevice;
        private SharpDX.Direct3D11.DeviceContext nativeDeviceContext;
        private SharpDX.Direct3D11.UserDefinedAnnotation nativeDeviceProfiler;
        private SharpDX.Direct3D11.InputAssemblerStage inputAssembler;
        private SharpDX.Direct3D11.OutputMergerStage outputMerger;

        private SharpDX.Direct3D11.DeviceCreationFlags creationFlags;
        private EffectInputSignature currentEffectInputSignature;

        private PipelineState defaultPipelineState;

        private PipelineState newPipelineState;
        private PipelineState currentPipelineState;

        private DescriptorSet[] currentDescriptorSets = new DescriptorSet[32];

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class using the default GraphicsAdapter
        /// and the Level10 <see cref="GraphicsProfile" />.
        /// </summary>
        /// <param name="device">The device.</param>
        private GraphicsDevice(GraphicsDevice device)
        {
            RootDevice = device;
            Adapter = device.Adapter;
            creationFlags = device.creationFlags;
            Features = device.Features;
            sharedDataPerDevice = device.sharedDataPerDevice;
            nativeDevice = device.NativeDevice;
            nativeDeviceContext = new SharpDX.Direct3D11.DeviceContext(NativeDevice).DisposeBy(this);
            nativeDeviceProfiler = SharpDX.ComObject.QueryInterfaceOrNull<SharpDX.Direct3D11.UserDefinedAnnotation>(nativeDeviceContext.NativePointer);
            isDeferred = true;
            IsDebugMode = device.IsDebugMode;
            if (IsDebugMode)
            {
                GraphicsResourceBase.SetDebugName(device, nativeDeviceContext, "DeferredContext");
            }
            NeedWorkAroundForUpdateSubResource = !Features.HasDriverCommandLists;

            primitiveQuad = new PrimitiveQuad(this).DisposeBy(this);

            InitializeStages();
        }

        // Used by Texture.SetData

        /// <summary>
        ///     Gets the status of this device.
        /// </summary>
        /// <value>The graphics device status.</value>
        public GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get
            {
                if (simulateReset)
                {
                    simulateReset = false;
                    return GraphicsDeviceStatus.Reset;
                }

                var result = NativeDevice.DeviceRemovedReason;
                if (result == SharpDX.DXGI.ResultCode.DeviceRemoved)
                {
                    return GraphicsDeviceStatus.Removed;
                }

                if (result == SharpDX.DXGI.ResultCode.DeviceReset)
                {
                    return GraphicsDeviceStatus.Reset;
                }

                if (result == SharpDX.DXGI.ResultCode.DeviceHung)
                {
                    return GraphicsDeviceStatus.Hung;
                }

                if (result == SharpDX.DXGI.ResultCode.DriverInternalError)
                {
                    return GraphicsDeviceStatus.InternalError;
                }

                if (result == SharpDX.DXGI.ResultCode.InvalidCall)
                {
                    return GraphicsDeviceStatus.InvalidCall;
                }

                if (result.Code < 0)
                {
                    return GraphicsDeviceStatus.Reset;
                }

                return GraphicsDeviceStatus.Normal;
            }
        }

        /// <summary>
        ///     Gets the native device.
        /// </summary>
        /// <value>The native device.</value>
        internal SharpDX.Direct3D11.Device NativeDevice
        {
            get
            {
                return nativeDevice;
            }
        }

        /// <summary>
        /// Gets the native device context.
        /// </summary>
        /// <value>The native device context.</value>
        internal SharpDX.Direct3D11.DeviceContext NativeDeviceContext
        {
            get
            {
                return nativeDeviceContext;
            }
        }

        /// <summary>
        /// Sets the type of the primitive.
        /// </summary>
        /// <value>The type of the primitive.</value>
        private PrimitiveType PrimitiveType
        {
            set
            {
                inputAssembler.PrimitiveTopology = (SharpDX.Direct3D.PrimitiveTopology)value;
            }
        }

        public void ApplyPlatformSpecificParams(Effect effect)
        {
        }

        /// <summary>
        ///     Marks context as active on the current thread.
        /// </summary>
        public void Begin()
        {
            FrameTriangleCount = 0;
            FrameDrawCalls = 0;
        }

        /// <summary>
        /// Begins profiling.
        /// </summary>
        /// <param name="profileColor">Color of the profile.</param>
        /// <param name="name">The name.</param>
        public unsafe void BeginProfile(Color4 profileColor, string name)
        {
            if (nativeDeviceProfiler != null)
            {
                nativeDeviceProfiler.BeginEvent(name);
            }
        }

        /// <summary>
        /// Clears the specified depth stencil buffer. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <param name="options">The options.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="stencil">The stencil.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public void Clear(Texture depthStencilBuffer, DepthStencilClearOptions options, float depth = 1, byte stencil = 0)
        {
            if (depthStencilBuffer == null) throw new ArgumentNullException("depthStencilBuffer");

            var flags = ((options & DepthStencilClearOptions.DepthBuffer) != 0) ? SharpDX.Direct3D11.DepthStencilClearFlags.Depth : 0;

            // Check that the DepthStencilBuffer has a Stencil if Clear Stencil is requested
            if ((options & DepthStencilClearOptions.Stencil) != 0)
            {
                if (!depthStencilBuffer.HasStencil)
                    throw new InvalidOperationException(string.Format(FrameworkResources.NoStencilBufferForDepthFormat, depthStencilBuffer.ViewFormat));
                flags |= SharpDX.Direct3D11.DepthStencilClearFlags.Stencil;
            }

            NativeDeviceContext.ClearDepthStencilView(depthStencilBuffer.NativeDepthStencilView, flags, depth, stencil);
        }

        /// <summary>
        /// Clears the specified render target. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="System.ArgumentNullException">renderTarget</exception>
        public unsafe void Clear(Texture renderTarget, Color4 color)
        {
            if (renderTarget == null) throw new ArgumentNullException("renderTarget");

            NativeDeviceContext.ClearRenderTargetView(renderTarget.NativeRenderTargetView, *(RawColor4*)&color);
        }

        /// <summary>
        /// Clears a read-write Buffer. This buffer must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;buffer</exception>
        public unsafe void ClearReadWrite(Buffer buffer, Vector4 value)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.NativeUnorderedAccessView == null) throw new ArgumentException("Expecting buffer supporting UAV", "buffer");

            NativeDeviceContext.ClearUnorderedAccessView(buffer.NativeUnorderedAccessView, *(RawVector4*)&value);
        }

        /// <summary>
        /// Clears a read-write Buffer. This buffer must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;buffer</exception>
        public unsafe void ClearReadWrite(Buffer buffer, Int4 value)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.NativeUnorderedAccessView == null) throw new ArgumentException("Expecting buffer supporting UAV", "buffer");

            NativeDeviceContext.ClearUnorderedAccessView(buffer.NativeUnorderedAccessView, *(RawInt4*)&value);
        }

        /// <summary>
        /// Clears a read-write Buffer. This buffer must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;buffer</exception>
        public unsafe void ClearReadWrite(Buffer buffer, UInt4 value)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.NativeUnorderedAccessView == null) throw new ArgumentException("Expecting buffer supporting UAV", "buffer");

            NativeDeviceContext.ClearUnorderedAccessView(buffer.NativeUnorderedAccessView, *(RawInt4*)&value);
        }

        /// <summary>
        /// Clears a read-write Texture. This texture must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">texture</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;texture</exception>
        public unsafe void ClearReadWrite(Texture texture, Vector4 value)
        {
            if (texture == null) throw new ArgumentNullException("texture");
            if (texture.NativeUnorderedAccessView == null) throw new ArgumentException("Expecting buffer supporting UAV", "texture");

            NativeDeviceContext.ClearUnorderedAccessView(texture.NativeUnorderedAccessView, *(RawVector4*)&value);
        }

        /// <summary>
        /// Clears a read-write Texture. This texture must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">texture</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;texture</exception>
        public unsafe void ClearReadWrite(Texture texture, Int4 value)
        {
            if (texture == null) throw new ArgumentNullException("texture");
            if (texture.NativeUnorderedAccessView == null) throw new ArgumentException("Expecting buffer supporting UAV", "texture");

            NativeDeviceContext.ClearUnorderedAccessView(texture.NativeUnorderedAccessView, *(RawInt4*)&value);
        }

        /// <summary>
        /// Clears a read-write Texture. This texture must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">texture</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;texture</exception>
        public unsafe void ClearReadWrite(Texture texture, UInt4 value)
        {
            if (texture == null) throw new ArgumentNullException("texture");
            if (texture.NativeUnorderedAccessView == null) throw new ArgumentException("Expecting buffer supporting UAV", "texture");

            NativeDeviceContext.ClearUnorderedAccessView(texture.NativeUnorderedAccessView, *(RawInt4*)&value);
        }

        private void ClearStateImpl()
        {
            NativeDeviceContext.ClearState();

            for (int i = 0; i < samplerStates.Length; ++i)
                samplerStates[i] = null;
            for (int i = 0; i < constantBuffers.Length; ++i)
                constantBuffers[i] = null;
            for (int i = 0; i < unorderedAccessViews.Length; ++i)
                unorderedAccessViews[i] = null;
            for (int i = 0; i < currentRenderTargetViews.Length; i++)
                currentRenderTargetViews[i] = null;

            currentEffectInputSignature = null;
            CurrentEffect = null;

            currentPipelineState = defaultPipelineState;
            newPipelineState = defaultPipelineState;
        }

        public void Copy(GraphicsResource source, GraphicsResource destination)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            NativeDeviceContext.CopyResource(source.NativeResource, destination.NativeResource);
        }

        public void CopyMultiSample(Texture sourceMsaaTexture, int sourceSubResource, Texture destTexture, int destSubResource, PixelFormat format = PixelFormat.None)
        {
            if (sourceMsaaTexture == null) throw new ArgumentNullException("sourceMsaaTexture");
            if (destTexture == null) throw new ArgumentNullException("destTexture");
            if (!sourceMsaaTexture.IsMultiSample) throw new ArgumentOutOfRangeException("sourceMsaaTexture", "Source texture is not a MSAA texture");

            NativeDeviceContext.ResolveSubresource(sourceMsaaTexture.NativeResource, sourceSubResource, destTexture.NativeResource, destSubResource, (Format)(format == PixelFormat.None ? destTexture.Format : format));
        }

        public void CopyRegion(GraphicsResource source, int sourceSubresource, ResourceRegion? sourecRegion, GraphicsResource destination, int destinationSubResource, int dstX = 0, int dstY = 0, int dstZ = 0)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");

            var nullableSharpDxRegion = new SharpDX.Direct3D11.ResourceRegion?();

            if (sourecRegion.HasValue)
            {
                var value = sourecRegion.Value;
                nullableSharpDxRegion = new SharpDX.Direct3D11.ResourceRegion(value.Left, value.Top, value.Front, value.Right, value.Bottom, value.Back);
            }

            NativeDeviceContext.CopySubresourceRegion(source.NativeResource, sourceSubresource, nullableSharpDxRegion, destination.NativeResource, destinationSubResource, dstX, dstY, dstZ);
        }

        /// <inheritdoc />
        public void CopyCount(Buffer sourceBuffer, Buffer destBuffer, int offsetInBytes)
        {
            if (sourceBuffer == null) throw new ArgumentNullException("sourceBuffer");
            if (destBuffer == null) throw new ArgumentNullException("destBuffer");
            NativeDeviceContext.CopyStructureCount(destBuffer.NativeBuffer, offsetInBytes, sourceBuffer.NativeUnorderedAccessView);
        }

        /// <inheritdoc />
        public void Dispatch(int threadCountX, int threadCountY, int threadCountZ)
        {
            NativeDeviceContext.Dispatch(threadCountX, threadCountY, threadCountZ);
        }

        /// <summary>
        /// Dispatches the specified indirect buffer.
        /// </summary>
        /// <param name="indirectBuffer">The indirect buffer.</param>
        /// <param name="offsetInBytes">The offset information bytes.</param>
        public void Dispatch(Buffer indirectBuffer, int offsetInBytes)
        {
            if (indirectBuffer == null) throw new ArgumentNullException("indirectBuffer");
            NativeDeviceContext.DispatchIndirect(indirectBuffer.NativeBuffer, offsetInBytes);
        }

        /// <summary>
        /// Draw non-indexed, non-instanced primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="vertexCount">Number of vertices to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex, which is usually an offset in a vertex buffer; it could also be used as the first vertex id generated for a shader parameter marked with the <strong>SV_TargetId</strong> system-value semantic.</param>
        public void Draw(PrimitiveType primitiveType, int vertexCount, int startVertexLocation = 0)
        {
            PrepareDraw(primitiveType);
            
            NativeDeviceContext.Draw(vertexCount, startVertexLocation);

            FrameTriangleCount += (uint)vertexCount;
            FrameDrawCalls++;
        }

        /// <summary>
        /// Draw geometry of an unknown size.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        public void DrawAuto(PrimitiveType primitiveType)
        {
            PrepareDraw(primitiveType);

            NativeDeviceContext.DrawAuto();

            FrameDrawCalls++;
        }

        /// <summary>
        /// Draw indexed, non-instanced primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="indexCount">Number of indices to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        public void DrawIndexed(PrimitiveType primitiveType, int indexCount, int startIndexLocation = 0, int baseVertexLocation = 0)
        {
            PrepareDraw(primitiveType);

            NativeDeviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);

            FrameDrawCalls++;
            FrameTriangleCount += (uint)indexCount;
        }

        /// <summary>
        /// Draw indexed, instanced primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="indexCountPerInstance">Number of indices read from the index buffer for each instance.</param>
        /// <param name="instanceCount">Number of instances to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        /// <param name="startInstanceLocation">A value added to each index before reading per-instance data from a vertex buffer.</param>
        public void DrawIndexedInstanced(PrimitiveType primitiveType, int indexCountPerInstance, int instanceCount, int startIndexLocation = 0, int baseVertexLocation = 0, int startInstanceLocation = 0)
        {
            PrepareDraw(primitiveType);

            NativeDeviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

            FrameDrawCalls++;
            FrameTriangleCount += (uint)(indexCountPerInstance * instanceCount);
        }

        /// <summary>
        /// Draw indexed, instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="argumentsBuffer">A buffer containing the GPU generated primitives.</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawIndexedInstanced(PrimitiveType primitiveType, Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {
            if (argumentsBuffer == null) throw new ArgumentNullException("argumentsBuffer");

            PrepareDraw(primitiveType);

            NativeDeviceContext.DrawIndexedInstancedIndirect(argumentsBuffer.NativeBuffer, alignedByteOffsetForArgs);

            FrameDrawCalls++;
        }

        /// <summary>
        /// Draw non-indexed, instanced primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="vertexCountPerInstance">Number of vertices to draw.</param>
        /// <param name="instanceCount">Number of instances to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex.</param>
        /// <param name="startInstanceLocation">A value added to each index before reading per-instance data from a vertex buffer.</param>
        public void DrawInstanced(PrimitiveType primitiveType, int vertexCountPerInstance, int instanceCount, int startVertexLocation = 0, int startInstanceLocation = 0)
        {
            PrepareDraw(primitiveType);

            NativeDeviceContext.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);

            FrameDrawCalls++;
            FrameTriangleCount += (uint)(vertexCountPerInstance * instanceCount);
        }

        /// <summary>
        /// Draw instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="argumentsBuffer">An arguments buffer</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawInstanced(PrimitiveType primitiveType, Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {
            if (argumentsBuffer == null) throw new ArgumentNullException("argumentsBuffer");

            PrepareDraw(primitiveType);

            NativeDeviceContext.DrawIndexedInstancedIndirect(argumentsBuffer.NativeBuffer, alignedByteOffsetForArgs);

            FrameDrawCalls++;
        }

        /// <summary>
        /// Enables profiling.
        /// </summary>
        /// <param name="enabledFlag">if set to <c>true</c> [enabled flag].</param>
        public void EnableProfile(bool enabledFlag)
        {
        }

        /// <summary>
        ///     Unmarks context as active on the current thread.
        /// </summary>
        public void End()
        {
        }

        /// <summary>
        /// Ends profiling.
        /// </summary>
        public void EndProfile()
        {
            if (nativeDeviceProfiler != null)
            {
                nativeDeviceProfiler.EndEvent();
            }
        }

        /// <summary>
        /// Executes a deferred command list.
        /// </summary>
        /// <param name="commandList">The deferred command list.</param>
        public void ExecuteCommandList(ICommandList commandList)
        {
            if (commandList == null) throw new ArgumentNullException("commandList");

            NativeDeviceContext.ExecuteCommandList(((CommandList)commandList).NativeCommandList, false);
            commandList.Release();
        }

        /// <summary>
        /// Finishes a deffered command list.
        /// </summary>
        /// <returns>A deferred command list.</returns>
        public ICommandList FinishCommandList()
        {
            return new CommandList(NativeDeviceContext.FinishCommandList(false));
        }

        /// <summary>
        /// Maps a subresource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="subResourceIndex">Index of the sub resource.</param>
        /// <param name="mapMode">The map mode.</param>
        /// <param name="doNotWait">if set to <c>true</c> this method will return immediately if the resource is still being used by the GPU for writing. Default is false</param>
        /// <param name="offsetInBytes">The offset information in bytes.</param>
        /// <param name="lengthInBytes">The length information in bytes.</param>
        /// <returns>Pointer to the sub resource to map.</returns>
        public unsafe MappedResource MapSubresource(GraphicsResource resource, int subResourceIndex, MapMode mapMode, bool doNotWait = false, int offsetInBytes = 0, int lengthInBytes = 0)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            SharpDX.DataBox dataBox = NativeDeviceContext.MapSubresource(resource.NativeResource, subResourceIndex, (SharpDX.Direct3D11.MapMode)mapMode, doNotWait ? SharpDX.Direct3D11.MapFlags.DoNotWait : SharpDX.Direct3D11.MapFlags.None);
            var databox = *(DataBox*)Interop.Cast(ref dataBox);
            if (!dataBox.IsEmpty)
            {
                databox.DataPointer = (IntPtr)((byte*)databox.DataPointer + offsetInBytes);
            }
            return new MappedResource(resource, subResourceIndex, databox);
        }

        /// <summary>
        /// Creates a new deferred device used for multithread deferred rendering.
        /// </summary>
        /// <returns>GraphicsDevice.</returns>
        public GraphicsDevice NewDeferred()
        {
            return new GraphicsDevice(RootDevice);
        }

        /// <summary>
        /// Unbinds all depth-stencil buffer and render targets from the output-merger stage.
        /// </summary>
        private void ResetTargetsImpl()
        {
            for (int i = 0; i < currentRenderTargetViews.Length; i++)
                currentRenderTargetViews[i] = null;
            outputMerger.ResetTargets();
        }

        /// <summary>
        /// Set the blend state of the output-merger stage. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="blendState">a blend-state</param>
        /// <param name="blendFactor">Blend factors, one for each RGBA component. This requires a blend state object that specifies the <see cref="Blend.BlendFactor" /></param>
        /// <param name="multiSampleMask">32-bit sample coverage. The default value is 0xffffffff.</param>
        private void SetBlendStateImpl(BlendState blendState, Color4 blendFactor, int multiSampleMask = -1)
        {
            if (blendState == null)
            {
                NativeDeviceContext.OutputMerger.SetBlendState(null, ColorHelper.Convert(blendFactor), multiSampleMask);
            }
            else
            {
                NativeDeviceContext.OutputMerger.SetBlendState((SharpDX.Direct3D11.BlendState)blendState.NativeDeviceChild, ColorHelper.Convert(blendFactor), multiSampleMask);
            }
        }

        /// <summary>
        /// Sets the depth-stencil state of the output-merger stage. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilState">a depth-stencil state</param>
        /// <param name="stencilReference">Reference value to perform against when doing a depth-stencil test.</param>
        private void SetDepthStencilStateImpl(DepthStencilState depthStencilState, int stencilReference = 0)
        {
            NativeDeviceContext.OutputMerger.SetDepthStencilState(depthStencilState != null ? (SharpDX.Direct3D11.DepthStencilState)depthStencilState.NativeDeviceChild : null, stencilReference);
        }

        /// <summary>
        /// Set the <strong>rasterizer state</strong> for the rasterizer stage of the pipeline. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="rasterizerState">The rasterizser state to set on this device.</param>
        private void SetRasterizerStateImpl(RasterizerState rasterizerState)
        {
            NativeDeviceContext.Rasterizer.State = rasterizerState != null ? (SharpDX.Direct3D11.RasterizerState)rasterizerState.NativeDeviceChild : null;
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a set of render targets to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <param name="renderTargets">The render targets.</param>
        /// <exception cref="System.ArgumentNullException">renderTargetViews</exception>
        private void SetDepthAndRenderTargetsImpl(Texture depthStencilBuffer, Texture[] renderTargets)
        {
            for (int i = 0; i < renderTargets.Length; i++)
                currentRenderTargetViews[i] = renderTargets[i] != null ? renderTargets[i].NativeRenderTargetView : null;
            outputMerger.SetTargets(depthStencilBuffer != null ? depthStencilBuffer.NativeDepthStencilView : null, currentRenderTargetViews.Length, currentRenderTargetViews);
        }

        /// <summary>
        /// Binds a single scissor rectangle to the rasterizer stage. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        /// <param name="right">The right.</param>
        /// <param name="bottom">The bottom.</param>
        public void SetScissorRectangles(int left, int top, int right, int bottom)
        {
            NativeDeviceContext.Rasterizer.SetScissorRectangle(left, top, right, bottom);
        }

        /// <summary>
        /// Binds a set of scissor rectangles to the rasterizer stage. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="scissorRectangles">The set of scissor rectangles to bind.</param>
        public unsafe void SetScissorRectangles(params Rectangle[] scissorRectangles)
        {
            if (scissorRectangles == null) throw new ArgumentNullException("scissorRectangles");
            var localScissorRectangles = new RawRectangle[scissorRectangles.Length];
            for (int i = 0; i < scissorRectangles.Length; i++)
            {
                localScissorRectangles[i] = new RawRectangle(scissorRectangles[i].X, scissorRectangles[i].Y, scissorRectangles[i].Right, scissorRectangles[i].Bottom);
            }
            NativeDeviceContext.Rasterizer.SetScissorRectangles(localScissorRectangles);
        }

        /// <summary>
        /// Sets the stream targets.
        /// </summary>
        /// <param name="buffers">The buffers.</param>
        public void SetStreamTargets(params Buffer[] buffers)
        {
            SharpDX.Direct3D11.StreamOutputBufferBinding[] streamOutputBufferBindings;

            if (buffers != null)
            {
                streamOutputBufferBindings = new SharpDX.Direct3D11.StreamOutputBufferBinding[buffers.Length];
                for (int i = 0; i < buffers.Length; ++i)
                    streamOutputBufferBindings[i].Buffer = buffers[i].NativeBuffer;
            }
            else
            {
                streamOutputBufferBindings = null;
            }

            NativeDeviceContext.StreamOutput.SetTargets(streamOutputBufferBindings);
        }

        /// <summary>
        ///     Gets or sets the 1st viewport. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <value>The viewport.</value>
        private unsafe void SetViewportImpl()
        {
            if (!needViewportUpdate)
                return;
            needViewportUpdate = false;

            fixed (Viewport* viewports = currentState.Viewports)
            {
                nativeDeviceContext.Rasterizer.SetViewports((RawViewportF*)viewports, currentState.Viewports.Length);
            }
        }

        public void UnmapSubresource(MappedResource unmapped)
        {
            NativeDeviceContext.UnmapSubresource(unmapped.Resource.NativeResource, unmapped.SubResourceIndex);
        }

        /// <summary>
        ///     Unsets the read/write buffers.
        /// </summary>
        public void UnsetReadWriteBuffers()
        {
            // TODO optimize it using SetUnorderedAccessViews
            for (int i = 0; i < UnorderedAcccesViewCount; i++)
            {
                SetUnorderedAccessView(ShaderStage.Compute, i, null);
            }
        }

        /// <summary>
        /// Unsets the render targets.
        /// </summary>
        public void UnsetRenderTargets()
        {
            NativeDeviceContext.OutputMerger.ResetTargets();
        }

        public void SimulateReset()
        {
            simulateReset = true;
        }

        /// <summary>
        ///     Sets a constant buffer to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="buffer">The constant buffer to set.</param>
        internal void SetConstantBuffer(ShaderStage stage, int slot, Buffer buffer)
        {
            if (stage == ShaderStage.None)
                throw new ArgumentException("Cannot use Stage.None", "stage");

            int stageIndex = (int)stage - 1;

            int slotIndex = stageIndex*ConstantBufferCount + slot;
            if (constantBuffers[slotIndex] != buffer)
            {
                constantBuffers[slotIndex] = buffer;
                shaderStages[stageIndex].SetConstantBuffer(slot, buffer != null ? buffer.NativeBuffer : null);
            }
        }

        /// <summary>
        ///     Sets a sampler state to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="samplerState">The sampler state to set.</param>
        internal void SetSamplerState(ShaderStage stage, int slot, SamplerState samplerState)
        {
            if (stage == ShaderStage.None)
                throw new ArgumentException("Cannot use Stage.None", "stage");
            int stageIndex = (int)stage - 1;

            int slotIndex = stageIndex*SamplerStateCount + slot;
            if (samplerStates[slotIndex] != samplerState)
            {
                samplerStates[slotIndex] = samplerState;
                shaderStages[stageIndex].SetSampler(slot, samplerState != null ? (SharpDX.Direct3D11.SamplerState)samplerState.NativeDeviceChild : null);
            }
        }

        /// <summary>
        ///     Sets a shader resource view to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="shaderResourceView">The shader resource view.</param>
        internal void SetShaderResourceView(ShaderStage stage, int slot, GraphicsResource shaderResourceView)
        {
            shaderStages[(int)stage - 1].SetShaderResource(slot, shaderResourceView != null ? shaderResourceView.NativeShaderResourceView : null);
        }

        /// <summary>
        ///     Sets an unordered access view to the shader pipeline.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="slot">The slot.</param>
        /// <param name="unorderedAccessView">The unordered access view.</param>
        /// <exception cref="System.ArgumentException">Invalid stage.;stage</exception>
        internal void SetUnorderedAccessView(ShaderStage stage, int slot, GraphicsResource unorderedAccessView)
        {
            if (stage != ShaderStage.Compute)
                throw new ArgumentException("Invalid stage.", "stage");

            NativeDeviceContext.ComputeShader.SetUnorderedAccessView(slot, unorderedAccessView != null ? unorderedAccessView.NativeUnorderedAccessView : null);
        }

        internal unsafe void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            NativeDeviceContext.UpdateSubresource(*(SharpDX.DataBox*)Interop.Cast(ref databox), resource.NativeResource, subResourceIndex);
        }

        internal unsafe void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox, ResourceRegion region)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            NativeDeviceContext.UpdateSubresource(*(SharpDX.DataBox*)Interop.Cast(ref databox), resource.NativeResource, subResourceIndex, *(SharpDX.Direct3D11.ResourceRegion*)Interop.Cast(ref region));
        }

        private void InitializeStages()
        {
            inputAssembler = nativeDeviceContext.InputAssembler;
            outputMerger = nativeDeviceContext.OutputMerger;
            shaderStages[(int)ShaderStage.Vertex - 1] = nativeDeviceContext.VertexShader;
            shaderStages[(int)ShaderStage.Hull - 1] = nativeDeviceContext.HullShader;
            shaderStages[(int)ShaderStage.Domain - 1] = nativeDeviceContext.DomainShader;
            shaderStages[(int)ShaderStage.Geometry - 1] = nativeDeviceContext.GeometryShader;
            shaderStages[(int)ShaderStage.Pixel - 1] = nativeDeviceContext.PixelShader;
            shaderStages[(int)ShaderStage.Compute - 1] = nativeDeviceContext.ComputeShader;
        }

        private void InitializeFactories()
        {
            
        }

        /// <summary>
        ///     Initializes the specified device.
        /// </summary>
        /// <param name="graphicsProfiles">The graphics profiles.</param>
        /// <param name="deviceCreationFlags">The device creation flags.</param>
        /// <param name="windowHandle">The window handle.</param>
        private void InitializePlatformDevice(GraphicsProfile[] graphicsProfiles, DeviceCreationFlags deviceCreationFlags, object windowHandle)
        {
            if (nativeDevice != null)
            {
                // Destroy previous device
                ReleaseDevice();
            }

            // Profiling is supported through pix markers
            IsProfilingSupported = true;

            // Map GraphicsProfile to D3D11 FeatureLevel
            SharpDX.Direct3D.FeatureLevel[] levels = graphicsProfiles.ToFeatureLevel();
            creationFlags = (SharpDX.Direct3D11.DeviceCreationFlags)deviceCreationFlags;

            // Create Device D3D11 with feature Level based on profile
            nativeDevice = new SharpDX.Direct3D11.Device(Adapter.NativeAdapter, creationFlags, levels);
            nativeDeviceContext = nativeDevice.ImmediateContext;
            if (IsDebugMode)
            {
                GraphicsResourceBase.SetDebugName(this, nativeDeviceContext, "ImmediateContext");
            }

            InitializeStages();
        }

        protected void DestroyPlatformDevice()
        {
            ReleaseDevice();
        }

        private void ReleaseDevice()
        {
            // Display D3D11 ref counting info
            ClearState();
            NativeDevice.ImmediateContext.Flush();
            NativeDevice.ImmediateContext.Dispose();

            if (IsDebugMode)
            {
                var deviceDebug = new SharpDX.Direct3D11.DeviceDebug(NativeDevice);
                deviceDebug.ReportLiveDeviceObjects(SharpDX.Direct3D11.ReportingLevel.Detail);
            }

            currentEffectInputSignature = null;
            nativeDevice.Dispose();
        }

        internal void OnDestroyed()
        {
        }

        /// <summary>
        ///     Prepares a draw call. This method is called before each Draw() method to setup the correct Primitive, InputLayout and VertexBuffers.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive.</param>
        /// <exception cref="System.InvalidOperationException">Cannot GraphicsDevice.Draw*() without an effect being previously applied with Effect.Apply() method</exception>
        private void PrepareDraw(PrimitiveType primitiveType)
        {
            if (CurrentEffect == null)
            {
                throw new InvalidOperationException("Cannot GraphicsDevice.Draw*() without an effect being previously applied with Effect.Apply() method");
            }

            // Setup the primitive type
            PrimitiveType = primitiveType;

            // Pipeline state
            if (newPipelineState != currentPipelineState)
            {
                newPipelineState.Apply(this, currentPipelineState);
                currentPipelineState = newPipelineState;
            }

            // Resources
            if (newPipelineState != null)
                newPipelineState.ResourceBinder.BindResources(this, currentDescriptorSets);

            SetViewportImpl();
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            newPipelineState = pipelineState ?? defaultPipelineState;
        }

        public void SetVertexBuffer(int index, Buffer buffer, int offset, int stride)
        {
            inputAssembler.SetVertexBuffers(index, new SharpDX.Direct3D11.VertexBufferBinding(buffer.NativeBuffer, stride, offset));
        }

        public void SetIndexBuffer(Buffer buffer, int offset, bool is32bits)
        {
            inputAssembler.SetIndexBuffer(buffer != null ? buffer.NativeBuffer : null, is32bits ? Format.R32_UInt : Format.R16_UInt, offset);
        }

        public void SetDescriptorSets(int index, DescriptorSet[] descriptorSets)
        {
            for (int i = 0; i < descriptorSets.Length; ++i)
            {
                currentDescriptorSets[index++] = descriptorSets[i];
            }
        }
    }
}
#endif
