// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.PostEffects
{
    /// <summary>
    /// Extensions for <see cref="PostEffectBase"/>.
    /// </summary>
    public static class PostEffectBaseExtensions
    {
        /// <summary>
        /// Sets an input texture
        /// </summary>
        /// <param name="postEffect">The post effect.</param>
        /// <param name="texture">The texture.</param>
        public static void SetInput(this PostEffectBase postEffect, Texture texture)
        {
            postEffect.SetInput(0, texture);
        }

        /// <summary>
        /// Sets two input textures
        /// </summary>
        /// <param name="postEffect">The post effect.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="texture2">The texture2.</param>
        public static void SetInput(this PostEffectBase postEffect, Texture texture, Texture texture2)
        {
            postEffect.SetInput(0, texture);
            postEffect.SetInput(1, texture2);
        }

        /// <summary>
        /// Sets two input textures
        /// </summary>
        /// <param name="postEffect">The post effect.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="texture2">The texture2.</param>
        /// <param name="texture3">The texture3.</param>
        public static void SetInput(this PostEffectBase postEffect, Texture texture, Texture texture2, Texture texture3)
        {
            postEffect.SetInput(0, texture);
            postEffect.SetInput(1, texture2);
            postEffect.SetInput(2, texture3);
        }         
    }
}