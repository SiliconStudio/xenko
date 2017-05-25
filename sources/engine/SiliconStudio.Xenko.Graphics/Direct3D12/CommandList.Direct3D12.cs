// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
using System;
using System.Collections.Generic;
using System.Threading;
using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;
using SiliconStudio.Core.Mathematics;
using Utilities = SiliconStudio.Core.Utilities;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class CommandList
    {
        private DescriptorHeapCache srvHeap;
        private int srvHeapOffset = GraphicsDevice.SrvHeapSize;
        private DescriptorHeapCache samplerHeap;
        private int samplerHeapOffset = GraphicsDevice.SamplerHeapSize;

        private PipelineState boundPipelineState;
        private readonly DescriptorHeap[] descriptorHeaps = new DescriptorHeap[2];

        private readonly Dictionary<long, GpuDescriptorHandle> srvMapping = new Dictionary<long, GpuDescriptorHandle>();
        private readonly Dictionary<long, GpuDescriptorHandle> samplerMapping = new Dictionary<long, GpuDescriptorHandle>();

        internal readonly Queue<GraphicsCommandList> NativeCommandLists = new Queue<GraphicsCommandList>();

        private CompiledCommandList currentCommandList;

        private RawRectangle[] nativeScissorRectangles = new RawRectangle[MaxViewportAndScissorRectangleCount];

        public static CommandList New(GraphicsDevice device)
        {
            return new CommandList(device);
        }

        private CommandList(GraphicsDevice device) : base(device)
        {
            Reset();
        }

        private void ResetCommandList()
        {
            if (NativeCommandLists.Count > 0)
            {
                currentCommandList.NativeCommandList = NativeCommandLists.Dequeue();
                currentCommandList.NativeCommandList.Reset(currentCommandList.NativeCommandAllocator, null);
            }
            else
            {
                currentCommandList.NativeCommandList = GraphicsDevice.NativeDevice.CreateCommandList(CommandListType.Direct, currentCommandList.NativeCommandAllocator, null);
            }

            currentCommandList.NativeCommandList.SetDescriptorHeaps(2, descriptorHeaps);
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
            // Recycle heaps
            ResetSrvHeap(false);
            ResetSamplerHeap(false);

            // Available right now (NextFenceValue - 1)
            // TODO: Note that it won't be available right away because CommandAllocators is currently not using a PriorityQueue but a simple Queue
            if (currentCommandList.NativeCommandAllocator != null)
            {
                GraphicsDevice.CommandAllocators.RecycleObject(GraphicsDevice.NextFenceValue - 1, currentCommandList.NativeCommandAllocator);
                currentCommandList.NativeCommandAllocator = null;
            }

            if (currentCommandList.NativeCommandList != null)
            {
                NativeCommandLists.Enqueue(currentCommandList.NativeCommandList);
                currentCommandList.NativeCommandList = null;
            }

            while (NativeCommandLists.Count > 0)
            {
                NativeCommandLists.Dequeue().Dispose();
            }

            base.OnDestroyed();
        }

        public void Reset()
        {
            if (currentCommandList.Builder != null)
                return;

            ResetSrvHeap(true);
            ResetSamplerHeap(true);

            // Clear descriptor mappings
            srvMapping.Clear();
            samplerMapping.Clear();

            currentCommandList.Builder = this;
            currentCommandList.SrvHeaps = GraphicsDevice.DescriptorHeapLists.Acquire();
            currentCommandList.SamplerHeaps = GraphicsDevice.DescriptorHeapLists.Acquire();
            currentCommandList.StagingResources = GraphicsDevice.StagingResourceLists.Acquire();

            // Get a new allocator and unused command list
            currentCommandList.NativeCommandAllocator = GraphicsDevice.CommandAllocators.GetObject();
            ResetCommandList();

            boundPipelineState = null;

            gpuQueryProfiler = new GpuQueryProfiler(this);
        }

        /// <summary>
        /// Closes the command list for recording and returns an executable token.
        /// </summary>
        /// <returns>The executable command list.</returns>
        public CompiledCommandList Close()
        {
            currentCommandList.NativeCommandList.Close();

            // Staging resources not updated anymore
            foreach (var stagingResource in currentCommandList.StagingResources)
            {
                stagingResource.StagingBuilder = null;
            }

            // Recycle heaps
            ResetSrvHeap(false);
            ResetSamplerHeap(false);

            var result = currentCommandList;
            currentCommandList = default(CompiledCommandList);
            return result;
        }

        /// <summary>
        /// Closes and executes the command list.
        /// </summary>
        public void Flush()
        {
            GraphicsDevice.ExecuteCommandList(Close());
        }

        private void FlushInternal(bool wait)
        {
            var fenceValue = GraphicsDevice.ExecuteCommandListInternal(Close());

            if (wait)
                GraphicsDevice.WaitForFenceInternal(fenceValue);

            Reset();

            // Restore states
            if (boundPipelineState != null)
                SetPipelineState(boundPipelineState);
            currentCommandList.NativeCommandList.SetDescriptorHeaps(2, descriptorHeaps);
            SetRenderTargetsImpl(depthStencilBuffer, renderTargetCount, renderTargets);
        }

        private void ClearStateImpl()
        {
        }

        /// <summary>
        /// Unbinds all depth-stencil buffer and render targets from the output-merger stage.
        /// </summary>
        private void ResetTargetsImpl()
        {
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a set of render targets to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <param name="renderTargets">The render targets.</param>
        /// <exception cref="System.ArgumentNullException">renderTargetViews</exception>
        private void SetRenderTargetsImpl(Texture depthStencilBuffer, int renderTargetCount, Texture[] renderTargets)
        {
            var renderTargetHandles = new CpuDescriptorHandle[renderTargetCount];
            for (int i = 0; i < renderTargetHandles.Length; ++i)
            {
                renderTargetHandles[i] = renderTargets[i].NativeRenderTargetView;
            }
            currentCommandList.NativeCommandList.SetRenderTargets(renderTargetHandles, depthStencilBuffer?.NativeDepthStencilView);
        }

        /// <summary>
        /// Sets the stream targets.
        /// </summary>
        /// <param name="buffers">The buffers.</param>
        public void SetStreamTargets(params Buffer[] buffers)
        {
        }

        /// <summary>
        ///     Gets or sets the 1st viewport. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <value>The viewport.</value>
        private void SetViewportImpl()
        {
            if (!viewportDirty && !scissorsDirty)
                return;

            var viewport = viewports[0];
            if (viewportDirty)
            {
                currentCommandList.NativeCommandList.SetViewport(new RawViewportF { Width = viewport.Width, Height = viewport.Height, X = viewport.X, Y = viewport.Y, MinDepth = viewport.MinDepth, MaxDepth = viewport.MaxDepth });
                currentCommandList.NativeCommandList.SetScissorRectangles(new RawRectangle { Left = (int)viewport.X, Right = (int)viewport.X + (int)viewport.Width, Top = (int)viewport.Y, Bottom = (int)viewport.Y + (int)viewport.Height });
                viewportDirty = false;
            }

            if (boundPipelineState?.HasScissorEnabled ?? false)
            {
                if (scissorsDirty)
                {
                    // Use manual scissor
                    var scissor = scissors[0];
                    currentCommandList.NativeCommandList.SetScissorRectangles(new RawRectangle { Left = scissor.Left, Right = scissor.Right, Top = scissor.Top, Bottom = scissor.Bottom });
                }
            }
            else
            {
                // Use viewport
                // Always update, because either scissor or viewport was dirty and we use viewport size
                currentCommandList.NativeCommandList.SetScissorRectangles(new RawRectangle { Left = (int)viewport.X, Right = (int)viewport.X + (int)viewport.Width, Top = (int)viewport.Y, Bottom = (int)viewport.Y + (int)viewport.Height });
            }

            scissorsDirty = false;
        }

        /// <summary>
        ///     Prepares a draw call. This method is called before each Draw() method to setup the correct Primitive, InputLayout and VertexBuffers.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Cannot GraphicsDevice.Draw*() without an effect being previously applied with Effect.Apply() method</exception>
        private void PrepareDraw()
        {
            SetViewportImpl();
        }

        public void SetStencilReference(int stencilReference)
        {
            currentCommandList.NativeCommandList.StencilReference = stencilReference;
        }

        public void SetBlendFactor(Color4 blendFactor)
        {
            currentCommandList.NativeCommandList.BlendFactor = ColorHelper.ConvertToVector4(blendFactor);
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            if (boundPipelineState != pipelineState && pipelineState?.CompiledState != null)
            {
                // If scissor state changed, force a refresh
                scissorsDirty |= (boundPipelineState?.HasScissorEnabled ?? false) != (pipelineState?.HasScissorEnabled ?? false);

                currentCommandList.NativeCommandList.PipelineState = pipelineState.CompiledState;
                currentCommandList.NativeCommandList.SetGraphicsRootSignature(pipelineState.RootSignature);
                boundPipelineState = pipelineState;
                currentCommandList.NativeCommandList.PrimitiveTopology = pipelineState.PrimitiveTopology;
            }
        }

        public void SetVertexBuffer(int index, Buffer buffer, int offset, int stride)
        {
            currentCommandList.NativeCommandList.SetVertexBuffer(index, new VertexBufferView
            {
                BufferLocation = buffer.NativeResource.GPUVirtualAddress + offset,
                StrideInBytes = stride,
                SizeInBytes = buffer.SizeInBytes - offset
            });
        }

        public void SetIndexBuffer(Buffer buffer, int offset, bool is32bits)
        {
            currentCommandList.NativeCommandList.SetIndexBuffer(buffer != null ? (IndexBufferView?)new IndexBufferView
            {
                BufferLocation = buffer.NativeResource.GPUVirtualAddress + offset,
                Format = is32bits ? SharpDX.DXGI.Format.R32_UInt : SharpDX.DXGI.Format.R16_UInt,
                SizeInBytes = buffer.SizeInBytes - offset
            } : null);
        }

        public void ResourceBarrierTransition(GraphicsResource resource, GraphicsResourceState newState)
        {
            // Find parent resource
            if (resource.ParentResource != null)
                resource = resource.ParentResource;

            var currentState = resource.NativeResourceState;
            if (currentState != (ResourceStates)newState)
            {
                resource.NativeResourceState = (ResourceStates)newState;
                currentCommandList.NativeCommandList.ResourceBarrierTransition(resource.NativeResource, currentState, (ResourceStates)newState);
            }
        }

        public void SetDescriptorSets(int index, DescriptorSet[] descriptorSets)
        {
        RestartWithNewHeap:
            var descriptorTableIndex = 0;
            for (int i = 0; i < descriptorSets.Length; ++i)
            {
                // Find what is already mapped
                var descriptorSet = descriptorSets[i];

                var srvBindCount = boundPipelineState.SrvBindCounts[i];
                var samplerBindCount = boundPipelineState.SamplerBindCounts[i];

                if (srvBindCount > 0 && (IntPtr)descriptorSet.SrvStart.Ptr != IntPtr.Zero)
                {
                    GpuDescriptorHandle gpuSrvStart;

                    // Check if we need to copy them to shader visible descriptor heap
                    if (!srvMapping.TryGetValue(descriptorSet.SrvStart.Ptr, out gpuSrvStart))
                    {
                        var srvCount = descriptorSet.Description.SrvCount;

                        // Make sure heap is big enough
                        if (srvHeapOffset + srvCount > GraphicsDevice.SrvHeapSize)
                        {
                            ResetSrvHeap(true);
                            currentCommandList.NativeCommandList.SetDescriptorHeaps(2, descriptorHeaps);
                            goto RestartWithNewHeap;
                        }

                        // Copy
                        NativeDevice.CopyDescriptorsSimple(srvCount, srvHeap.CPUDescriptorHandleForHeapStart + srvHeapOffset * GraphicsDevice.SrvHandleIncrementSize, descriptorSet.SrvStart, DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

                        // Store mapping
                        srvMapping.Add(descriptorSet.SrvStart.Ptr, gpuSrvStart = srvHeap.GPUDescriptorHandleForHeapStart + srvHeapOffset * GraphicsDevice.SrvHandleIncrementSize);

                        // Bump
                        srvHeapOffset += srvCount;
                    }

                    // Bind resource tables (note: once per using stage, until we solve how to choose shader registers effect-wide at compile time)
                    for (int j = 0; j < srvBindCount; ++j)
                        currentCommandList.NativeCommandList.SetGraphicsRootDescriptorTable(descriptorTableIndex++, gpuSrvStart);
                }

                if (samplerBindCount > 0 && (IntPtr)descriptorSet.SamplerStart.Ptr != IntPtr.Zero)
                {
                    GpuDescriptorHandle gpuSamplerStart;

                    // Check if we need to copy them to shader visible descriptor heap
                    if (!samplerMapping.TryGetValue(descriptorSet.SamplerStart.Ptr, out gpuSamplerStart))
                    {
                        var samplerCount = descriptorSet.Description.SamplerCount;

                        // Make sure heap is big enough
                        if (samplerHeapOffset + samplerCount > GraphicsDevice.SamplerHeapSize)
                        {
                            ResetSamplerHeap(true);
                            currentCommandList.NativeCommandList.SetDescriptorHeaps(2, descriptorHeaps);
                            goto RestartWithNewHeap;
                        }

                        // Copy
                        NativeDevice.CopyDescriptorsSimple(samplerCount, samplerHeap.CPUDescriptorHandleForHeapStart + samplerHeapOffset * GraphicsDevice.SamplerHandleIncrementSize, descriptorSet.SamplerStart, DescriptorHeapType.Sampler);

                        // Store mapping
                        samplerMapping.Add(descriptorSet.SamplerStart.Ptr, gpuSamplerStart = samplerHeap.GPUDescriptorHandleForHeapStart + samplerHeapOffset * GraphicsDevice.SamplerHandleIncrementSize);

                        // Bump
                        samplerHeapOffset += samplerCount;
                    }

                    // Bind resource tables (note: once per using stage, until we solve how to choose shader registers effect-wide at compile time)
                    for (int j = 0; j < samplerBindCount; ++j)
                        currentCommandList.NativeCommandList.SetGraphicsRootDescriptorTable(descriptorTableIndex++, gpuSamplerStart);
                }
            }
        }

        private void ResetSrvHeap(bool createNewHeap)
        {
            if (srvHeap.Heap != null)
            {
                currentCommandList.SrvHeaps.Add(srvHeap.Heap);
                srvHeap.Heap = null;
            }

            if (createNewHeap)
            {
                srvHeap = new DescriptorHeapCache(GraphicsDevice.SrvHeaps.GetObject());
                srvHeapOffset = 0;
                srvMapping.Clear();
            }

            descriptorHeaps[0] = srvHeap.Heap;
        }

        private void ResetSamplerHeap(bool createNewHeap)
        {
            if (samplerHeap.Heap != null)
            {
                currentCommandList.SamplerHeaps.Add(samplerHeap.Heap);
                samplerHeap.Heap = null;
            }

            if (createNewHeap)
            {
                samplerHeap = new DescriptorHeapCache(GraphicsDevice.SamplerHeaps.GetObject());
                samplerHeapOffset = 0;
                samplerMapping.Clear();
            }

            descriptorHeaps[1] = samplerHeap.Heap;
        }

        /// <inheritdoc />
        public void Dispatch(int threadCountX, int threadCountY, int threadCountZ)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispatches the specified indirect buffer.
        /// </summary>
        /// <param name="indirectBuffer">The indirect buffer.</param>
        /// <param name="offsetInBytes">The offset information bytes.</param>
        public void Dispatch(Buffer indirectBuffer, int offsetInBytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draw non-indexed, non-instanced primitives.
        /// </summary>
        /// <param name="vertexCount">Number of vertices to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex, which is usually an offset in a vertex buffer; it could also be used as the first vertex id generated for a shader parameter marked with the <strong>SV_TargetId</strong> system-value semantic.</param>
        public void Draw(int vertexCount, int startVertexLocation = 0)
        {
            PrepareDraw();

            currentCommandList.NativeCommandList.DrawInstanced(vertexCount, 1, startVertexLocation, 0);

            GraphicsDevice.FrameTriangleCount += (uint)vertexCount;
            GraphicsDevice.FrameDrawCalls++;
        }

        /// <summary>
        /// Begins profiling.
        /// Submits a GPU timestamp query.
        /// </summary>
        /// <param name="queryPool">The <see cref="QueryPool"/> owning "query".</param>
        /// <param name="query">The <see cref="Query"/>.</param>
        public void WriteTimestamp(QueryPool queryPool, Query query)
        {

        }

        public void BeginQuery(QueryPool queryPool, Query query)
        {

        }

        public void EndQuery(QueryPool queryPool, Query query)
        {

        }

        /// <summary>
        /// Draw geometry of an unknown size.
        /// </summary>
        public void DrawAuto()
        {
            PrepareDraw();

            //NativeDeviceContext.DrawAuto();
            throw new NotImplementedException();

            GraphicsDevice.FrameDrawCalls++;
        }

        /// <summary>
        /// Draw indexed, non-instanced primitives.
        /// </summary>
        /// <param name="indexCount">Number of indices to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        public void DrawIndexed(int indexCount, int startIndexLocation = 0, int baseVertexLocation = 0)
        {
            PrepareDraw();

            currentCommandList.NativeCommandList.DrawIndexedInstanced(indexCount, 1, startIndexLocation, baseVertexLocation, 0);

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
            PrepareDraw();

            currentCommandList.NativeCommandList.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

            GraphicsDevice.FrameDrawCalls++;
            GraphicsDevice.FrameTriangleCount += (uint)(indexCountPerInstance * instanceCount);
        }

        /// <summary>
        /// Draw indexed, instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="argumentsBuffer">A buffer containing the GPU generated primitives.</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawIndexedInstanced(Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {
            if (argumentsBuffer == null) throw new ArgumentNullException("argumentsBuffer");

            PrepareDraw();

            //NativeDeviceContext.DrawIndexedInstancedIndirect(argumentsBuffer.NativeBuffer, alignedByteOffsetForArgs);
            throw new NotImplementedException();

            GraphicsDevice.FrameDrawCalls++;
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
            PrepareDraw();

            currentCommandList.NativeCommandList.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);

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
            if (argumentsBuffer == null) throw new ArgumentNullException("argumentsBuffer");

            PrepareDraw();

            //NativeDeviceContext.DrawIndexedInstancedIndirect(argumentsBuffer.NativeBuffer, alignedByteOffsetForArgs);
            throw new NotImplementedException();

            GraphicsDevice.FrameDrawCalls++;
        }

        /// <summary>
        /// Begins profiling.
        /// </summary>
        /// <param name="profileColor">Color of the profile.</param>
        /// <param name="name">The name.</param>
        public void BeginProfile(Color4 profileColor, string name)
        {
            //currentCommandList.NativeCommandList.BeginEvent();
        }

        /// <summary>
        /// Ends profiling.
        /// </summary>
        public void EndDebugEvent()
        {
            //currentCommandList.NativeCommandList.EndEvent();
        }

        /// <summary>
        /// Submit a timestamp query.
        /// </summary>
        /// <param name="queryPool">The QueryPool owning the query.</param>
        /// <param name="index">The query index.</param>
        public void WriteTimestamp(QueryPool queryPool, int index)
        {
            currentCommandList.NativeCommandList.EndQuery(queryPool.NativeQueryHeap, SharpDX.Direct3D12.QueryType.Timestamp, index);
            queryPool.PendingValue = queryPool.CompletedValue + 1;
        }

        public void ResetQueryPool(QueryPool queryPool)
        {
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
            currentCommandList.NativeCommandList.ClearDepthStencilView(depthStencilBuffer.NativeDepthStencilView, (ClearFlags)options, depth, stencil);
        }

        /// <summary>
        /// Clears the specified render target. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="System.ArgumentNullException">renderTarget</exception>
        public unsafe void Clear(Texture renderTarget, Color4 color)
        {
            currentCommandList.NativeCommandList.ClearRenderTargetView(renderTarget.NativeRenderTargetView, *(RawColor4*)&color);
        }

        /// <summary>
        /// Clears a read-write Buffer. This buffer must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;buffer</exception>
        public void ClearReadWrite(Buffer buffer, Vector4 value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a read-write Buffer. This buffer must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;buffer</exception>
        public void ClearReadWrite(Buffer buffer, Int4 value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a read-write Buffer. This buffer must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;buffer</exception>
        public void ClearReadWrite(Buffer buffer, UInt4 value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a read-write Texture. This texture must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">texture</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;texture</exception>
        public void ClearReadWrite(Texture texture, Vector4 value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a read-write Texture. This texture must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">texture</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;texture</exception>
        public void ClearReadWrite(Texture texture, Int4 value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a read-write Texture. This texture must have been created with read-write/unordered access.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">texture</exception>
        /// <exception cref="System.ArgumentException">Expecting buffer supporting UAV;texture</exception>
        public void ClearReadWrite(Texture texture, UInt4 value)
        {
            throw new NotImplementedException();
        }

        public void Copy(GraphicsResource source, GraphicsResource destination)
        {
            var sourceTexture = source as Texture;
            var destinationTexture = destination as Texture;

            if (sourceTexture != null && destinationTexture != null)
            {
                var sourceParent = sourceTexture.ParentTexture ?? sourceTexture;
                var destinationParent = destinationTexture.ParentTexture ?? destinationTexture;

                if (sourceParent.NativeResourceState != ResourceStates.CopySource)
                    currentCommandList.NativeCommandList.ResourceBarrierTransition(sourceTexture.NativeResource, sourceParent.NativeResourceState, ResourceStates.CopySource);
                if (destinationParent.NativeResourceState != ResourceStates.CopyDestination)
                    currentCommandList.NativeCommandList.ResourceBarrierTransition(destinationTexture.NativeResource, destinationParent.NativeResourceState, ResourceStates.CopyDestination);

                if (destinationTexture.Usage == GraphicsResourceUsage.Staging)
                {
                    int copyOffset = 0;
                    for (int arraySlice = 0; arraySlice < sourceParent.ArraySize; ++arraySlice)
                    {
                        for (int mipLevel = 0; mipLevel < sourceParent.MipLevels; ++mipLevel)
                        {
                            currentCommandList.NativeCommandList.CopyTextureRegion(new TextureCopyLocation(destinationTexture.NativeResource,
                                new PlacedSubResourceFootprint
                                {
                                    Footprint =
                                    {
                                        Width = Texture.CalculateMipSize(destinationTexture.Width, mipLevel),
                                        Height = Texture.CalculateMipSize(destinationTexture.Height, mipLevel),
                                        Depth = Texture.CalculateMipSize(destinationTexture.Depth, mipLevel),
                                        Format = (SharpDX.DXGI.Format)destinationTexture.Format,
                                        RowPitch = destinationTexture.ComputeRowPitch(mipLevel),
                                    },
                                    Offset = copyOffset,
                                }), 0, 0, 0, new TextureCopyLocation(sourceTexture.NativeResource, arraySlice * sourceParent.MipLevels + mipLevel), null);

                            copyOffset += destinationTexture.ComputeSubresourceSize(mipLevel);
                        }
                    }

                    // Fence for host access
                    destinationParent.StagingFenceValue = null;
                    destinationParent.StagingBuilder = this;
                    currentCommandList.StagingResources.Add(destinationParent);
                }
                else
                {
                    currentCommandList.NativeCommandList.CopyResource(destinationTexture.NativeResource, sourceTexture.NativeResource);
                }

                if (sourceParent.NativeResourceState != ResourceStates.CopySource)
                    currentCommandList.NativeCommandList.ResourceBarrierTransition(sourceTexture.NativeResource, ResourceStates.CopySource, sourceParent.NativeResourceState);
                if (destinationParent.NativeResourceState != ResourceStates.CopyDestination)
                    currentCommandList.NativeCommandList.ResourceBarrierTransition(destinationTexture.NativeResource, ResourceStates.CopyDestination, destinationParent.NativeResourceState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void CopyMultisample(Texture sourceMultisampleTexture, int sourceSubResource, Texture destTexture, int destSubResource, PixelFormat format = PixelFormat.None)
        {
            throw new NotImplementedException();
        }

        public void CopyRegion(GraphicsResource source, int sourceSubresource, ResourceRegion? sourceRegion, GraphicsResource destination, int destinationSubResource, int dstX = 0, int dstY = 0, int dstZ = 0)
        {
            if (source is Texture && destination is Texture)
            {
                if (((Texture)source).Usage == GraphicsResourceUsage.Staging || ((Texture)destination).Usage == GraphicsResourceUsage.Staging)
                {
                    throw new NotImplementedException("Copy region of staging resources is not supported yet");
                }

                currentCommandList.NativeCommandList.CopyTextureRegion(
                    new TextureCopyLocation(destination.NativeResource, sourceSubresource),
                    dstX, dstY, dstZ,
                    new TextureCopyLocation(source.NativeResource, sourceSubresource),
                    sourceRegion.HasValue
                        ? (SharpDX.Direct3D12.ResourceRegion?)new SharpDX.Direct3D12.ResourceRegion
                        {
                            Left = sourceRegion.Value.Left,
                            Top = sourceRegion.Value.Top,
                            Front = sourceRegion.Value.Front,
                            Right = sourceRegion.Value.Right,
                            Bottom = sourceRegion.Value.Bottom,
                            Back = sourceRegion.Value.Back
                        }
                        : null);
            }
            else if (source is Buffer && destination is Buffer)
            {
                currentCommandList.NativeCommandList.CopyBufferRegion(destination.NativeResource, dstX,
                    source.NativeResource, sourceRegion?.Left ?? 0, sourceRegion.HasValue ? sourceRegion.Value.Right - sourceRegion.Value.Left : ((Buffer)source).SizeInBytes);
            }
        }

        /// <inheritdoc />
        public void CopyCount(Buffer sourceBuffer, Buffer destBuffer, int offsetInBytes)
        {
            throw new NotImplementedException();
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox)
        {
            ResourceRegion region;
            var texture = resource as Texture;
            if (texture != null)
            {
                region = new ResourceRegion(0, 0, 0, texture.Width, texture.Height, texture.Depth);
            }
            else
            {
                var buffer = resource as Buffer;
                if (buffer != null)
                {
                    region = new ResourceRegion(0, 0, 0, buffer.SizeInBytes, 1, 1);
                }
                else
                {
                    throw new InvalidOperationException("Unknown resource type");
                }
            }

            UpdateSubresource(resource, subResourceIndex, databox, region);
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox, ResourceRegion region)
        {
            var texture = resource as Texture;
            if (texture != null)
            {
                var width = region.Right - region.Left;
                var height = region.Bottom - region.Top;
                var depth = region.Back - region.Front;

                ResourceDescription resourceDescription;
                switch (texture.Dimension)
                {
                    case TextureDimension.Texture1D:
                        resourceDescription = ResourceDescription.Texture1D((SharpDX.DXGI.Format)texture.Format, width, 1, 1);
                        break;
                    case TextureDimension.Texture2D:
                    case TextureDimension.TextureCube:
                        resourceDescription = ResourceDescription.Texture2D((SharpDX.DXGI.Format)texture.Format, width, height, 1, 1);
                        break;
                    case TextureDimension.Texture3D:
                        resourceDescription = ResourceDescription.Texture3D((SharpDX.DXGI.Format)texture.Format, width, height, (short)depth, 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // TODO D3D12 allocate in upload heap (placed resources?)
                var nativeUploadTexture = NativeDevice.CreateCommittedResource(new HeapProperties(CpuPageProperty.WriteBack, MemoryPool.L0), HeapFlags.None,
                    resourceDescription,
                    ResourceStates.GenericRead);


                GraphicsDevice.TemporaryResources.Enqueue(new KeyValuePair<long, object>(GraphicsDevice.NextFenceValue, nativeUploadTexture));

                nativeUploadTexture.WriteToSubresource(0, null, databox.DataPointer, databox.RowPitch, databox.SlicePitch);

                var parentResource = resource.ParentResource ?? resource;

                // Trigger copy
                currentCommandList.NativeCommandList.ResourceBarrierTransition(resource.NativeResource, parentResource.NativeResourceState, ResourceStates.CopyDestination);
                currentCommandList.NativeCommandList.CopyTextureRegion(new TextureCopyLocation(resource.NativeResource, subResourceIndex), region.Left, region.Top, region.Front, new TextureCopyLocation(nativeUploadTexture, 0), null);
                currentCommandList.NativeCommandList.ResourceBarrierTransition(resource.NativeResource, ResourceStates.CopyDestination, parentResource.NativeResourceState);
            }
            else
            {
                var buffer = resource as Buffer;
                if (buffer != null)
                {
                    SharpDX.Direct3D12.Resource uploadResource;
                    int uploadOffset;
                    var uploadSize = region.Right - region.Left;
                    var uploadMemory = GraphicsDevice.AllocateUploadBuffer(region.Right - region.Left, out uploadResource, out uploadOffset);

                    Utilities.CopyMemory(uploadMemory, databox.DataPointer, uploadSize);

                    currentCommandList.NativeCommandList.ResourceBarrierTransition(resource.NativeResource, resource.NativeResourceState, ResourceStates.CopyDestination);
                    currentCommandList.NativeCommandList.CopyBufferRegion(resource.NativeResource, region.Left, uploadResource, uploadOffset, uploadSize);
                    currentCommandList.NativeCommandList.ResourceBarrierTransition(resource.NativeResource, ResourceStates.CopyDestination, resource.NativeResourceState);
                }
                else
                {
                    throw new InvalidOperationException("Unknown resource type");
                }
            }
        }

        // TODO GRAPHICS REFACTOR what should we do with this?
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
        public MappedResource MapSubresource(GraphicsResource resource, int subResourceIndex, MapMode mapMode, bool doNotWait = false, int offsetInBytes = 0, int lengthInBytes = 0)
        {
            if (resource == null) throw new ArgumentNullException("resource");

            var rowPitch = 0;
            var depthStride = 0;
            var usage = GraphicsResourceUsage.Default;

            var texture = resource as Texture;
            if (texture != null)
            {
                usage = texture.Usage;
                if (lengthInBytes == 0)
                    lengthInBytes = texture.ComputeSubresourceSize(subResourceIndex);

                rowPitch = texture.ComputeRowPitch(subResourceIndex % texture.MipLevels);
                depthStride = texture.ComputeSlicePitch(subResourceIndex % texture.MipLevels);

                if (texture.Usage == GraphicsResourceUsage.Staging)
                {
                    // Internally it's a buffer, so adapt resource index and offset
                    offsetInBytes = texture.ComputeBufferOffset(subResourceIndex, 0);
                    subResourceIndex = 0;
                }
            }
            else
            {
                var buffer = resource as Buffer;
                if (buffer != null)
                {
                    usage = buffer.Usage;
                    if (lengthInBytes == 0)
                        lengthInBytes = buffer.SizeInBytes;
                }
            }

            if (mapMode == MapMode.Read || mapMode == MapMode.ReadWrite || mapMode == MapMode.Write)
            {
                // Is non-staging ever possible for Read/Write?
                if (usage != GraphicsResourceUsage.Staging)
                    throw new InvalidOperationException();
            }

            if (mapMode == MapMode.WriteDiscard)
            {
                throw new InvalidOperationException("Can't use WriteDiscard on Graphics API that don't support renaming");
            }

            if (mapMode != MapMode.WriteNoOverwrite)
            {
                // Need to wait?
                if (!resource.StagingFenceValue.HasValue || !GraphicsDevice.IsFenceCompleteInternal(resource.StagingFenceValue.Value))
                {
                    if (doNotWait)
                    {
                        return new MappedResource(resource, subResourceIndex, new DataBox(IntPtr.Zero, 0, 0));
                    }

                    // Need to flush? (i.e. part of)
                    if (resource.StagingBuilder == this)
                        FlushInternal(false);

                    if (!resource.StagingFenceValue.HasValue)
                        throw new InvalidOperationException("CommandList updating the staging resource has not been submitted");

                    GraphicsDevice.WaitForFenceInternal(resource.StagingFenceValue.Value);
                }
            }

            var mappedMemory = resource.NativeResource.Map(subResourceIndex) + offsetInBytes;
            return new MappedResource(resource, subResourceIndex, new DataBox(mappedMemory, rowPitch, depthStride), offsetInBytes, lengthInBytes);
        }

        // TODO GRAPHICS REFACTOR what should we do with this?
        public void UnmapSubresource(MappedResource unmapped)
        {
            unmapped.Resource.NativeResource.Unmap(unmapped.SubResourceIndex);
        }

        // Contains a DescriptorHeap and cache its GPU and CPU pointers
        struct DescriptorHeapCache
        {
            public DescriptorHeapCache(DescriptorHeap heap) : this()
            {
                Heap = heap;
                if (heap != null)
                {
                    CPUDescriptorHandleForHeapStart = heap.CPUDescriptorHandleForHeapStart;
                    GPUDescriptorHandleForHeapStart = heap.GPUDescriptorHandleForHeapStart;
                }
            }

            public DescriptorHeap Heap;
            public CpuDescriptorHandle CPUDescriptorHandleForHeapStart;
            public GpuDescriptorHandle GPUDescriptorHandleForHeapStart;
        }
    }
}

#endif 
