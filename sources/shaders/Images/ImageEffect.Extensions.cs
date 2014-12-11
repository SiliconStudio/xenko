// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
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
        public static void SetInput(this ImageEffect imageEffect, Texture texture)
        {
            imageEffect.SetInput(0, texture);
        }

        /// <summary>
        /// Sets two input textures
        /// </summary>
        /// <param name="imageEffect">The post effect.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="texture2">The texture2.</param>
        public static void SetInput(this ImageEffect imageEffect, Texture texture, Texture texture2)
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
        public static void SetInput(this ImageEffect imageEffect, Texture texture, Texture texture2, Texture texture3)
        {
            imageEffect.SetInput(0, texture);
            imageEffect.SetInput(1, texture2);
            imageEffect.SetInput(2, texture3);
        }         
    }
}