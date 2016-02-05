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
        private const GraphicsPlatform GraphicPlatform = GraphicsPlatform.Direct3D11;

        private bool simulateReset = false;

        private SharpDX.Direct3D11.Device nativeDevice;
        private SharpDX.Direct3D11.DeviceContext nativeDeviceContext;

        private SharpDX.Direct3D11.DeviceCreationFlags creationFlags;

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
            isDeferred = true;
            IsDebugMode = device.IsDebugMode;
            if (IsDebugMode)
            {
                GraphicsResourceBase.SetDebugName(device, nativeDeviceContext, "DeferredContext");
            }
            NeedWorkAroundForUpdateSubResource = !Features.HasDriverCommandLists;

            PrimitiveQuad = new PrimitiveQuad(this).DisposeBy(this);
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

        // TODO GRAPHICS REFACTOR what should we do with this?
        public void UnmapSubresource(MappedResource unmapped)
        {
            NativeDeviceContext.UnmapSubresource(unmapped.Resource.NativeResource, unmapped.SubResourceIndex);
        }

        // TODO GRAPHICS REFACTOR kept around for SetData/GetData
        public void Copy(GraphicsResource source, GraphicsResource destination)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            NativeDeviceContext.CopyResource(source.NativeResource, destination.NativeResource);
        }

        /// <summary>
        /// Creates a new deferred device used for multithread deferred rendering.
        /// </summary>
        /// <returns>GraphicsDevice.</returns>
        public GraphicsDevice NewDeferred()
        {
            return new GraphicsDevice(RootDevice);
        }

        public void SimulateReset()
        {
            simulateReset = true;
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
        }

        protected void DestroyPlatformDevice()
        {
            ReleaseDevice();
        }

        private void ReleaseDevice()
        {
            // Display D3D11 ref counting info
            //ClearState();
            NativeDevice.ImmediateContext.Flush();
            NativeDevice.ImmediateContext.Dispose();

            if (IsDebugMode)
            {
                var deviceDebug = new SharpDX.Direct3D11.DeviceDebug(NativeDevice);
                deviceDebug.ReportLiveDeviceObjects(SharpDX.Direct3D11.ReportingLevel.Detail);
            }

            nativeDevice.Dispose();
        }

        internal void OnDestroyed()
        {
        }
    }
}
#endif
