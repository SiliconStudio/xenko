// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Extensions for the <see cref="GraphicsDevice"/>
    /// </summary>
    public static class GraphicsDeviceExtensions
    {
        /// <summary>
        /// Draws a fullscreen quad with the specified effect and parameters.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="effectInstance">The effect instance.</param>
        /// <exception cref="System.ArgumentNullException">effect</exception>
        public static void DrawQuad(this CommandList commandList, EffectInstance effectInstance)
        {
            if (effectInstance == null) throw new ArgumentNullException("effectInstance");

            // Apply the effect
            effectInstance.Apply(commandList);

            // Draw a full screen quad
            commandList.DrawQuad();
        }

        public static Texture GetSharedWhiteTexture(this GraphicsDevice device)
        {
            return device.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, "WhiteTexture", CreateWhiteTexture);
        }

        private static Texture CreateWhiteTexture(GraphicsDevice device)
        {
            const int Size = 2;
            var whiteData = new Color[Size * Size];
            for (int i = 0; i < Size*Size; i++)
                whiteData[i] = Color.White;

            return Texture.New2D(device, Size, Size, PixelFormat.R8G8B8A8_UNorm, whiteData);
        }
    }
}