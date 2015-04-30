// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Serialization.Assets
{
    /// <summary>
    /// Interface of the asset manager.
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// Check if the specified asset url exists.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns><c>true</c> if the specified asset url exists, <c>false</c> otherwise.</returns>
        bool Exists(string url);

        /// <summary>
        /// Opens the specified URL as a stream used for custom raw asset loading.
        /// </summary>
        /// <param name="url">The URL to the raw asset.</param>
        /// <param name="streamFlags">The type of stream needed</param>
        /// <returns>A stream to the raw asset.</returns>
        Stream OpenAsStream(string url, StreamFlags streamFlags = StreamFlags.None);

        /// <summary>
        /// Loads content from the specified URL.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="url">The URL to load from.</param>
        /// <param name="settings">The settings. If null, fallback to <see cref="AssetManagerLoaderSettings.Default"/>.</param>
        /// <returns></returns>
        T Load<T>(string url, AssetManagerLoaderSettings settings = null) where T : class;

        /// <summary>
        /// Loads content from the specified URL asynchronously.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="url">The URL to load from.</param>
        /// <param name="settings">The settings. If null, fallback to <see cref="AssetManagerLoaderSettings.Default"/>.</param>
        /// <returns></returns>
        Task<T> LoadAsync<T>(string url, AssetManagerLoaderSettings settings = null) where T : class;

        /// <summary>
        /// Unloads the specified object.
        /// </summary>
        /// <param name="obj">The object to unload.</param>
        void Unload(object obj);

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>The serializer.</value>
        AssetSerializer Serializer { get; }
    }
}