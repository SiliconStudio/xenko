// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Extensions for <see cref="ImageEffect"/>.
    /// </summary>
    public static class ImageEffectExtensions
    {
        /// <summary>
        /// Sets an input texture
        /// </summary>
        /// <param name="imageEffect">The post effect.</param>
        /// <param name="texture">The texture.</param>
        public static void SetInput(this IImageEffect imageEffect, Texture texture)
        {
            imageEffect.SetInput(0, texture);
        }

        /// <summary>
        /// Sets two input textures
        /// </summary>
        /// <param name="imageEffect">The post effect.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="texture2">The texture2.</param>
        public static void SetInput(this IImageEffect imageEffect, Texture texture, Texture texture2)
        {
            imageEffect.SetInput(0, texture);
            imageEffect.SetInput(1, texture2);
        }

        /// <summary>
        /// Sets two input textures
        /// </summary>
        /// <param name="imageEffect">The post effect.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="texture2">The texture2.</param>
        /// <param name="texture3">The texture3.</param>
        public static void SetInput(this IImageEffect imageEffect, Texture texture, Texture texture2, Texture texture3)
        {
            imageEffect.SetInput(0, texture);
            imageEffect.SetInput(1, texture2);
            imageEffect.SetInput(2, texture3);
        }         
    }
}
