// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// GraphicsResource class
    /// </summary>
    public abstract partial class GraphicsResourceBase
    {
        protected internal SharpDX.Direct3D11.DeviceChild _nativeDeviceChild;

        protected internal SharpDX.Direct3D11.Resource NativeResource;

        private void Initialize()
        {
            if (GraphicsDevice != null)
                NativeDevice = GraphicsDevice.NativeDevice;
        }

        /// <summary>
        /// Gets or sets the device child.
        /// </summary>
        /// <value>The device child.</value>
        protected internal SharpDX.Direct3D11.DeviceChild NativeDeviceChild
        {
            get
            {
                return _nativeDeviceChild;
            }
            set
            {
                if (_nativeDeviceChild != null) throw new ArgumentException(string.Format(FrameworkResources.GraphicsResourceAlreadySet, "DeviceChild"), "value");
                _nativeDeviceChild = value;

                if (_nativeDeviceChild is SharpDX.Direct3D11.Resource)
                    NativeResource = (SharpDX.Direct3D11.Resource)_nativeDeviceChild;

                // Associate PrivateData to this DeviceResource
                SetDebugName(GraphicsDevice, _nativeDeviceChild, Name);
            }
        }

        protected virtual void DestroyImpl()
        {
            if (_nativeDeviceChild != null)
            {
                ((IUnknown)_nativeDeviceChild).Release();
                _nativeDeviceChild = null;
                NativeResource = null;
            }
        }

        /// <summary>
        /// Associates the private data to the device child, useful to get the name in PIX debugger.
        /// </summary>
        internal static void SetDebugName(GraphicsDevice graphicsDevice, SharpDX.Direct3D11.DeviceChild deviceChild, string name)
        {
            if (graphicsDevice.IsDebugMode && deviceChild != null)
            {
                IntPtr namePtr = SharpDX.Utilities.StringToHGlobalAnsi(name);
                deviceChild.SetPrivateData(CommonGuid.DebugObjectName, name.Length, namePtr);
                // TODO Should PrivateData be deallocated now or keep a reference to it?
            }
        }

        /// <summary>
        /// Associates the private data to the device child, useful to get the name in PIX debugger.
        /// </summary>
        internal static void SetDebugName(GraphicsDevice graphicsDevice, DXGIObject dxgiObject, string name)
        {
            if (graphicsDevice.IsDebugMode)
            {
                IntPtr namePtr = SharpDX.Utilities.StringToHGlobalAnsi(name);
                dxgiObject.SetPrivateData(CommonGuid.DebugObjectName, name.Length, namePtr);
            }
        }

        /// <summary>
        /// Called when graphics device has been detected to be internally destroyed.
        /// </summary>
        protected internal virtual void OnDestroyed()
        {
            NativeDevice = null;
        }

        /// <summary>
        /// Called when graphics device has been recreated.
        /// </summary>
        /// <returns>True if item transitionned to a <see cref="GraphicsResourceLifetimeState.Active"/> state.</returns>
        protected internal virtual bool OnRecreate()
        {
            NativeDevice = GraphicsDevice.NativeDevice;
            return false;
        }

        protected SharpDX.Direct3D11.Device NativeDevice
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the cpu access flags from resource usage.
        /// </summary>
        /// <param name="usage">The usage.</param>
        /// <returns></returns>
        internal static SharpDX.Direct3D11.CpuAccessFlags GetCpuAccessFlagsFromUsage(GraphicsResourceUsage usage)
        {
            switch (usage)
            {
                case GraphicsResourceUsage.Dynamic:
                    return SharpDX.Direct3D11.CpuAccessFlags.Write;
                case GraphicsResourceUsage.Staging:
                    return SharpDX.Direct3D11.CpuAccessFlags.Read;
            }
            return SharpDX.Direct3D11.CpuAccessFlags.None;
        }
    }
}
 
#endif
