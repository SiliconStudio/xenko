// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Extensions for the <see cref="GraphicsResourceAllocator"/>.
    /// </summary>
    public static class GraphicsResourceAllocatorExtensions
    {
        /// <summary>
        /// Gets a <see cref="RenderTarget" /> output for the specified description.
        /// </summary>
        /// <returns>A new instance of <see cref="RenderTarget" /> class.</returns>
        public static RenderTarget GetTemporaryRenderTarget2D(this GraphicsResourceAllocator allocator, TextureDescription description)
        {
            return allocator.GetTemporaryTexture(description).ToRenderTarget();
        }

        /// <summary>
        /// Gets a <see cref="RenderTarget" /> output for the specified description with a single mipmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="flags">Sets the texture flags (for unordered access...etc.)</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>A new instance of <see cref="RenderTarget" /> class.</returns>
        /// <msdn-id>ff476521</msdn-id>
        ///   <unmanaged>HRESULT ID3D11Device::CreateTexture2D([In] const D3D11_TEXTURE2D_DESC* pDesc,[In, Buffer, Optional] const D3D11_SUBRESOURCE_DATA* pInitialData,[Out, Fast] ID3D11Texture2D** ppTexture2D)</unmanaged>
        ///   <unmanaged-short>ID3D11Device::CreateTexture2D</unmanaged-short>
        public static RenderTarget GetTemporaryRenderTarget2D(this GraphicsResourceAllocator allocator, int width, int height, PixelFormat format, TextureFlags flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource, int arraySize = 1)
        {
            return allocator.GetTemporaryTexture(Texture2DBase.NewDescription(width, height, format, flags, 1, arraySize, GraphicsResourceUsage.Default)).ToRenderTarget();
        }

        /// <summary>
        /// Gets a <see cref="RenderTarget" /> output for the specified description.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int &gt;=1 for a particular mipmap count.</param>
        /// <param name="flags">Sets the texture flags (for unordered access...etc.)</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>A new instance of <see cref="RenderTarget" /> class.</returns>
        /// <msdn-id>ff476521</msdn-id>
        ///   <unmanaged>HRESULT ID3D11Device::CreateTexture2D([In] const D3D11_TEXTURE2D_DESC* pDesc,[In, Buffer, Optional] const D3D11_SUBRESOURCE_DATA* pInitialData,[Out, Fast] ID3D11Texture2D** ppTexture2D)</unmanaged>
        ///   <unmanaged-short>ID3D11Device::CreateTexture2D</unmanaged-short>
        public static RenderTarget GetTemporaryRenderTarget2D(this GraphicsResourceAllocator allocator, int width, int height, PixelFormat format, MipMapCount mipCount, TextureFlags flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource, int arraySize = 1)
        {
            return allocator.GetTemporaryTexture(Texture2DBase.NewDescription(width, height, format, flags, mipCount, arraySize, GraphicsResourceUsage.Default)).ToRenderTarget();
        }        
    }
}