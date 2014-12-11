// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Extension methods for <see cref="Texture"/>
    /// </summary>
    internal static class TextureExtensions
    {
        /// <summary>
        /// Creates a 2D texture from an image file data (png, dds, ...).
        /// </summary>
        /// <param name="graphicsDevice">The graphics device in which to create the texture</param>
        /// <param name="data">The image file data</param>
        /// <returns>A 2D texture</returns>
        public static Texture CreateTextureFromFileData(GraphicsDevice graphicsDevice, byte[] data)
        {
            Texture result;

            {
                using (var imageStream = new MemoryStream(data))
                {
                    using (var image = Image.Load(imageStream))
                        result = Texture.New(graphicsDevice, image);
                }
            }

            result.Reload = graphicsResource =>
            {
                using (var imageStream = new MemoryStream(data))
                {
                    using (var image = Image.Load(imageStream))
                        ((Texture)graphicsResource).Recreate(image.ToDataBox());
                }
            };

            return result;
        }
    }
}