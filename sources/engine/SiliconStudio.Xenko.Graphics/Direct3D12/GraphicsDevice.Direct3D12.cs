// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
using System;
using System.Collections.Generic;
using System.Threading;
using SharpDX.Direct3D12;
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
        private const GraphicsPlatform GraphicPlatform = GraphicsPlatform.Direct3D11;

        private bool simulateReset = false;

        private SharpDX.Direct3D12.Device nativeDevice;
        internal CommandQueue NativeCommandQueue;

        internal CommandAllocator NativeCopyCommandAllocator;
        internal GraphicsCommandList NativeCopyCommandList;

        internal CommandAllocatorPool CommandAllocators;
        internal HeapPool SrvHeaps;
        internal HeapPool SamplerHeaps;
        internal const int SrvHeapSize = 2048;
        internal const int SamplerHeapSize = 64;

        internal DescriptorAllocator SamplerAllocator;
        internal DescriptorAllocator ShaderResourceViewAllocator;
        internal DescriptorAllocator DepthStencilViewAllocator;
        internal DescriptorAllocator RenderTargetViewAllocator;

        private SharpDX.Direct3D12.Resource nativeUploadBuffer;
        private IntPtr nativeUploadBufferStart;
        private int nativeUploadBufferOffset;

        internal int SrvHandleIncrementSize;
        internal int SamplerHandleIncrementSize;

        private Fence nativeFence;
        private long lastCompletedFence;
        internal long NextFenceValue = 1;
        private AutoResetEvent fenceEvent = new AutoResetEvent(false);
        internal Queue<KeyValuePair<long, Pageable>> TemporaryResources = new Queue<KeyValuePair<long, Pageable>>();

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
        internal SharpDX.Direct3D12.Device NativeDevice
        {
            get
            {
                return nativeDevice;
            }
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

        public void SimulateReset()
        {
            simulateReset = true;
        }

        private void InitializePostFeatures()
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
            if ((deviceCreationFlags & DeviceCreationFlags.Debug) != 0)
            {
                SharpDX.Direct3D12.DebugInterface.Get().EnableDebugLayer();
            }

            // Create Device D3D12 with feature Level based on profile
            foreach (var level in levels)
            {
                try
                {
                    nativeDevice = new SharpDX.Direct3D12.Device(Adapter.NativeAdapter, level);
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (nativeDevice == null)
                throw new InvalidOperationException("Could not create D3D12 graphics device");

            // Describe and create the command queue.
            var queueDesc = new SharpDX.Direct3D12.CommandQueueDescription(SharpDX.Direct3D12.CommandListType.Direct);
            NativeCommandQueue = nativeDevice.CreateCommandQueue(queueDesc);

            SrvHandleIncrementSize = NativeDevice.GetDescriptorHandleIncrementSize(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
            SamplerHandleIncrementSize = NativeDevice.GetDescriptorHandleIncrementSize(DescriptorHeapType.Sampler);

            // Prepare pools
            CommandAllocators = new CommandAllocatorPool(this);
            SrvHeaps = new HeapPool(this, SrvHeapSize, DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
            SamplerHeaps = new HeapPool(this, SamplerHeapSize, DescriptorHeapType.Sampler);

            // Prepare descriptor allocators
            SamplerAllocator = new DescriptorAllocator(this, DescriptorHeapType.Sampler);
            ShaderResourceViewAllocator = new DescriptorAllocator(this, DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
            DepthStencilViewAllocator = new DescriptorAllocator(this, DescriptorHeapType.DepthStencilView);
            RenderTargetViewAllocator = new DescriptorAllocator(this, DescriptorHeapType.RenderTargetView);

            // Prepare copy command list (start it closed, so that every new use start with a Reset)
            NativeCopyCommandAllocator = NativeDevice.CreateCommandAllocator(CommandListType.Direct);
            NativeCopyCommandList = NativeDevice.CreateCommandList(CommandListType.Direct, NativeCopyCommandAllocator, null);
            NativeCopyCommandList.Close();

            // Fence for next frame and resource cleaning
            nativeFence = NativeDevice.CreateFence(0, FenceFlags.None);
        }

        internal IntPtr AllocateUploadBuffer(int size, out SharpDX.Direct3D12.Resource resource, out int offset)
        {
            // TODO D3D12 thread safety, should we simply use locks?
            if (nativeUploadBuffer == null || nativeUploadBufferOffset + size > nativeUploadBuffer.Description.Width)
            {
                if (nativeUploadBuffer != null)
                {
                    nativeUploadBuffer.Unmap(0);
                    TemporaryResources.Enqueue(new KeyValuePair<long, Pageable>(NextFenceValue, nativeUploadBuffer));
                }

                // Allocate new buffer
                // TODO D3D12 recycle old ones (using fences to know when GPU is done with them)
                // TODO D3D12 ResourceStates.CopySource not working?
                var bufferSize = Math.Max(4 * 1024*1024, size);
                nativeUploadBuffer = NativeDevice.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, ResourceDescription.Buffer(bufferSize), ResourceStates.GenericRead);
                nativeUploadBufferStart = nativeUploadBuffer.Map(0, new Range());
                nativeUploadBufferOffset = 0;
            }

            // Bump allocate
            resource = nativeUploadBuffer;
            offset = nativeUploadBufferOffset;
            nativeUploadBufferOffset += size;
            return nativeUploadBufferStart + offset;
        }

        internal void ReleaseTemporaryResources()
        {
            // Release previous frame resources
            while (TemporaryResources.Count > 0 && IsFenceCompleteInternal(TemporaryResources.Peek().Key))
            {
                var temporaryResource = TemporaryResources.Dequeue();
                temporaryResource.Value.Dispose();
            }
        }

        private void AdjustDefaultPipelineStateDescription(ref PipelineStateDescription pipelineStateDescription)
        {
        }

        protected void DestroyPlatformDevice()
        {
            ReleaseDevice();
        }

        private void ReleaseDevice()
        {
            nativeDevice.Dispose();
        }

        internal void OnDestroyed()
        {
        }

        internal long ExecuteCommandListInternal(GraphicsCommandList nativeCommandList)
        {
            NativeCommandQueue.ExecuteCommandList(nativeCommandList);
            NativeCommandQueue.Signal(nativeFence, NextFenceValue);

            return NextFenceValue++;
        }

        internal bool IsFenceCompleteInternal(long fenceValue)
        {
            // Try to avoid checking the fence if possible
            if (fenceValue > lastCompletedFence)
                lastCompletedFence = Math.Max(lastCompletedFence, nativeFence.CompletedValue); // Protect against race conditions

            return fenceValue <= lastCompletedFence;
        }

        internal void WaitForFenceInternal(long fenceValue)
        {
            if (IsFenceCompleteInternal(fenceValue))
                return;

            // TODO D3D12 in case of concurrency, this lock could end up blocking too long a second thread with lower fenceValue then first one
            lock (nativeFence)
            {
                nativeFence.SetEventOnCompletion(fenceValue, fenceEvent.SafeWaitHandle.DangerousGetHandle());
                fenceEvent.WaitOne();
                lastCompletedFence = fenceValue;
            }
        }
        
        internal abstract class ResourcePool<T> where T : Pageable
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
                        if (firstAllocator.Key <= GraphicsDevice.nativeFence.CompletedValue)
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

        internal class CommandAllocatorPool : ResourcePool<CommandAllocator>
        {
            public CommandAllocatorPool(GraphicsDevice graphicsDevice) : base(graphicsDevice)
            {
            }

            protected override CommandAllocator CreateObject()
            {
                // No allocator ready to be used, let's create a new one
                return GraphicsDevice.NativeDevice.CreateCommandAllocator(CommandListType.Direct);
            }

            protected override void ResetObject(CommandAllocator obj)
            {
                obj.Reset();
            }
        }

        internal class HeapPool : ResourcePool<DescriptorHeap>
        {
            private readonly int heapSize;
            private readonly DescriptorHeapType heapType;

            public HeapPool(GraphicsDevice graphicsDevice, int heapSize, DescriptorHeapType heapType) : base(graphicsDevice)
            {
                this.heapSize = heapSize;
                this.heapType = heapType;
            }

            protected override DescriptorHeap CreateObject()
            {
                // No allocator ready to be used, let's create a new one
                return GraphicsDevice.NativeDevice.CreateDescriptorHeap(new DescriptorHeapDescription
                {
                    DescriptorCount = heapSize,
                    Flags = DescriptorHeapFlags.ShaderVisible,
                    Type = heapType,
                });
            }

            protected override void ResetObject(DescriptorHeap obj)
            {
            }
        }

        /// <summary>
        /// Allocate descriptor handles. For now a simple bump alloc, but at some point we will have to make a real allocator with free
        /// </summary>
        internal class DescriptorAllocator
        {
            private const int DescriptorPerHeap = 256;

            private GraphicsDevice device;
            private DescriptorHeapType descriptorHeapType;
            private DescriptorHeap currentHeap;
            private CpuDescriptorHandle currentHandle;
            private int remainingHandles;
            private readonly int descriptorSize;

            public DescriptorAllocator(GraphicsDevice device, DescriptorHeapType descriptorHeapType)
            {
                this.device = device;
                this.descriptorHeapType = descriptorHeapType;
                this.descriptorSize = device.NativeDevice.GetDescriptorHandleIncrementSize(descriptorHeapType);
            }

            public CpuDescriptorHandle Allocate(int count)
            {
                if (currentHeap == null || remainingHandles < count)
                {
                    currentHeap = device.NativeDevice.CreateDescriptorHeap(new DescriptorHeapDescription
                    {
                        Flags = DescriptorHeapFlags.None,
                        Type = descriptorHeapType,
                        DescriptorCount = DescriptorPerHeap,
                        NodeMask = 1,
                    });
                    remainingHandles = DescriptorPerHeap;
                    currentHandle = currentHeap.CPUDescriptorHandleForHeapStart;
                }

                var result = currentHandle;

                currentHandle.Ptr += descriptorSize;
                remainingHandles -= count;

                return result;
            }
        }
    }
}
#endif
