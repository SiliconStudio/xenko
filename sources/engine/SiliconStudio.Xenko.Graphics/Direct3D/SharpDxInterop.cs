// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using SharpDX;
using SharpDX.Direct3D11;

namespace SiliconStudio.Xenko.Graphics.Direct3D
{
    public static class SharpDxInterop
    {
        /// <summary>
        /// Gets the DX11 native device
        /// </summary>
        /// <param name="device">The Xenko GraphicsDevice</param>
        /// <returns></returns>
        public static Device GetNativeDevice(GraphicsDevice device)
        {
            return device.NativeDevice;
        }

        /// <summary>
        /// Gets the DX11 native device context
        /// </summary>
        /// <param name="device">The Xenko GraphicsDevice</param>
        /// <returns></returns>
        public static DeviceContext GetNativeDeviceContext(GraphicsDevice device)
        {
            return device.NativeDeviceContext;
        }

        /// <summary>
        /// Gets the DX11 native resource handle
        /// </summary>
        /// <param name="resource">The Xenko GraphicsResourceBase</param>
        /// <returns></returns>
        public static Resource GetNativeResource(GraphicsResourceBase resource)
        {
            return resource.NativeResource;
        }

        /// <summary>
        /// Creates a Texture from a DirectX11 native texture
        /// This method internally will call AddReference on the dxTexture2D texture.
        /// </summary>
        /// <param name="device">The GraphicsDevice in use</param>
        /// <param name="dxTexture2D">The DX11 texture</param>
        /// <param name="isSrgb">If the texture is SRGB</param>
        /// <returns></returns>
        public static Texture CreateTextureFromNative(GraphicsDevice device, Texture2D dxTexture2D, bool isSrgb)
        {
            var tex = new Texture(device);
            var unknown = dxTexture2D as IUnknown;
            unknown.AddReference();
            tex.InitializeFrom(dxTexture2D, isSrgb);
            return tex;
        }
    }
}

#endif
