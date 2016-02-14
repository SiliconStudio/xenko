// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Collections.Generic;
using System.Threading;
using SharpVulkan;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class CommandList
    {
        private CommandPool nativeCommandPool;
        internal CommandBuffer NativeCommandBuffer;

        //private CommandAllocator nativeCommandAllocator;
        //internal GraphicsCommandList NativeCommandList;

        //private const int SrvHeapSize = 2048;
        //private DescriptorHeap srvHeap;
        //private int srvHeapOffset = SrvHeapSize;
        //private const int SamplerHeapSize = 64;
        //private DescriptorHeap samplerHeap;
        //private int samplerHeapOffset = SamplerHeapSize;

        //private PipelineState boundPipelineState;
        //private DescriptorHeap[] descriptorHeaps = new DescriptorHeap[2];

        //private Dictionary<IntPtr, GpuDescriptorHandle> srvMapping = new Dictionary<IntPtr, GpuDescriptorHandle>();
        //private Dictionary<IntPtr, GpuDescriptorHandle> samplerMapping = new Dictionary<IntPtr, GpuDescriptorHandle>();

        public unsafe CommandList(GraphicsDevice device) : base(device)
        {
            var commandPoolCreateInfo = new CommandPoolCreateInfo
            {
                StructureType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = 0, //device.NativeCommandQueue.FamilyIndex
                Flags = CommandPoolCreateFlags.ResetCommandBuffer
            };
            nativeCommandPool = device.NativeDevice.CreateCommandPool(ref commandPoolCreateInfo);

            var commandBufferAllocationInfo = new CommandBufferAllocateInfo
            {
                StructureType = StructureType.CommandBufferAllocateInfo,
                Level = CommandBufferLevel.Primary,
                CommandPool = nativeCommandPool,
                CommandBufferCount = 1
            };
            CommandBuffer nativeCommandBuffer;
            device.NativeDevice.AllocateCommandBuffers(ref commandBufferAllocationInfo, &nativeCommandBuffer);
            NativeCommandBuffer = nativeCommandBuffer;

            //nativeCommandAllocator = device.NativeDevice.CreateCommandAllocator(CommandListType.Direct);
            //NativeCommandList = device.NativeDevice.CreateCommandList(CommandListType.Direct, nativeCommandAllocator, null);

            ResetSrvHeap();
            ResetSamplerHeap();
        }

        public void Reset()
        {
            NativeCommandBuffer.Reset(CommandBufferResetFlags.ReleaseResources);

            //GraphicsDevice.ReleaseTemporaryResources();

            //ResetSrvHeap();
            //ResetSamplerHeap();

            //// Clear descriptor mappings
            //srvMapping.Clear();
            //samplerMapping.Clear();

            //nativeCommandAllocator.Reset();
            //NativeCommandList.Reset(nativeCommandAllocator, null);

            //// TODO D3D12 This should happen at beginning of frame only on main command list
            //NativeCommandList.ResourceBarrierTransition(GraphicsDevice.Presenter.BackBuffer.NativeResource, ResourceStates.Present, ResourceStates.RenderTarget);
        }

        public unsafe void Close()
        {
            var nativeCommandBufferCopy = NativeCommandBuffer;
            var submitInfo = new SubmitInfo
            {
                StructureType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                CommandBuffers = new IntPtr(&nativeCommandBufferCopy),
            };
            GraphicsDevice.NativeCommandQueue.Submit(1, &submitInfo, Fence.Null);

            //// TODO D3D12 This should happen at end of frame only on main command list
            //NativeCommandList.ResourceBarrierTransition(GraphicsDevice.Presenter.BackBuffer.NativeResource, ResourceStates.RenderTarget, ResourceStates.Present);

            //NativeCommandList.Close();
            //GraphicsDevice.NativeCommandQueue.ExecuteCommandList(NativeCommandList);
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
        private void SetDepthAndRenderTargetsImpl(Texture depthStencilBuffer, Texture[] renderTargets)
        {
            //// TODO D3D12 we don't have a way to provide array + size with SharpDX
            //var renderTargetLength = renderTargets.Length;
            //for (int i = 0; i < renderTargets.Length; ++i)
            //{
            //    if (renderTargets[i] == null)
            //    {
            //        renderTargetLength = i;
            //        break;
            //    }
            //}
            //var renderTargetHandles = new CpuDescriptorHandle[renderTargetLength];
            //for (int i = 0; i < renderTargetHandles.Length; ++i)
            //{
            //    renderTargetHandles[i] = renderTargets[i].NativeRenderTargetView;
            //}
            //NativeCommandList.SetRenderTargets(renderTargetHandles, depthStencilBuffer?.NativeDepthStencilView);
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
        }

        /// <summary>
        /// Binds a set of scissor rectangles to the rasterizer stage. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="scissorRectangles">The set of scissor rectangles to bind.</param>
        public void SetScissorRectangles(params Rectangle[] scissorRectangles)
        {
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
        private unsafe void SetViewportImpl()
        {
        }

        /// <summary>
        ///     Unsets the read/write buffers.
        /// </summary>
        public void UnsetReadWriteBuffers()
        {
        }

        /// <summary>
        /// Unsets the render targets.
        /// </summary>
        public void UnsetRenderTargets()
        {
        }

        /// <summary>
        ///     Prepares a draw call. This method is called before each Draw() method to setup the correct Primitive, InputLayout and VertexBuffers.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Cannot GraphicsDevice.Draw*() without an effect being previously applied with Effect.Apply() method</exception>
        private void PrepareDraw()
        {
            //// TODO D3D12 Hardcoded for one viewport
            //var viewport = viewports[0];
            //NativeCommandList.SetViewport(new RawViewportF { Width = viewport.Width, Height = viewport.Height, X = viewport.X, Y = viewport.Y, MinDepth = viewport.MinDepth, MaxDepth = viewport.MaxDepth });
            //NativeCommandList.SetScissorRectangles(new RawRectangle { Right = (int)viewport.Width, Bottom = (int)viewport.Height });
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            //boundPipelineState = pipelineState;
            //if (pipelineState.CompiledState != null)
            //{
            //    NativeCommandList.PipelineState = pipelineState.CompiledState;
            //    NativeCommandList.SetGraphicsRootSignature(pipelineState.RootSignature);
            //}
            //NativeCommandList.PrimitiveTopology = pipelineState.PrimitiveTopology;
        }

        public void SetVertexBuffer(int index, Buffer buffer, int offset, int stride)
        {
            //NativeCommandList.SetVertexBuffer(index, new VertexBufferView
            //{
            //    BufferLocation = buffer.NativeResource.GPUVirtualAddress + offset,
            //    StrideInBytes = stride,
            //    SizeInBytes = buffer.SizeInBytes - offset
            //});
        }

        public void SetIndexBuffer(Buffer buffer, int offset, bool is32bits)
        {
            //NativeCommandList.SetIndexBuffer(buffer != null ? (IndexBufferView?)new IndexBufferView
            //{
            //    BufferLocation = buffer.NativeResource.GPUVirtualAddress + offset,
            //    Format = is32bits ? SharpDX.DXGI.Format.R32_UInt : SharpDX.DXGI.Format.R16_UInt,
            //    SizeInBytes = buffer.SizeInBytes - offset
            //} : null);
        }

        public void SetDescriptorSets(int index, DescriptorSet[] descriptorSets)
        {
        //RestartWithNewHeap:
        //    NativeCommandList.SetDescriptorHeaps(2, descriptorHeaps);
        //    var descriptorTableIndex = 0;
        //    for (int i = 0; i < descriptorSets.Length; ++i)
        //    {
        //        // Find what is already mapped
        //        var descriptorSet = descriptorSets[i];

        //        if ((IntPtr)descriptorSet.SrvStart.Ptr != IntPtr.Zero)
        //        {
        //            GpuDescriptorHandle gpuSrvStart;

        //            // Check if we need to copy them to shader visible descriptor heap
        //            if (!srvMapping.TryGetValue(descriptorSet.SrvStart.Ptr, out gpuSrvStart))
        //            {
        //                var srvCount = descriptorSet.Description.SrvCount;

        //                // Make sure heap is big enough
        //                if (srvHeapOffset + srvCount > SrvHeapSize)
        //                {
        //                    ResetSrvHeap();
        //                    goto RestartWithNewHeap;
        //                }

        //                // Copy
        //                NativeDevice.CopyDescriptorsSimple(srvCount, srvHeap.CPUDescriptorHandleForHeapStart + srvHeapOffset * GraphicsDevice.SrvHandleIncrementSize, descriptorSet.SrvStart, DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

        //                // Store mapping
        //                srvMapping.Add(descriptorSet.SrvStart.Ptr, gpuSrvStart = srvHeap.GPUDescriptorHandleForHeapStart + srvHeapOffset * GraphicsDevice.SrvHandleIncrementSize);

        //                // Bump
        //                srvHeapOffset += srvCount;
        //            }

        //            // Bind resource tables (note: once per using stage, until we solve how to choose shader registers effect-wide at compile time)
        //            var srvBindCount = boundPipelineState.SrvBindCounts[i];
        //            for (int j = 0; j < srvBindCount; ++j)
        //                NativeCommandList.SetGraphicsRootDescriptorTable(descriptorTableIndex++, gpuSrvStart);
        //        }

        //        if ((IntPtr)descriptorSet.SamplerStart.Ptr != IntPtr.Zero)
        //        {
        //            GpuDescriptorHandle gpuSamplerStart;

        //            // Check if we need to copy them to shader visible descriptor heap
        //            if (!samplerMapping.TryGetValue(descriptorSet.SamplerStart.Ptr, out gpuSamplerStart))
        //            {
        //                var samplerCount = descriptorSet.Description.SamplerCount;

        //                // Make sure heap is big enough
        //                if (samplerHeapOffset + samplerCount > SamplerHeapSize)
        //                {
        //                    ResetSamplerHeap();
        //                    goto RestartWithNewHeap;
        //                }

        //                // Copy
        //                NativeDevice.CopyDescriptorsSimple(samplerCount, samplerHeap.CPUDescriptorHandleForHeapStart + samplerHeapOffset * GraphicsDevice.SamplerHandleIncrementSize, descriptorSet.SamplerStart, DescriptorHeapType.Sampler);

        //                // Store mapping
        //                samplerMapping.Add(descriptorSet.SamplerStart.Ptr, gpuSamplerStart = samplerHeap.GPUDescriptorHandleForHeapStart + samplerHeapOffset * GraphicsDevice.SamplerHandleIncrementSize);

        //                // Bump
        //                samplerHeapOffset += samplerCount;
        //            }

        //            // Bind resource tables (note: once per using stage, until we solve how to choose shader registers effect-wide at compile time)
        //            var samplerBindCount = boundPipelineState.SamplerBindCounts[i];
        //            for (int j = 0; j < samplerBindCount; ++j)
        //                NativeCommandList.SetGraphicsRootDescriptorTable(descriptorTableIndex++, gpuSamplerStart);
        //        }
        //    }
        }

        private void ResetSrvHeap()
        {
            //// Running out of space, create new heap and restart everything (to make sure everything is copied)
            //// TODO D3D12 probably could do a count before copying to avoid copying part of it for nothing?
            //srvHeap = NativeDevice.CreateDescriptorHeap(new DescriptorHeapDescription
            //{
            //    DescriptorCount = SrvHeapSize,
            //    Flags = DescriptorHeapFlags.ShaderVisible,
            //    Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
            //});
            //GraphicsDevice.TemporaryResources.Add(srvHeap);
            //srvHeapOffset = 0;
            //srvMapping.Clear();
            //descriptorHeaps[0] = srvHeap;
        }

        private void ResetSamplerHeap()
        {
            //// Running out of space, create new heap and restart everything (to make sure everything is copied)
            //// TODO D3D12 probably could do a count before copying to avoid copying part of it for nothing?
            //samplerHeap = NativeDevice.CreateDescriptorHeap(new DescriptorHeapDescription
            //{
            //    DescriptorCount = SamplerHeapSize,
            //    Flags = DescriptorHeapFlags.ShaderVisible,
            //    Type = DescriptorHeapType.Sampler,
            //});
            //GraphicsDevice.TemporaryResources.Add(samplerHeap);
            //samplerHeapOffset = 0;
            //samplerMapping.Clear();
            //descriptorHeaps[1] = samplerHeap;
        }

        /// <inheritdoc />
        public void Dispatch(int threadCountX, int threadCountY, int threadCountZ)
        {
        }

        /// <summary>
        /// Dispatches the specified indirect buffer.
        /// </summary>
        /// <param name="indirectBuffer">The indirect buffer.</param>
        /// <param name="offsetInBytes">The offset information bytes.</param>
        public void Dispatch(Buffer indirectBuffer, int offsetInBytes)
        {
        }

        /// <summary>
        /// Draw non-indexed, non-instanced primitives.
        /// </summary>
        /// <param name="vertexCount">Number of vertices to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex, which is usually an offset in a vertex buffer; it could also be used as the first vertex id generated for a shader parameter marked with the <strong>SV_TargetId</strong> system-value semantic.</param>
        public void Draw(int vertexCount, int startVertexLocation = 0)
        {
            PrepareDraw();

            NativeCommandBuffer.Draw((uint)vertexCount, 1, (uint)startVertexLocation, 0);
            //NativeCommandList.DrawInstanced(vertexCount, 1, startVertexLocation, 0);

            GraphicsDevice.FrameTriangleCount += (uint)vertexCount;
            GraphicsDevice.FrameDrawCalls++;
        }

        /// <summary>
        /// Draw geometry of an unknown size.
        /// </summary>
        public void DrawAuto()
        {
            PrepareDraw();

            throw new NotImplementedException();
            //NativeDeviceContext.DrawAuto();

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

            NativeCommandBuffer.DrawIndexed((uint)indexCount, 1, (uint)startIndexLocation, baseVertexLocation, 0);
            //NativeCommandList.DrawIndexedInstanced(indexCount, 1, startIndexLocation, baseVertexLocation, 0);

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

            NativeCommandBuffer.DrawIndexed((uint)indexCountPerInstance, (uint)instanceCount, (uint)startIndexLocation, baseVertexLocation, (uint)startInstanceLocation);
            //NativeCommandList.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

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

            throw new NotImplementedException();
            //NativeCommandBuffer.DrawIndirect(argumentsBuffer.NativeBuffer, (ulong)alignedByteOffsetForArgs, );
            //NativeDeviceContext.DrawIndexedInstancedIndirect(argumentsBuffer.NativeBuffer, alignedByteOffsetForArgs);

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

            NativeCommandBuffer.Draw((uint)vertexCountPerInstance, (uint)instanceCount, (uint)startVertexLocation, (uint)startVertexLocation);
            //NativeCommandList.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);

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

            throw new NotImplementedException();
            //NativeDeviceContext.DrawIndexedInstancedIndirect(argumentsBuffer.NativeBuffer, alignedByteOffsetForArgs);

            GraphicsDevice.FrameDrawCalls++;
        }

        /// <summary>
        /// Begins profiling.
        /// </summary>
        /// <param name="profileColor">Color of the profile.</param>
        /// <param name="name">The name.</param>
        public unsafe void BeginProfile(Color4 profileColor, string name)
        {
        }

        /// <summary>
        /// Ends profiling.
        /// </summary>
        public void EndProfile()
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
        public unsafe void Clear(Texture depthStencilBuffer, DepthStencilClearOptions options, float depth = 1, byte stencil = 0)
        {
            var clearRange = new ImageSubresourceRange
            {
                BaseMipLevel = (uint)depthStencilBuffer.MipLevel,
                LevelCount = (uint)depthStencilBuffer.MipLevels,
                BaseArrayLayer = (uint)depthStencilBuffer.ArraySlice,
                LayerCount = (uint)depthStencilBuffer.ArraySize,
            };

            if ((options & DepthStencilClearOptions.DepthBuffer) != 0)
                clearRange.AspectMask |= ImageAspectFlags.Depth;

            if ((options & DepthStencilClearOptions.Stencil) != 0)
                clearRange.AspectMask |= ImageAspectFlags.Stencil;

            var clearValue = new ClearDepthStencilValue { Depth = depth, Stencil = stencil };
            NativeCommandBuffer.ClearDepthStencilImage(depthStencilBuffer.NativeImage, ImageLayout.TransferDestinationOptimal, clearValue, 1, &clearRange);
            //NativeCommandList.ClearDepthStencilView(depthStencilBuffer.NativeDepthStencilView, (ClearFlags)options, depth, stencil);
        }

        /// <summary>
        /// Clears the specified render target. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="System.ArgumentNullException">renderTarget</exception>
        public unsafe void Clear(Texture renderTarget, Color4 color)
        {
            var clearRange = new ImageSubresourceRange
            {
                BaseMipLevel = (uint)depthStencilBuffer.MipLevel,
                LevelCount = (uint)depthStencilBuffer.MipLevels,
                BaseArrayLayer = (uint)depthStencilBuffer.ArraySlice,
                LayerCount = (uint)depthStencilBuffer.ArraySize,
                AspectMask = ImageAspectFlags.Color
            };
            
            NativeCommandBuffer.ClearColorImage(depthStencilBuffer.NativeImage, ImageLayout.TransferDestinationOptimal, ColorHelper.Convert(color), 1, &clearRange);
            //NativeCommandList.ClearRenderTargetView(renderTarget.NativeRenderTargetView, *(RawColor4*)&color);
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
        }

        public void Copy(GraphicsResource source, GraphicsResource destination)
        {
        }

        public void CopyMultiSample(Texture sourceMsaaTexture, int sourceSubResource, Texture destTexture, int destSubResource, PixelFormat format = PixelFormat.None)
        {
        }

        public void CopyRegion(GraphicsResource source, int sourceSubresource, ResourceRegion? sourecRegion, GraphicsResource destination, int destinationSubResource, int dstX = 0, int dstY = 0, int dstZ = 0)
        {
        }

        /// <inheritdoc />
        public void CopyCount(Buffer sourceBuffer, Buffer destBuffer, int offsetInBytes)
        {
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox)
        {
        }

        internal unsafe void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox, ResourceRegion region)
        {
            

            var texture = resource as Texture;
            if (texture != null)
            {
                if (texture.Dimension != TextureDimension.Texture2D)
                    throw new NotImplementedException();

                var width = region.Right - region.Left;
                var height = region.Bottom - region.Top;

                // TODO D3D12 allocate in upload heap (placed resources?)
                //var nativeUploadTexture = NativeDevice.CreateCommittedResource(new HeapProperties(CpuPageProperty.WriteBack, MemoryPool.L0), HeapFlags.None,
                //    ResourceDescription.Texture2D((SharpDX.DXGI.Format)texture.Format, width, height),
                //    ResourceStates.GenericRead);

                //GraphicsDevice.TemporaryResources.Add(nativeUploadTexture);

//                NativeCommandBuffer.UpdateBuffer(nativeUploadBuffer, offset, (uint)databox.SlicePitch, (uint*)databox.DataPointer);
                //nativeUploadTexture.WriteToSubresource(0, null, databox.DataPointer, databox.RowPitch, databox.SlicePitch);

                // Trigger copy
                var copyRegion = new BufferImageCopy
                {
                    //ImageExtent = new Extent3D(texture.Width, texture.Height, texture.Depth),
                    //ImageOffset = new Offset3D(region.Left, region.Top, 0),

                    //ImageSubresource = new ImageSubresourceLayers { AspectMask = }
                };
//                NativeCommandBuffer.CopyBufferToImage(nativeUploadBuffer, texture.NativeImage, ImageLayout.TransferDestinationOptimal, 1, &copyRegion);
                //NativeCommandList.ResourceBarrierTransition(resource.NativeResource, ResourceStates.Common, ResourceStates.CopyDestination);
                //NativeCommandList.CopyTextureRegion(new TextureCopyLocation(resource.NativeResource, subResourceIndex), region.Left, region.Top, region.Front, new TextureCopyLocation(nativeUploadTexture, 0), null);
                //NativeCommandList.ResourceBarrierTransition(resource.NativeResource, ResourceStates.CopyDestination, ResourceStates.Common);
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
            var texture = resource as Texture;
            if (texture != null)
            {
                if (lengthInBytes == 0)
                    lengthInBytes = texture.ViewWidth * texture.ViewHeight * texture.ViewDepth * texture.ViewFormat.SizeInBytes();
            }
            else
            {
                var buffer = resource as Buffer;
                if (buffer != null)
                {
                    if (lengthInBytes == 0)
                        lengthInBytes = buffer.SizeInBytes;
                }
            }

            //SharpDX.Direct3D12.Resource uploadResource;
            //int uploadOffset;
            //var uploadMemory = GraphicsDevice.AllocateUploadBuffer(lengthInBytes, out uploadResource, out uploadOffset);

            //return new MappedResource(resource, subResourceIndex, new DataBox(uploadMemory, 0, 0), offsetInBytes, lengthInBytes)
            //{
            //    UploadResource = uploadResource,
            //    UploadOffset = uploadOffset,
            //};
            return default(MappedResource);
        }

        // TODO GRAPHICS REFACTOR what should we do with this?
        public void UnmapSubresource(MappedResource unmapped)
        {
            // Copy back
            //var buffer = unmapped.Resource as Buffer;
            //if (buffer != null)
            //{
            //    NativeCommandList.ResourceBarrierTransition(buffer.NativeResource, buffer.NativeResourceStates, ResourceStates.CopyDestination);
            //    NativeCommandList.CopyBufferRegion(buffer.NativeResource, unmapped.OffsetInBytes, unmapped.UploadResource, unmapped.UploadOffset, unmapped.SizeInBytes);
            //    NativeCommandList.ResourceBarrierTransition(buffer.NativeResource, ResourceStates.CopyDestination, buffer.NativeResourceStates);
            //}
        }
    }
}
 
#endif 
