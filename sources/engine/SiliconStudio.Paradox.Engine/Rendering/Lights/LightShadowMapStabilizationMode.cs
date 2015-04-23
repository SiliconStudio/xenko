// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// The stabilization mode used for the shadow maps.
    /// </summary>
    [DataContract("LightShadowMapStabilizationMode")]
    public enum LightShadowMapStabilizationMode
    {
        /// <summary>
        /// No stabilization is performed.
        /// </summary>
        None,

        /// <summary>
        /// The light projection is snapped to the closest pixel according to the size of the shadow map. This will decrease filtering but lower the quality of the shadow map (more than <see cref="None"/>).
        /// </summary>
        [Display("Projection Snapping")]
        ProjectionSnapping,

        /// <summary>
        /// The light target view is snapped according to the size of the shadow map. Gives better results but decrease the quality of the shadow map (more than <see cref="ProjectionSnapping"/>).
        /// </summary>
        [Display("View Snapping")]
        ViewSnapping,
    }
}