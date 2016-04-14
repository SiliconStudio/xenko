// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpVulkan;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class GraphicsDevice
    {
        private const GraphicsPlatform GraphicPlatform = GraphicsPlatform.Vulkan;

        private bool simulateReset = false;
        private string rendererName;

        private Device nativeDevice;
        internal Queue NativeCommandQueue;

        internal CommandPool NativeCopyCommandPool;
        internal CommandBuffer NativeCopyCommandBuffer;

        private SharpVulkan.Buffer nativeUploadBuffer;
        private DeviceMemory nativeUploadBufferMemory;
        private IntPtr nativeUploadBufferStart;
        private int nativeUploadBufferSize;
        private int nativeUploadBufferOffset;

        private Queue<KeyValuePair<long, Fence>> nativeFences = new Queue<KeyValuePair<long, Fence>>();
        private long lastCompletedFence;
        internal long NextFenceValue = 1;
        internal Queue<BufferInfo> TemporaryResources = new Queue<BufferInfo>();

        internal HeapPool descriptorPools;

        internal struct BufferInfo
        {
            public long FenceValue;

            public SharpVulkan.Buffer Buffer;

            public DeviceMemory Memory;

            public BufferInfo(long fenceValue, SharpVulkan.Buffer buffer, DeviceMemory memory)
            {
                FenceValue = fenceValue;
                Buffer = buffer;
                Memory = memory;
            }
        }

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

                //var result = NativeDevice.DeviceRemovedReason;
                //if (result == SharpDX.DXGI.ResultCode.DeviceRemoved)
                //{
                //    return GraphicsDeviceStatus.Removed;
                //}

                //if (result == SharpDX.DXGI.ResultCode.DeviceReset)
                //{
                //    return GraphicsDeviceStatus.Reset;
                //}

                //if (result == SharpDX.DXGI.ResultCode.DeviceHung)
                //{
                //    return GraphicsDeviceStatus.Hung;
                //}

                //if (result == SharpDX.DXGI.ResultCode.DriverInternalError)
                //{
                //    return GraphicsDeviceStatus.InternalError;
                //}

                //if (result == SharpDX.DXGI.ResultCode.InvalidCall)
                //{
                //    return GraphicsDeviceStatus.InvalidCall;
                //}

                //if (result.Code < 0)
                //{
                //    return GraphicsDeviceStatus.Reset;
                //}

                return GraphicsDeviceStatus.Normal;
            }
        }

        /// <summary>
        ///     Gets the native device.
        /// </summary>
        /// <value>The native device.</value>
        internal Device NativeDevice
        {
            get { return nativeDevice; }
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
        /// Executes a deferred command list.
        /// </summary>
        /// <param name="commandList">The deferred command list.</param>
        public void ExecuteCommandList(CommandList commandList)
        {
            //if (commandList == null) throw new ArgumentNullException("commandList");
            //
            //NativeDeviceContext.ExecuteCommandList(((CommandList)commandList).NativeCommandList, false);
            //commandList.Dispose();
        }

        private void InitializePostFeatures()
        {
        }

        private string GetRendererName()
        {
            return rendererName;
        }

        public void SimulateReset()
        {
            simulateReset = true;
        }

        /// <summary>
        ///     Initializes the specified device.
        /// </summary>
        /// <param name="graphicsProfiles">The graphics profiles.</param>
        /// <param name="deviceCreationFlags">The device creation flags.</param>
        /// <param name="windowHandle">The window handle.</param>
        private unsafe void InitializePlatformDevice(GraphicsProfile[] graphicsProfiles, DeviceCreationFlags deviceCreationFlags, object windowHandle)
        {
            if (nativeDevice != Device.Null)
            {
                // Destroy previous device
                ReleaseDevice();
            }

            rendererName = Adapter.Description;

            // Profiling is supported through pix markers
            IsProfilingSupported = true;

            if ((deviceCreationFlags & DeviceCreationFlags.Debug) != 0)
            {
                // TODO VULKAN debug layer
            }

            var queueProperties = Adapter.PhysicalDevice.QueueFamilyProperties;

            // TODO VULKAN
            // Create Vulkan device based on profile
            uint queuePriorities = 0;
            var queueCreateInfo = new DeviceQueueCreateInfo
            {
                StructureType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = 0,
                QueueCount = 1,
                QueuePriorities = new IntPtr(&queuePriorities)
            };

            var enabledLayerNames = new[]
            {
                //Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_standard_validation"),

                Marshal.StringToHGlobalAnsi("VK_LAYER_GOOGLE_threading"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_parameter_validation"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_device_limits"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_object_tracker"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_image"),
                //Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_mem_tracker"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_core_validation"),
                //Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_swapchain"),
                //Marshal.StringToHGlobalAnsi("VK_LAYER_GOOGLE_unique_objects"),
            };

            var enabledFeature = new PhysicalDeviceFeatures
            {
                FillModeNonSolid = true,
                ShaderClipDistance = true,
                SamplerAnisotropy = true,
                DepthClamp = true,
            };

            var enabledExtensionNames = new[]
            {
                Marshal.StringToHGlobalAnsi("VK_KHR_swapchain"),
            };

            try
            {
                fixed (void* enabledLayerNamesPointer = &enabledLayerNames[0])
                fixed (void* enabledExtensionNamesPointer = &enabledExtensionNames[0])
                {
                    var deviceCreateInfo = new DeviceCreateInfo
                    {
                        StructureType = StructureType.DeviceCreateInfo,
                        QueueCreateInfoCount = 1,
                        QueueCreateInfos = new IntPtr(&queueCreateInfo),
                        //EnabledLayerCount = (uint)enabledLayerNames.Length,
                        //EnabledLayerNames = new IntPtr(enabledLayerNamesPointer),
                        EnabledExtensionCount = (uint)enabledExtensionNames.Length,
                        EnabledExtensionNames = new IntPtr(enabledExtensionNamesPointer),
                        EnabledFeatures = new IntPtr(&enabledFeature)
                    };

                    nativeDevice = Adapter.PhysicalDevice.CreateDevice(ref deviceCreateInfo);
                }
            }
            finally
            {
                foreach (var enabledExtensionName in enabledExtensionNames)
                {
                    Marshal.FreeHGlobal(enabledExtensionName);
                }

                foreach (var enabledLayerName in enabledLayerNames)
                {
                    Marshal.FreeHGlobal(enabledLayerName);
                }
            }

            NativeCommandQueue = nativeDevice.GetQueue(0, 0);

            //SrvHandleIncrementSize = NativeDevice.GetDescriptorHandleIncrementSize(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
            //SamplerHandleIncrementSize = NativeDevice.GetDescriptorHandleIncrementSize(DescriptorHeapType.Sampler);

            //// Prepare descriptor allocators
            //ShaderResourceViewAllocator = new DescriptorAllocator(this, DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
            //DepthStencilViewAllocator = new DescriptorAllocator(this, DescriptorHeapType.DepthStencilView);
            //RenderTargetViewAllocator = new DescriptorAllocator(this, DescriptorHeapType.RenderTargetView);

            //// Prepare copy command list (start it closed, so that every new use start with a Reset)
            var commandPoolCreateInfo = new CommandPoolCreateInfo
            {
                StructureType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = 0, //device.NativeCommandQueue.FamilyIndex
                Flags = CommandPoolCreateFlags.ResetCommandBuffer
            };
            NativeCopyCommandPool = NativeDevice.CreateCommandPool(ref commandPoolCreateInfo);

            var commandBufferAllocationInfo = new CommandBufferAllocateInfo
            {
                StructureType = StructureType.CommandBufferAllocateInfo,
                Level = CommandBufferLevel.Primary,
                CommandPool = NativeCopyCommandPool,
                CommandBufferCount = 1
            };
            CommandBuffer nativeCommandBuffer;
            NativeDevice.AllocateCommandBuffers(ref commandBufferAllocationInfo, &nativeCommandBuffer);
            NativeCopyCommandBuffer = nativeCommandBuffer;
            //NativeCopyCommandAllocator = NativeDevice.CreateCommandAllocator(CommandListType.Direct);
            //NativeCopyCommandList = NativeDevice.CreateCommandList(CommandListType.Direct, NativeCopyCommandAllocator, null);
            //NativeCopyCommandList.Close();

            descriptorPools = new HeapPool(this, 1 << 16);
        }

        internal unsafe IntPtr AllocateUploadBuffer(int size, out SharpVulkan.Buffer resource, out int offset)
        {
            // TODO D3D12 thread safety, should we simply use locks?
            if (nativeUploadBuffer == SharpVulkan.Buffer.Null || nativeUploadBufferOffset + size > nativeUploadBufferSize)
            {
                if (nativeUploadBuffer != SharpVulkan.Buffer.Null)
                {
                    NativeDevice.UnmapMemory(nativeUploadBufferMemory);
                    TemporaryResources.Enqueue(new BufferInfo(NextFenceValue, nativeUploadBuffer, nativeUploadBufferMemory));
                }

                // Allocate new buffer
                // TODO D3D12 recycle old ones (using fences to know when GPU is done with them)
                // TODO D3D12 ResourceStates.CopySource not working?
                nativeUploadBufferSize = Math.Max(4 * 1024 * 1024, size);

                var bufferCreateInfo = new BufferCreateInfo
                {
                    StructureType = StructureType.BufferCreateInfo,
                    Size = (ulong)nativeUploadBufferSize,
                    Flags = BufferCreateFlags.None,
                    Usage = BufferUsageFlags.TransferSource,
                };
                nativeUploadBuffer = NativeDevice.CreateBuffer(ref bufferCreateInfo);
                AllocateMemory(MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent);

                nativeUploadBufferStart = NativeDevice.MapMemory(nativeUploadBufferMemory, 0, (ulong)nativeUploadBufferSize, MemoryMapFlags.None);
                nativeUploadBufferOffset = 0;
            }

            // Bump allocate
            resource = nativeUploadBuffer;
            offset = nativeUploadBufferOffset;
            nativeUploadBufferOffset += size;
            return nativeUploadBufferStart + offset;
        }

        protected unsafe void AllocateMemory(MemoryPropertyFlags memoryProperties)
        {
            MemoryRequirements memoryRequirements;
            NativeDevice.GetBufferMemoryRequirements(nativeUploadBuffer, out memoryRequirements);

            if (memoryRequirements.Size == 0)
                return;

            var allocateInfo = new MemoryAllocateInfo
            {
                StructureType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirements.Size,
            };

            PhysicalDeviceMemoryProperties physicalDeviceMemoryProperties;
            Adapter.PhysicalDevice.GetMemoryProperties(out physicalDeviceMemoryProperties);
            var typeBits = memoryRequirements.MemoryTypeBits;
            for (uint i = 0; i < physicalDeviceMemoryProperties.MemoryTypeCount; i++)
            {
                if ((typeBits & 1) == 1)
                {
                    // Type is available, does it match user properties?
                    var memoryType = *((MemoryType*)&physicalDeviceMemoryProperties.MemoryTypes + i);
                    if ((memoryType.PropertyFlags & memoryProperties) == memoryProperties)
                    {
                        allocateInfo.MemoryTypeIndex = i;
                        break;
                    }
                }
                typeBits >>= 1;
            }

            nativeUploadBufferMemory = NativeDevice.AllocateMemory(ref allocateInfo);

            NativeDevice.BindBufferMemory(nativeUploadBuffer, nativeUploadBufferMemory, 0);
        }

        internal unsafe void ReleaseTemporaryResources()
        {
            // Release previous frame resources
            while (TemporaryResources.Count > 0 && IsFenceCompleteInternal(TemporaryResources.Peek().FenceValue))
            {
                var temporaryResource = TemporaryResources.Dequeue();
                NativeDevice.FreeMemory(temporaryResource.Memory);
                NativeDevice.DestroyBuffer(temporaryResource.Buffer);
            }
        }

        private void AdjustDefaultPipelineStateDescription(ref PipelineStateDescription pipelineStateDescription)
        {
        }

        protected void DestroyPlatformDevice()
        {
            ReleaseDevice();
        }

        private unsafe void ReleaseDevice()
        {
            nativeDevice.WaitIdle();

            ReleaseTemporaryResources();
            nativeDevice.DestroyCommandPool(NativeCopyCommandPool);
            nativeDevice.Destroy();
        }

        internal void OnDestroyed()
        {
        }

        internal unsafe long ExecuteCommandListInternal(CommandBuffer nativeCommandBuffer)
        {
            //if (nativeUploadBuffer != SharpVulkan.Buffer.Null)
            //{
            //    NativeDevice.UnmapMemory(nativeUploadBufferMemory);
            //    TemporaryResources.Enqueue(new BufferInfo(NextFenceValue, nativeUploadBuffer, nativeUploadBufferMemory));

            //    nativeUploadBuffer = SharpVulkan.Buffer.Null;
            //    nativeUploadBufferMemory = DeviceMemory.Null;
            //}

            // Create new fence
            var fenceCreateInfo = new FenceCreateInfo { StructureType = StructureType.FenceCreateInfo };
            var fence = nativeDevice.CreateFence(ref fenceCreateInfo);
            nativeFences.Enqueue(new KeyValuePair<long, Fence>(NextFenceValue, fence));

            // Submit commands
            var nativeCommandBufferCopy = nativeCommandBuffer;
            var pipelineStageFlags = PipelineStageFlags.AllCommands;

            var submitInfo = new SubmitInfo
            {
                StructureType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                CommandBuffers = new IntPtr(&nativeCommandBufferCopy),
                WaitDstStageMask = new IntPtr(&pipelineStageFlags),
            };
            NativeCommandQueue.Submit(1, &submitInfo, fence);

            return NextFenceValue++;
        }

        internal bool IsFenceCompleteInternal(long fenceValue)
        {
            // Try to avoid checking the fence if possible
            if (fenceValue > lastCompletedFence)
            {
                GetCompletedValue();
            }

            return fenceValue <= lastCompletedFence;
        }

        internal unsafe long GetCompletedValue()
        {
            // TODO VULKAN: SpinLock this
            while (nativeFences.Count > 0 && NativeDevice.GetFenceStatus(nativeFences.Peek().Value) == Result.Success)
            {
                var fence = nativeFences.Dequeue();
                NativeDevice.DestroyFence(fence.Value);
                lastCompletedFence = Math.Max(lastCompletedFence, fence.Key);
            }

            return lastCompletedFence;
        }

        internal unsafe void WaitForFenceInternal(long fenceValue)
        {
            if (IsFenceCompleteInternal(fenceValue))
                return;

            // TODO D3D12 in case of concurrency, this lock could end up blocking too long a second thread with lower fenceValue then first one
            lock (nativeFences)
            {
                while (nativeFences.Count > 0 && nativeFences.Peek().Key <= fenceValue)
                {
                    var fence = nativeFences.Dequeue();
                    var fenceCopy = fence.Value;

                    NativeDevice.WaitForFences(1, &fenceCopy, true, ulong.MaxValue);
                    NativeDevice.DestroyFence(fence.Value);
                    lastCompletedFence = fenceValue;
                }
            }
        }
    }

    internal abstract class ResourcePool<T>
    {
        protected readonly GraphicsDevice GraphicsDevice;
        private readonly Queue<KeyValuePair<long, T>> liveObjects = new Queue<KeyValuePair<long, T>>();

        protected ResourcePool(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        public T GetObject()
        {
            lock (liveObjects)
            {
                // Check if first allocator is ready for reuse
                if (liveObjects.Count > 0)
                {
                    var firstAllocator = liveObjects.Peek();
                    if (firstAllocator.Key <= GraphicsDevice.GetCompletedValue())
                    {
                        liveObjects.Dequeue();
                        ResetObject(firstAllocator.Value);
                        return firstAllocator.Value;
                    }
                }

                return CreateObject();
            }
        }

        protected abstract T CreateObject();

        protected abstract void ResetObject(T obj);

        public void RecycleObject(long fenceValue, T obj)
        {
            lock (liveObjects)
            {
                liveObjects.Enqueue(new KeyValuePair<long, T>(fenceValue, obj));
            }
        }
    }

    internal class CommandBufferPool : ResourcePool<CommandBuffer>
    {
        private readonly CommandPool commandPool;

        public unsafe CommandBufferPool(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            var commandPoolCreateInfo = new CommandPoolCreateInfo
            {
                StructureType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = 0, //device.NativeCommandQueue.FamilyIndex
                Flags = CommandPoolCreateFlags.ResetCommandBuffer
            };

            commandPool = graphicsDevice.NativeDevice.CreateCommandPool(ref commandPoolCreateInfo);
        }

        protected unsafe override CommandBuffer CreateObject()
        {
            // No allocator ready to be used, let's create a new one
            var commandBufferAllocationInfo = new CommandBufferAllocateInfo
            {
                StructureType = StructureType.CommandBufferAllocateInfo,
                Level = CommandBufferLevel.Primary,
                CommandPool = commandPool,
                CommandBufferCount = 1,
            };

            CommandBuffer commandBuffer;
            GraphicsDevice.NativeDevice.AllocateCommandBuffers(ref commandBufferAllocationInfo, &commandBuffer);
            return commandBuffer;
        }

        protected unsafe override void ResetObject(CommandBuffer obj)
        {
            obj.Reset(CommandBufferResetFlags.None);
        }
    }

    internal class HeapPool : ResourcePool<HeapPool.DescriptorAllocation>
    {
        public class DescriptorAllocation
        {
            public SharpVulkan.DescriptorPool Pool;
            public FastList<SharpVulkan.DescriptorSet> Sets = new FastList<SharpVulkan.DescriptorSet>(); 
        };

        private readonly uint heapSize;

        public HeapPool(GraphicsDevice graphicsDevice, uint heapSize) : base(graphicsDevice)
        {
            this.heapSize = heapSize;
        }

        protected unsafe override DescriptorAllocation CreateObject()
        {
            // No allocator ready to be used, let's create a new one
            var poolSizes = new[]
            {
                new DescriptorPoolSize { Type = DescriptorType.SampledImage, DescriptorCount = heapSize },
                new DescriptorPoolSize { Type = DescriptorType.Sampler, DescriptorCount = heapSize },
                new DescriptorPoolSize { Type = DescriptorType.UniformBuffer, DescriptorCount = heapSize },
            };

            var descriptorPoolCreateInfo = new DescriptorPoolCreateInfo
            {
                StructureType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PoolSizes = new IntPtr(Interop.Fixed(poolSizes)),
                MaxSets = heapSize,
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSet,
            };
            return new DescriptorAllocation { Pool = GraphicsDevice.NativeDevice.CreateDescriptorPool(ref descriptorPoolCreateInfo) };
        }

        protected unsafe override void ResetObject(DescriptorAllocation obj)
        {
            // TODO VULKAN: Resetting the pool crashes. Only when using DescriptorSetCopies to the sets though.

            //GraphicsDevice.NativeDevice.ResetDescriptorPool(obj, DescriptorPoolResetFlags.None);
            GraphicsDevice.NativeDevice.FreeDescriptorSets(obj.Pool, (uint)obj.Sets.Count, obj.Sets.Count > 0 ? (SharpVulkan.DescriptorSet*)Interop.Fixed(obj.Sets.Items) : null);
            obj.Sets.Clear(true);
        }
    }

    internal class FramebufferCollector : TemporaryResourceCollector<Framebuffer>
    {
        public FramebufferCollector(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        protected unsafe override void ReleaseObject(Framebuffer item)
        {
            GraphicsDevice.NativeDevice.DestroyFramebuffer(item);
        }
    }


    internal abstract class TemporaryResourceCollector<T>
    {
        protected readonly GraphicsDevice GraphicsDevice;
        private readonly Queue<KeyValuePair<long, T>> items = new Queue<KeyValuePair<long, T>>();

        protected TemporaryResourceCollector(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        public void Add(long fenceValue, T item)
        {
            lock (items)
            {
                items.Enqueue(new KeyValuePair<long, T>(fenceValue, item));
            }
        }

        public void Release()
        {
            lock (items)
            {
                while (items.Count > 0 && GraphicsDevice.IsFenceCompleteInternal(items.Peek().Key))
                {
                    ReleaseObject(items.Dequeue().Value);
                }
            }
        }

        protected abstract void ReleaseObject(T item);
    }
}
#endif
