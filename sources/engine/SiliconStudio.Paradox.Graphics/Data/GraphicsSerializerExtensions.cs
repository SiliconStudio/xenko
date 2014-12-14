// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Graphics.Data
{
    /// <summary>
    /// Various extension method for serialization of GPU types having separate CPU serialized data format.
    /// </summary>
    public static class GraphicsSerializerExtensions
    {
        /// <summary>
        /// Creates a fake <see cref="Buffer" /> that will have the given serialized data version.
        /// </summary>
        /// <param name="bufferData">The buffer data.</param>
        /// <returns></returns>
        public static Buffer ToSerializableVersion(this BufferData bufferData)
        {
            var buffer = new Buffer();
            buffer.SetSerializationData(bufferData);

            return buffer;
        }

        /// <summary>
        /// Gets the serialized data version of this <see cref="Buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static BufferData GetSerializationData(this Buffer buffer)
        {
            var urlInfo = UrlServices.GetUrlInfo(buffer);
            return urlInfo != null ? (BufferData)urlInfo.Data : null;
        }

        /// <summary>
        /// Sets the serialized data version of this <see cref="Buffer" />.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="bufferData">The buffer data.</param>
        public static void SetSerializationData(this Buffer buffer, BufferData bufferData)
        {
            var urlInfo = UrlServices.GetOrCreateUrlInfo(buffer);
            urlInfo.Data = bufferData;
        }

        /// <summary>
        /// Creates a fake <see cref="Texture"/> that will have the given serialized data version.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static Texture ToSerializableVersion(this Image image)
        {
            var texture = new Texture();
            texture.SetSerializationData(image);

            return texture;
        }

        /// <summary>
        /// Gets the serialized data version of this <see cref="Texture"/>.
        /// </summary>
        /// <param name="Texture">The texture.</param>
        /// <returns></returns>
        public static Image GetSerializationData(this Texture texture)
        {
            var urlInfo = UrlServices.GetUrlInfo(texture);
            return urlInfo != null ? (Image)urlInfo.Data : null;
        }

        /// <summary>
        /// Sets the serialized data version of this <see cref="Texture" />.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="image">The image.</param>
        public static void SetSerializationData(this Texture texture, Image image)
        {
            var urlInfo = UrlServices.GetOrCreateUrlInfo(texture);
            urlInfo.Data = image;
        }
    }
}