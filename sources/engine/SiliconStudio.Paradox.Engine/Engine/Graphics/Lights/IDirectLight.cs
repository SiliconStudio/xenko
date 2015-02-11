// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Base interface for all direct lights.
    /// </summary>
    public interface IDirectLight : ILight
    {
        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        /// <value>The color.</value>
        ILightColor Color { get; set; }

        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        ILightShadow Shadow { get; set; }

        Color3 ComputeColor(float intensity);
    }
}