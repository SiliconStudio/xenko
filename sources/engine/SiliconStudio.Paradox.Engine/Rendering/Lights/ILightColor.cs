// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Defines the interface for describing the color of a light.
    /// </summary>
    public interface ILightColor
    {
        /// <summary>
        /// Computes the color of the light (sRgb space).
        /// </summary>
        /// <returns>Color3.</returns>
        Color3 ComputeColor();
    }
}