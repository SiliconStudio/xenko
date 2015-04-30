// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Base interface for a light with a color
    /// </summary>
    public interface IColorLight : ILight
    {
        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        /// <value>The color.</value>
        ILightColor Color { get; set; }

        /// <summary>
        /// Computes the color.
        /// </summary>
        /// <param name="intensity">The intensity.</param>
        /// <returns>Color3.</returns>
        Color3 ComputeColor(float intensity);
    }
}