// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using SharpVulkan;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class CommandList
    {
        private CommandPool nativeCommandPool;
        internal CommandBuffer NativeCommandBuffer;

        private RenderPass activeRenderPass;
        private PipelineState activePipeline;

        private readonly ImageView[] framebufferAttachments = new ImageView[9];
        private int framebufferAttachmentCount;
        private bool framebufferDirty = true;
        private Framebuffer activeFramebuffer;

        private SharpVulkan.DescriptorPool descriptorPool;
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

            var poolSizes = new[]
            {
                new DescriptorPoolSize { Type = DescriptorType.SampledImage, DescriptorCount = 1 << 16 },
                new DescriptorPoolSize { Type = DescriptorType.Sampler, DescriptorCount = 1 << 16 },
                new DescriptorPoolSize { Type = DescriptorType.UniformBuffer, DescriptorCount = 1 << 16 },
            };

            var descriptorPoolCreateInfo = new DescriptorPoolCreateInfo
            {
                StructureType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PoolSizes = new IntPtr(Interop.Fixed(poolSizes)),
                MaxSets = 1 << 16,
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSet
            };
            descriptorPool = GraphicsDevice.NativeDevice.CreateDescriptorPool(ref descriptorPoolCreateInfo);

            Reset();
        }

        public unsafe void Reset()
        {
            CleanupRenderPass();
            boundDescriptorSets.Clear();

            GraphicsDevice.ReleaseTemporaryResources();

            foreach (var framebuffer in framebuffers)
            {
                GraphicsDevice.NativeDevice.DestroyFramebuffer(framebuffer);
            }
            framebuffers.Clear();

            if (descriptorSets.Count > 0)
            {
                fixed (SharpVulkan.DescriptorSet* descriptorSetsPointer = &descriptorSets.Items[0])
                    GraphicsDevice.NativeDevice.FreeDescriptorSets(descriptorPool, (uint)descriptorSets.Count, descriptorSetsPointer);
                descriptorSets.Clear(true);
            }
            //GraphicsDevice.NativeDevice.ResetDescriptorPool(descriptorPool, DescriptorPoolResetFlags.None);

            //ResetSrvHeap();
            //ResetSamplerHeap();

            //// Clear descriptor mappings
            //srvMapping.Clear();
            //samplerMapping.Clear();

            // NOTE: Begin implies Reset(CommandBufferResetFlags.None)
            var beginInfo = new CommandBufferBeginInfo
            {
                StructureType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmit,
            };
            NativeCommandBuffer.Begin(ref beginInfo);

            // TODO VULKAN: Reset whole pool per frame?
            // NativeCommandBuffer.Reset(CommandBufferResetFlags.ReleaseResources);
            // GraphicsDevice.NativeDevice.ResetCommandPool(nativeCommandPool, CommandPoolResetFlags.ReleseResources);
        }

        public void Close()
        {
            // End active render pass
            CleanupRenderPass();

            // Close
            NativeCommandBuffer.End();

            // Submit
            GraphicsDevice.ExecuteCommandListInternal(NativeCommandBuffer);


            // TODO VULKAN
            GraphicsDevice.NativeCommandQueue.WaitIdle();

            NativeCommandBuffer.Reset(CommandBufferResetFlags.None);
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
            framebufferAttachmentCount = renderTargetCount;
            for (int i = 0; i < renderTargetCount; i++)
            {
                framebufferAttachments[i] = renderTargets[i].NativeColorAttachmentView;
            }
            
            if (depthStencilBuffer != null)
            {
                framebufferAttachments[renderTargetCount] = depthStencilBuffer.NativeDepthStencilView;
                framebufferAttachmentCount++;
            }

            framebufferDirty = true;
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
        private void SetViewportImpl()
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
        private unsafe void PrepareDraw()
        {
            //// TODO D3D12 Hardcoded for one viewport
            var viewport = Viewport;
            NativeCommandBuffer.SetViewport(0, 1, (SharpVulkan.Viewport*)&viewport);

            var scissor = new Rect2D(0, 0, (uint)Viewport.Width, (uint)Viewport.Height);
            NativeCommandBuffer.SetScissor(0, 1, &scissor);

            NativeCommandBuffer.SetStencilReference(StencilFaceFlags.FrontAndBack, 0);

            // Lazily set the render pass and frame buffer
            EnsureRenderPass();

            var allocateInfo = new DescriptorSetAllocateInfo
            {
                StructureType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = descriptorPool,
                DescriptorSetCount = 1, //(uint)activePipeline.NativeDescriptorSetLayouts.Length,
                SetLayouts = new IntPtr(Interop.Fixed(activePipeline.NativeDescriptorSetLayouts))
            };
            var descriptorSet = GraphicsDevice.NativeDevice.AllocateDescriptorSets(ref allocateInfo);
            descriptorSets.Add(descriptorSet);

            

            writes.Clear(true);
            copies.Clear(true);
            samplerInfos.Clear(true);
            samplerInfos.EnsureCapacity(activePipeline.ImmutableSamplers.Length);

            fixed (DescriptorImageInfo* samplerInfosPointer = &samplerInfos.Items[0])
            {
                for (int i = 0; i < boundDescriptorSets.Count; i++)
                {
                    var setInfo = activePipeline.DescriptorSetMapping[i];

                    if (setInfo.Index >= 0)
                    {
                        for (int j = 0; j < setInfo.BindingCount; j++)
                        {
                            var destinationBinding = setInfo.BindingOffset + j;
                            var immutableSampler = activePipeline.ImmutableSamplers[destinationBinding];

                            // TODO VULKAN: Why do we need to update bindings that are just an immutable sampler?
                            if (immutableSampler != Sampler.Null)
                            {
                                writes.Add(new WriteDescriptorSet
                                {
                                    StructureType = StructureType.WriteDescriptorSet,
                                    DescriptorCount = 1,
                                    DestinationSet = descriptorSet,
                                    DestinationBinding = (uint)destinationBinding,
                                    DescriptorType = DescriptorType.Sampler,
                                    ImageInfo = new IntPtr(samplerInfosPointer + samplerInfos.Count)
                                });

                                samplerInfos.Add(new DescriptorImageInfo { Sampler = immutableSampler });
                            }
                            else
                            {
                                copies.Add(new CopyDescriptorSet
                                {
                                    StructureType = StructureType.CopyDescriptorSet,
                                    SourceSet = boundDescriptorSets[i],
                                    SourceBinding = (uint)j,
                                    SourceArrayElement = 0,
                                    DestinationSet = descriptorSet,
                                    DestinationBinding = (uint)destinationBinding,
                                    DestinationArrayElement = 0,
                                    DescriptorCount = 1
                                });
                            }
                        }
                    }
                }

                GraphicsDevice.NativeDevice.UpdateDescriptorSets(
                    (uint)writes.Count, writes.Count > 0 ? (WriteDescriptorSet*)Interop.Fixed(writes.Items) : null,
                    (uint)copies.Count, copies.Count > 0 ? (CopyDescriptorSet*)Interop.Fixed(copies.Items) : null);
            }

            NativeCommandBuffer.BindDescriptorSets(PipelineBindPoint.Graphics, activePipeline.NativeLayout, 0, 1, &descriptorSet, 0, null);
        }

        private readonly FastList<DescriptorImageInfo> samplerInfos = new FastList<DescriptorImageInfo>();
        private readonly FastList<WriteDescriptorSet> writes = new FastList<WriteDescriptorSet>();
        private readonly FastList<CopyDescriptorSet> copies = new FastList<CopyDescriptorSet>();
        private readonly FastList<SharpVulkan.DescriptorSet> descriptorSets = new FastList<SharpVulkan.DescriptorSet>(); 

        public void SetStencilReference(int stencilReference)
        {
            //NativeCommandBuffer.StencilReference = stencilReference;
        }

        public void SetPipelineState(PipelineState pipelineState)
        {
            if (pipelineState == activePipeline)
                return;

            activePipeline = pipelineState;

            NativeCommandBuffer.BindPipeline(PipelineBindPoint.Graphics, pipelineState.NativePipeline);
        }

        public unsafe void SetVertexBuffer(int index, Buffer buffer, int offset, int stride)
        {
            // TODO VULKAN API: Stride is part of Pipeline 

            // TODO VULKAN: Handle multiple buffers. Collect and apply before draw?
            if (index != 0)
                throw new NotImplementedException();

            var bufferCopy = buffer.NativeBuffer;
            var offsetCopy = (ulong)offset;

            NativeCommandBuffer.BindVertexBuffers((uint)index, 1, &bufferCopy, &offsetCopy);
        }

        public void SetIndexBuffer(Buffer buffer, int offset, bool is32bits)
        {
            NativeCommandBuffer.BindIndexBuffer(buffer.NativeBuffer, (ulong)offset, is32bits ? IndexType.UInt32 : IndexType.UInt16);
        }

        public unsafe void ResourceBarrierTransition(GraphicsResource resource, GraphicsResourceState newState)
        {
            var texture = resource as Texture;
            if (texture != null)
            {
                if (texture.ParentTexture != null)
                    texture = texture.ParentTexture;

                // TODO VULKAN: Check for change

                var oldLayout = texture.NativeLayout;
                var oldAccessMask = texture.NativeAccessMask;

                switch (newState)
                {
                    case GraphicsResourceState.RenderTarget:
                        texture.NativeLayout = ImageLayout.ColorAttachmentOptimal;
                        texture.NativeAccessMask = AccessFlags.ColorAttachmentWrite;
                        break;
                    case GraphicsResourceState.Present:
                        texture.NativeLayout = ImageLayout.PresentSource;
                        texture.NativeAccessMask = AccessFlags.MemoryRead;
                        break;
                    case GraphicsResourceState.DepthWrite:
                        texture.NativeLayout = ImageLayout.DepthStencilAttachmentOptimal;
                        texture.NativeAccessMask = AccessFlags.DepthStencilAttachmentWrite;
                        break;
                    case GraphicsResourceState.PixelShaderResource:
                        texture.NativeLayout = ImageLayout.ShaderReadOnlyOptimal;
                        texture.NativeAccessMask = AccessFlags.ShaderRead;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                if (oldLayout == texture.NativeLayout && oldAccessMask == texture.NativeAccessMask)
                    return;

                // End render pass, so barrier effects all commands in the buffer
                CleanupRenderPass();

                var memoryBarrier = new ImageMemoryBarrier
                {
                    StructureType = StructureType.ImageMemoryBarrier,
                    Image = texture.NativeImage,
                    SubresourceRange = new ImageSubresourceRange(texture.NativeImageAspect, 0, (uint)texture.ArraySize, 0, (uint)texture.MipLevels),
                    OldLayout = oldLayout,
                    NewLayout = texture.NativeLayout,
                    SourceAccessMask = oldAccessMask,
                    DestinationAccessMask = texture.NativeAccessMask,
                };
                NativeCommandBuffer.PipelineBarrier(
                    newState == GraphicsResourceState.Present ? PipelineStageFlags.AllCommands : PipelineStageFlags.TopOfPipe,
                    newState == GraphicsResourceState.Present ? PipelineStageFlags.BottomOfPipe : PipelineStageFlags.TopOfPipe,
                    DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private readonly FastList<SharpVulkan.DescriptorSet> boundDescriptorSets = new FastList<SharpVulkan.DescriptorSet>();

        public unsafe void SetDescriptorSets(int index, DescriptorSet[] descriptorSets)
        {
            if (index != 0)
                throw new NotImplementedException();

            boundDescriptorSets.Clear(true);
            for (int i = 0; i < descriptorSets.Length; i++)
            {
                boundDescriptorSets.Add(descriptorSets[i].NativeDescriptorSet);
            }
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
            var memoryBarrier = new ImageMemoryBarrier
            {
                StructureType = StructureType.ImageMemoryBarrier,
                Image = depthStencilBuffer.NativeImage,
                SubresourceRange = new ImageSubresourceRange(depthStencilBuffer.NativeImageAspect, 0, (uint)depthStencilBuffer.ArraySize, 0, (uint)depthStencilBuffer.MipLevels),
                OldLayout = depthStencilBuffer.NativeLayout,
                NewLayout = ImageLayout.TransferDestinationOptimal,
                SourceAccessMask = depthStencilBuffer.NativeAccessMask,
                DestinationAccessMask = AccessFlags.TransferWrite,
            };
            NativeCommandBuffer.PipelineBarrier(PipelineStageFlags.TopOfPipe, PipelineStageFlags.TopOfPipe, DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);

            var clearRange = new ImageSubresourceRange
            {
                BaseMipLevel = (uint)depthStencilBuffer.MipLevel,
                LevelCount = (uint)depthStencilBuffer.MipLevels,
                BaseArrayLayer = (uint)depthStencilBuffer.ArraySlice,
                LayerCount = (uint)depthStencilBuffer.ArraySize,
            };

            if ((options & DepthStencilClearOptions.DepthBuffer) != 0)
                clearRange.AspectMask |= ImageAspectFlags.Depth & depthStencilBuffer.NativeImageAspect;

            if ((options & DepthStencilClearOptions.Stencil) != 0)
                clearRange.AspectMask |= ImageAspectFlags.Stencil & depthStencilBuffer.NativeImageAspect;

            var clearValue = new ClearDepthStencilValue { Depth = depth, Stencil = stencil };
            NativeCommandBuffer.ClearDepthStencilImage(depthStencilBuffer.NativeImage, ImageLayout.TransferDestinationOptimal, clearValue, 1, &clearRange);

            memoryBarrier = new ImageMemoryBarrier
            {
                StructureType = StructureType.ImageMemoryBarrier,
                Image = depthStencilBuffer.NativeImage,
                SubresourceRange = new ImageSubresourceRange(depthStencilBuffer.NativeImageAspect, 0, (uint)depthStencilBuffer.ArraySize, 0, (uint)depthStencilBuffer.MipLevels),
                OldLayout = ImageLayout.TransferDestinationOptimal,
                NewLayout = depthStencilBuffer.NativeLayout,
                SourceAccessMask = AccessFlags.TransferWrite,
                DestinationAccessMask = depthStencilBuffer.NativeAccessMask,
            };
            NativeCommandBuffer.PipelineBarrier(PipelineStageFlags.TopOfPipe, PipelineStageFlags.TopOfPipe, DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
        }

        /// <summary>
        /// Clears the specified render target. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="System.ArgumentNullException">renderTarget</exception>
        public unsafe void Clear(Texture renderTarget, Color4 color)
        {
            // TODO VULKAN: Detect if inside render pass. If so, NativeCommandBuffer.ClearAttachments()

            var memoryBarrier = new ImageMemoryBarrier
            {
                StructureType = StructureType.ImageMemoryBarrier,
                Image = renderTarget.NativeImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.Color, 0, (uint)renderTarget.ArraySize, 0, (uint)renderTarget.MipLevels),
                OldLayout = renderTarget.NativeLayout,
                NewLayout = ImageLayout.TransferDestinationOptimal,
                SourceAccessMask = renderTarget.NativeAccessMask,
                DestinationAccessMask = AccessFlags.TransferWrite,
            };
            NativeCommandBuffer.PipelineBarrier(PipelineStageFlags.TopOfPipe, PipelineStageFlags.TopOfPipe, DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);

            var clearRange = new ImageSubresourceRange(ImageAspectFlags.Color, (uint)renderTarget.ArraySlice, (uint)renderTarget.ArraySize, (uint)renderTarget.MipLevel, (uint)renderTarget.MipLevels);
            NativeCommandBuffer.ClearColorImage(renderTarget.NativeImage, ImageLayout.TransferDestinationOptimal, ColorHelper.Convert(color), 1, &clearRange);

            memoryBarrier = new ImageMemoryBarrier
            {
                StructureType = StructureType.ImageMemoryBarrier,
                Image = renderTarget.NativeImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.Color, 0, (uint)renderTarget.ArraySize, 0, (uint)renderTarget.MipLevels),
                OldLayout = ImageLayout.TransferDestinationOptimal,
                NewLayout = renderTarget.NativeLayout,
                SourceAccessMask = AccessFlags.TransferWrite,
                DestinationAccessMask = renderTarget.NativeAccessMask,
            };
            NativeCommandBuffer.PipelineBarrier(PipelineStageFlags.TopOfPipe, PipelineStageFlags.TopOfPipe, DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
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
            throw new NotImplementedException();
        }

        public void CopyMultiSample(Texture sourceMsaaTexture, int sourceSubResource, Texture destTexture, int destSubResource, PixelFormat format = PixelFormat.None)
        {
            throw new NotImplementedException();
        }

        public void CopyRegion(GraphicsResource source, int sourceSubresource, ResourceRegion? sourecRegion, GraphicsResource destination, int destinationSubResource, int dstX = 0, int dstY = 0, int dstZ = 0)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CopyCount(Buffer sourceBuffer, Buffer destBuffer, int offsetInBytes)
        {
            throw new NotImplementedException();
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox)
        {
            throw new NotImplementedException();
        }

        internal unsafe void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox, ResourceRegion region)
        {
            var texture = resource as Texture;
            if (texture != null)
            {
                CleanupRenderPass();

                if (texture.Dimension != TextureDimension.Texture2D)
                    throw new NotImplementedException();

                var subresource = new ImageSubresource { AspectMask = ImageAspectFlags.Color, ArrayLayer = 0, MipLevel = 0 };
                SubresourceLayout subResourceLayout;
                GraphicsDevice.NativeDevice.GetImageSubresourceLayout(texture.NativeImage, subresource, out subResourceLayout);
                
                var mipSlice = subResourceIndex % texture.MipLevels;
                var arraySlice = subResourceIndex / texture.MipLevels;

                // BufferImageCopy.BufferOffset needs to be a multiple of 4
                SharpVulkan.Buffer uploadResource;
                int uploadOffset;
                var uploadMemory = GraphicsDevice.AllocateUploadBuffer(databox.SlicePitch + 4, out uploadResource, out uploadOffset);
                var alignment = ((uploadOffset + 3) & ~3) - uploadOffset;

                Utilities.CopyMemory(uploadMemory + alignment, databox.DataPointer, databox.SlicePitch);

                var bufferMemoryBarrier = new BufferMemoryBarrier
                {
                    StructureType = StructureType.BufferMemoryBarrier,
                    Buffer = uploadResource,
                    SourceAccessMask = AccessFlags.HostWrite,
                    DestinationAccessMask = AccessFlags.TransferRead,
                };

                var memoryBarrier = new ImageMemoryBarrier
                {
                    StructureType = StructureType.ImageMemoryBarrier,
                    Image = texture.NativeImage,
                    SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.Color, (uint)arraySlice, 1, (uint)mipSlice, 1),
                    OldLayout = texture.NativeLayout,
                    NewLayout = ImageLayout.TransferDestinationOptimal,
                    SourceAccessMask = texture.NativeAccessMask,
                    DestinationAccessMask = AccessFlags.TransferWrite,
                };
                NativeCommandBuffer.PipelineBarrier(PipelineStageFlags.AllCommands, PipelineStageFlags.Transfer, DependencyFlags.None, 0, null, 1, &bufferMemoryBarrier, 1, &memoryBarrier);

                // TODO VULKAN: Handle depth-stencil (NOTE: only supported on graphics queue)
                // TODO VULKAN: Check on non-zero slices
                // TODO VULKAN: Handle non-packed pitches
                var bufferCopy = new BufferImageCopy
                {
                    BufferOffset = (ulong)(uploadOffset + alignment),
                    ImageSubresource = new ImageSubresourceLayers { AspectMask = ImageAspectFlags.Color, BaseArrayLayer = (uint)arraySlice, LayerCount = 1, MipLevel = (uint)mipSlice },
                    BufferRowLength = 0, //(uint)databox.RowPitch / ...,
                    BufferImageHeight = 0, //(uint)databox.SlicePitch / ...,
                    ImageOffset = new Offset3D(region.Left, region.Top, region.Front),
                    ImageExtent = new Extent3D((uint)(region.Right - region.Left), (uint)(region.Bottom - region.Top), (uint)(region.Back - region.Front))
                };
                NativeCommandBuffer.CopyBufferToImage(uploadResource, texture.NativeImage, ImageLayout.TransferDestinationOptimal, 1, &bufferCopy);

                memoryBarrier = new ImageMemoryBarrier
                {
                    StructureType = StructureType.ImageMemoryBarrier,
                    Image = texture.NativeImage,
                    SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.Color, (uint)arraySlice, 1, (uint)mipSlice, 1),
                    OldLayout = ImageLayout.TransferDestinationOptimal,
                    NewLayout = texture.NativeLayout,
                    SourceAccessMask = AccessFlags.TransferWrite,
                    DestinationAccessMask = texture.NativeAccessMask,
                };
                NativeCommandBuffer.PipelineBarrier(PipelineStageFlags.Transfer, PipelineStageFlags.AllCommands, DependencyFlags.None, 0, null, 0, null, 1, &memoryBarrier);
            }
            else
            {
                throw new NotImplementedException();
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

            if (mapMode == MapMode.WriteDiscard || mapMode == MapMode.WriteNoOverwrite)
            {
                SharpVulkan.Buffer uploadResource;
                int uploadOffset;
                var uploadMemory = GraphicsDevice.AllocateUploadBuffer(lengthInBytes, out uploadResource, out uploadOffset);

                return new MappedResource(resource, subResourceIndex, new DataBox(uploadMemory, 0, 0), offsetInBytes, lengthInBytes)
                {
                    UploadResource = uploadResource,
                    UploadOffset = uploadOffset,
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        // TODO GRAPHICS REFACTOR what should we do with this?
        public unsafe void UnmapSubresource(MappedResource unmapped)
        {
             // Copy back
            var buffer = unmapped.Resource as Buffer;
            if (buffer != null)
            {
                CleanupRenderPass();

                var memoryBarrier = new BufferMemoryBarrier
                {
                    StructureType = StructureType.BufferMemoryBarrier,
                    Buffer = buffer.NativeBuffer,
                    Offset = (uint)unmapped.OffsetInBytes,
                    Size = (uint)unmapped.SizeInBytes,
                    SourceAccessMask = buffer.NativeAccessMask,
                    DestinationAccessMask = AccessFlags.TransferWrite,
                };
                NativeCommandBuffer.PipelineBarrier(PipelineStageFlags.AllCommands, PipelineStageFlags.Transfer, DependencyFlags.None, 0, null, 1, &memoryBarrier, 0, null);

                var bufferCopy = new BufferCopy
                {
                    DestinationOffset = (uint)unmapped.OffsetInBytes,
                    SourceOffset = (uint)unmapped.UploadOffset,
                    Size = (uint)unmapped.SizeInBytes
                };
                NativeCommandBuffer.CopyBuffer(unmapped.UploadResource, buffer.NativeBuffer, 1, &bufferCopy);

                memoryBarrier = new BufferMemoryBarrier
                {
                    StructureType = StructureType.BufferMemoryBarrier,
                    Buffer = buffer.NativeBuffer,
                    Offset = (uint)unmapped.OffsetInBytes,
                    Size = (uint)unmapped.SizeInBytes,
                    SourceAccessMask = AccessFlags.TransferWrite,
                    DestinationAccessMask = buffer.NativeAccessMask,
                };
                NativeCommandBuffer.PipelineBarrier(PipelineStageFlags.Transfer, PipelineStageFlags.AllCommands, DependencyFlags.None, 0, null, 1, &memoryBarrier, 0, null);
            }
        }

        protected internal unsafe override void OnDestroyed()
        {
            // TODO VULKAN: Cleanup

            base.OnDestroyed();
        }

        private unsafe void EnsureRenderPass()
        {
            if (activePipeline == null)
                return;

            var pipelineRenderPass = activePipeline.NativeRenderPass;

            if (!framebufferDirty && activeRenderPass == pipelineRenderPass)
                return;

            // End old render pass
            if (activeRenderPass != RenderPass.Null)
            {
                NativeCommandBuffer.EndRenderPass();
                activeRenderPass = RenderPass.Null;
            }

            // Release old frame buffer
            if (activeFramebuffer != Framebuffer.Null)
            {
                throw new NotImplementedException();
                //GraphicsDevice.NativeDevice.DestroyFramebuffer(activeFramebuffer);
                activeFramebuffer = Framebuffer.Null;
            }           
            
            if (pipelineRenderPass != RenderPass.Null)
            {
                var renderTarget = renderTargets[0] ?? depthStencilBuffer;

                // Create new frame buffer
                fixed (ImageView* attachmentsPointer = &framebufferAttachments[0])
                {
                    var framebufferCreateInfo = new FramebufferCreateInfo
                    {
                        StructureType = StructureType.FramebufferCreateInfo,
                        RenderPass = pipelineRenderPass,
                        AttachmentCount = (uint)framebufferAttachmentCount,
                        Attachments = new IntPtr(attachmentsPointer),
                        Width = (uint)renderTarget.ViewWidth,
                        Height = (uint)renderTarget.ViewHeight,
                        Layers = 1, // TODO VULKAN: Use correct view depth/array size
                    };
                    activeFramebuffer = GraphicsDevice.NativeDevice.CreateFramebuffer(ref framebufferCreateInfo);
                    framebuffers.Add(activeFramebuffer);
                }
                framebufferDirty = false;

                // Start new render pass
                var renderPassBegin = new RenderPassBeginInfo
                {
                    StructureType = StructureType.RenderPassBeginInfo,
                    RenderPass = pipelineRenderPass,
                    Framebuffer = activeFramebuffer,
                    RenderArea = new Rect2D(0, 0, (uint)renderTarget.ViewWidth, (uint)renderTarget.ViewHeight)
                };
                NativeCommandBuffer.BeginRenderPass(ref renderPassBegin, SubpassContents.Inline);

                activeRenderPass = pipelineRenderPass;
            }
        }

        private List<Framebuffer> framebuffers = new List<Framebuffer>(); 

        private unsafe void CleanupRenderPass()
        {
            if (activeRenderPass != RenderPass.Null)
            {
                NativeCommandBuffer.EndRenderPass();
                activeRenderPass = RenderPass.Null;
            }

            if (activeFramebuffer != Framebuffer.Null)
            {
                //GraphicsDevice.NativeDevice.DestroyFramebuffer(activeFramebuffer);
                activeFramebuffer = Framebuffer.Null;

                framebufferDirty = true;
            }
        }
    }
}
 
#endif 
