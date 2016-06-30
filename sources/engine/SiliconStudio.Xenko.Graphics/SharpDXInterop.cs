// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using SharpDX;
using SharpDX.Direct3D11;

namespace SiliconStudio.Xenko.Graphics
{
    public static class SharpDXInterop
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
        /// <param name="commandList">The Xenko CommandList</param>
        /// <returns></returns>
        public static DeviceContext GetNativeDeviceContext(CommandList commandList)
        {
            return commandList.NativeDeviceContext;
        }

        /// <summary>
        /// Gets the DX11 native resource handle
        /// </summary>
        /// <param name="resource">The Xenko GraphicsResourceBase</param>
        /// <returns></returns>
        public static Resource GetNativeResource(GraphicsResource resource)
        {
            return resource.NativeResource;
        }

        /// <summary>
        /// Creates a Texture from a DirectX11 native texture
        /// This method internally will call AddReference on the dxTexture2D texture.
        /// </summary>
        /// <param name="device">The GraphicsDevice in use</param>
        /// <param name="dxTexture2D">The DX11 texture</param>
        /// <param name="takeOwnership">If false AddRef will be called on the texture, if true will not, effectively taking ownership</param>
        /// <returns></returns>
        public static Texture CreateTextureFromNative(GraphicsDevice device, Texture2D dxTexture2D, bool takeOwnership)
        {
            var tex = new Texture(device);

            if (takeOwnership)
            {
                var unknown = dxTexture2D as IUnknown;
                unknown.AddReference();
            }

            tex.InitializeFrom(dxTexture2D, false);

            return tex;
        }
    }
}

#endif
