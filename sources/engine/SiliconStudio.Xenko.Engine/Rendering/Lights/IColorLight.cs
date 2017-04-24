// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Colors;

namespace SiliconStudio.Xenko.Rendering.Lights
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
        IColorProvider Color { get; set; }

        /// <summary>
        /// Computes the color to the proper <see cref="ColorSpace"/> with the specified intensity.
        /// </summary>
        /// <param name="colorSpace"></param>
        /// <param name="intensity">The intensity.</param>
        /// <returns>Color3.</returns>
        Color3 ComputeColor(ColorSpace colorSpace, float intensity);
    }
}
