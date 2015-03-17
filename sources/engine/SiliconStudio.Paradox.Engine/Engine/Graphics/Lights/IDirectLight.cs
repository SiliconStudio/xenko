// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Importance of a shadow.
    /// </summary>
    public enum LightShadowImportance
    {
        /// <summary>
        /// A low importance means that the shadow has a low visual impact. 
        /// (e.g shadows from point lights)
        /// </summary>
        Low,

        /// <summary>
        /// A medium importance shadow means the shadow has a medium visual impact. 
        /// (e.g shadows from spot lights)
        /// </summary>
        Medium,

        /// <summary>
        /// A high importance means the shadow has a high visual impact.
        /// (e.g shadows from directional lights)
        /// </summary>
        High
    }

    /// <summary>
    /// Base interface for all direct lights.
    /// </summary>
    public interface IDirectLight : IColorLight
    {
        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        ILightShadow Shadow { get; set; }

        /// <summary>
        /// Gets the importance of the shadow. See remarks.
        /// </summary>
        /// <returns>System.Single.</returns>
        /// <remarks>The higher the importance is, the higher the cost of shadow computation is costly</remarks>
        LightShadowImportance ShadowImportance { get; set; }

        /// <summary>
        /// Computes the screen coverage of this light in pixel.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="position">The position of the light in world space.</param>
        /// <param name="direction">The direction of the light in world space.</param>
        /// <returns>The largest screen coverage width or height size in pixels of this light.</returns>
        float ComputeScreenCoverage(RenderContext context, Vector3 position, Vector3 direction);
    }
}