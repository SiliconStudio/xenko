// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Extension class for <see cref="Color"/>
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Create a copy of the color with the provided alpha value.
        /// </summary>
        /// <param name="color">The color to take as reference</param>
        /// <param name="alpha">The alpha value of the new color</param>
        /// <returns>The color with the provided alpha value</returns>
        public static Color WithAlpha(this Color color, byte alpha)
        {
            return new Color(color.R, color.G, color.B, alpha);
        }
    }
}