// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Lights
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
        /// <userdoc>No stabilization is performed.</userdoc>
        None,

        /// <summary>
        /// The light projection is snapped to the closest pixel according to the size of the shadow map. This will decrease filtering but lower the quality of the shadow map (more than <see cref="None"/>).
        /// </summary>
        /// <userdoc>The light projection is snapped to the closest pixel according to the size of the shadow map. This will decrease filtering but lower the quality of the shadow map (more than <see cref="None"/>)</userdoc>
        [Display("Projection Snapping")]
        ProjectionSnapping,

        /// <summary>
        /// The light target view is snapped according to the size of the shadow map. Gives better results but decrease the quality of the shadow map (more than <see cref="ProjectionSnapping"/>).
        /// </summary>
        /// <userdoc>The light target view is snapped according to the size of the shadow map. Gives better results but decrease the quality of the shadow map (more than <see cref="ProjectionSnapping"/>).</userdoc>
        [Display("View Snapping")]
        ViewSnapping,
    }
}
