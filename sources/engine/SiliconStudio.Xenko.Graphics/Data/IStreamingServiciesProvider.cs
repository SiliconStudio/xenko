// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Streaming;

namespace SiliconStudio.Xenko.Graphics.Data
{
    /// <summary>
    /// Used internally to find the currently active textures streaming service
    /// </summary>
    public interface ITexturesStreamingProvider
    {
        /// <summary>
        /// Loads the texture in a streaming service.
        /// </summary>
        /// <param name="obj">The texture object.</param>
        /// <param name="imageDescription">The image description.</param>
        /// <param name="storageHeader">The storage header.</param>
        void FullyLoadTexture(Texture obj, ref ImageDescription imageDescription, ContentStorageHeader storageHeader);

        /// <summary>
        /// Registers the texture in a streaming service.
        /// </summary>
        /// <param name="obj">The texture object.</param>
        /// <param name="imageDescription">The image description.</param>
        /// <param name="storageHeader">The storage header.</param>
        void RegisterTexture(Texture obj, ref ImageDescription imageDescription, ContentStorageHeader storageHeader);

        /// <summary>
        /// Unregisters the texture.
        /// </summary>
        /// <param name="obj">The texture object.</param>
        void UnregisterTexture(Texture obj);
    }
}
