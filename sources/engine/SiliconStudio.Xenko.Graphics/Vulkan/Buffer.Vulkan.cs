// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Collections.Generic;

using SharpVulkan;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class Buffer
    {
        internal SharpVulkan.Buffer NativeBuffer;
        internal BufferView NativeBufferView;
        internal AccessFlags NativeAccessMask;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer" /> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        protected Buffer(GraphicsDevice device)
            : base(device)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer" /> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="viewFlags">Type of the buffer.</param>
        /// <param name="viewFormat">The view format.</param>
        /// <param name="dataPointer">The data pointer.</param>
        protected Buffer InitializeFromImpl(BufferDescription description, BufferFlags viewFlags, PixelFormat viewFormat, IntPtr dataPointer)
        {
            bufferDescription = description;
            //nativeDescription = ConvertToNativeDescription(Description);
            ViewFlags = viewFlags;
            InitCountAndViewFormat(out this.elementCount, ref viewFormat);
            ViewFormat = viewFormat;
            Recreate(dataPointer);

            if (GraphicsDevice != null)
            {
                GraphicsDevice.BuffersMemory += SizeInBytes/(float)0x100000;
            }

            return this;
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
            GraphicsDevice.BuffersMemory -= SizeInBytes / (float)0x100000;

            if (NativeBufferView != BufferView.Null)
            {
                GraphicsDevice.Collect(NativeBufferView);
                NativeBufferView = BufferView.Null;
            }

            if (NativeBuffer != SharpVulkan.Buffer.Null)
            {
                GraphicsDevice.Collect(NativeBuffer);
                NativeBuffer = SharpVulkan.Buffer.Null;
            }

            if (NativeMemory != DeviceMemory.Null)
            {
                GraphicsDevice.Collect(NativeMemory);
                NativeMemory = DeviceMemory.Null;
            }

            base.OnDestroyed();
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            base.OnRecreate();

            if (Description.Usage == GraphicsResourceUsage.Immutable
                || Description.Usage == GraphicsResourceUsage.Default)
                return false;

            Recreate(IntPtr.Zero);

            return true;
        }

        /// <summary>
        /// Explicitly recreate buffer with given data. Usually called after a <see cref="GraphicsDevice"/> reset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataPointer"></param>
        public unsafe void Recreate(IntPtr dataPointer)
        {
            var createInfo = new BufferCreateInfo
            {
                StructureType = StructureType.BufferCreateInfo,
                Size = (ulong)bufferDescription.SizeInBytes,
                Flags = BufferCreateFlags.None,
            };

            createInfo.Usage |= BufferUsageFlags.TransferSource;

            // We always fill using transfer
            //if (bufferDescription.Usage != GraphicsResourceUsage.Immutable)
                createInfo.Usage |= BufferUsageFlags.TransferDestination;

            if ((ViewFlags & BufferFlags.VertexBuffer) != 0)
            {
                createInfo.Usage |= BufferUsageFlags.VertexBuffer;
                NativeAccessMask |= AccessFlags.VertexAttributeRead;
                NativePipelineStageMask |= PipelineStageFlags.VertexInput;
            }

            if ((ViewFlags & BufferFlags.IndexBuffer) != 0)
            {
                createInfo.Usage |= BufferUsageFlags.IndexBuffer;
                NativeAccessMask |= AccessFlags.IndexRead;
                NativePipelineStageMask |= PipelineStageFlags.VertexInput;
            }

            if ((ViewFlags & BufferFlags.ConstantBuffer) != 0)
            {
                createInfo.Usage |= BufferUsageFlags.UniformBuffer;
                NativeAccessMask |= AccessFlags.UniformRead;
                NativePipelineStageMask |= PipelineStageFlags.VertexShader | PipelineStageFlags.FragmentShader;
            }

            if ((ViewFlags & BufferFlags.ShaderResource) != 0)
            {
                createInfo.Usage |= BufferUsageFlags.UniformTexelBuffer;
                NativeAccessMask |= AccessFlags.ShaderRead;
                NativePipelineStageMask |= PipelineStageFlags.VertexShader | PipelineStageFlags.FragmentShader;

                if ((ViewFlags & BufferFlags.UnorderedAccess) != 0)
                {
                    createInfo.Usage |= BufferUsageFlags.StorageTexelBuffer;
                    NativeAccessMask |= AccessFlags.ShaderWrite;
                }
            }

            // Create buffer
            var isStaging = bufferDescription.Usage == GraphicsResourceUsage.Staging;
            NativeBuffer = GraphicsDevice.NativeDevice.CreateBuffer(ref createInfo);

            var memoryProperties = isStaging ? MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent : MemoryPropertyFlags.DeviceLocal;

            MemoryRequirements memoryRequirements;
            GraphicsDevice.NativeDevice.GetBufferMemoryRequirements(NativeBuffer, out memoryRequirements);

            AllocateMemory(memoryProperties, memoryRequirements);

            if (NativeMemory != DeviceMemory.Null)
            {
                GraphicsDevice.NativeDevice.BindBufferMemory(NativeBuffer, NativeMemory, 0);
            }

            // Begin copy command buffer
            //var commandBuffer = GraphicsDevice.NativeCopyCommandBuffer;
            var commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                StructureType = StructureType.CommandBufferAllocateInfo,
                CommandPool = GraphicsDevice.NativeCopyCommandPool,
                CommandBufferCount = 1,
                Level = CommandBufferLevel.Primary
            };
            CommandBuffer commandBuffer;
            GraphicsDevice.NativeDevice.AllocateCommandBuffers(ref commandBufferAllocateInfo, &commandBuffer);
            var beginInfo = new CommandBufferBeginInfo { StructureType = StructureType.CommandBufferBeginInfo, Flags = CommandBufferUsageFlags.OneTimeSubmit };
            commandBuffer.Begin(ref beginInfo);

            if (Description.SizeInBytes > 0)
            {
                // Copy to upload buffer
                if (dataPointer != IntPtr.Zero)
                {
                    var sizeInBytes = bufferDescription.SizeInBytes;
                    SharpVulkan.Buffer uploadResource;
                    int uploadOffset;
                    var uploadMemory = GraphicsDevice.AllocateUploadBuffer(sizeInBytes, out uploadResource, out uploadOffset);

                    Utilities.CopyMemory(uploadMemory, dataPointer, sizeInBytes);

                    // Barrier
                    var memoryBarrier = new BufferMemoryBarrier(uploadResource, AccessFlags.HostWrite, AccessFlags.TransferRead, (ulong)uploadOffset, (ulong)sizeInBytes);
                    commandBuffer.PipelineBarrier(PipelineStageFlags.Host, PipelineStageFlags.Transfer, DependencyFlags.None, 0, null, 1, &memoryBarrier, 0, null);

                    // Copy
                    var bufferCopy = new BufferCopy
                    {
                        SourceOffset = (uint)uploadOffset,
                        DestinationOffset = 0,
                        Size = (uint)sizeInBytes
                    };
                    commandBuffer.CopyBuffer(uploadResource, NativeBuffer, 1, &bufferCopy);
                }
                else
                {
                    commandBuffer.FillBuffer(NativeBuffer, 0, (uint)bufferDescription.SizeInBytes, 0);
                }

                // Barrier
                var bufferMemoryBarrier = new BufferMemoryBarrier(NativeBuffer, AccessFlags.TransferWrite, NativeAccessMask);
                commandBuffer.PipelineBarrier(PipelineStageFlags.Transfer, PipelineStageFlags.AllCommands, DependencyFlags.None, 0, null, 1, &bufferMemoryBarrier, 0, null);

                // Close and submit
                commandBuffer.End();

                var submitInfo = new SubmitInfo
                {
                    StructureType = StructureType.SubmitInfo,
                    CommandBufferCount = 1,
                    CommandBuffers = new IntPtr(&commandBuffer),
                };

                GraphicsDevice.NativeCommandQueue.Submit(1, &submitInfo, Fence.Null);
                GraphicsDevice.NativeCommandQueue.WaitIdle();
                //commandBuffer.Reset(CommandBufferResetFlags.None);
                GraphicsDevice.NativeDevice.FreeCommandBuffers(GraphicsDevice.NativeCopyCommandPool, 1, &commandBuffer);

                // Staging resource don't have any views
                if (!isStaging)
                    InitializeViews();
            }
        }

        /// <summary>
        /// Initializes the views.
        /// </summary>
        private void InitializeViews()
        {
            var viewFormat = ViewFormat;

            if (((ViewFlags & BufferFlags.RawBuffer) != 0))
            {
                viewFormat = PixelFormat.R32_Typeless;
            }

            if ((ViewFlags & (BufferFlags.ShaderResource | BufferFlags.UnorderedAccess)) != 0)
            {
                NativeBufferView = GetShaderResourceView(viewFormat);
            }
        }

        internal unsafe BufferView GetShaderResourceView(PixelFormat viewFormat)
        {
            var createInfo = new BufferViewCreateInfo
            {
                StructureType = StructureType.BufferViewCreateInfo,
                Buffer = NativeBuffer,
                Format = viewFormat == PixelFormat.None ? Format.Undefined : VulkanConvertExtensions.ConvertPixelFormat(viewFormat),
                Range = (ulong)SizeInBytes, // this.ElementCount
                //View = (Description.BufferFlags & BufferFlags.RawBuffer) != 0 ? BufferViewType.Raw : BufferViewType.Formatted
            };

            return GraphicsDevice.NativeDevice.CreateBufferView(ref createInfo);
        }

        private void InitCountAndViewFormat(out int count, ref PixelFormat viewFormat)
        {
            if (Description.StructureByteStride == 0)
            {
                // TODO: The way to calculate the count is not always correct depending on the ViewFlags...etc.
                if ((ViewFlags & BufferFlags.RawBuffer) != 0)
                {
                    count = Description.SizeInBytes / sizeof(int);
                }
                else if ((ViewFlags & BufferFlags.ShaderResource) != 0)
                {
                    count = Description.SizeInBytes / viewFormat.SizeInBytes();
                }
                else
                {
                    count = 0;
                }
            }
            else
            {
                // For structured buffer
                count = Description.SizeInBytes / Description.StructureByteStride;
                viewFormat = PixelFormat.None;
            }
        }
    }
} 
#endif 
