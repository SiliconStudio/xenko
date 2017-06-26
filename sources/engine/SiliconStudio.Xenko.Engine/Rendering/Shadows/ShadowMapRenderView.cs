// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    /// <summary>
    /// A view used to render a shadow map to a <see cref="LightShadowMapTexture"/>
    /// </summary>
    public class ShadowMapRenderView : RenderView
    {
        /// <summary>
        /// The view for which this shadow map is rendered
        /// </summary>
        public RenderView RenderView;

        /// <summary>
        /// The shadow map to render
        /// </summary>
        public LightShadowMapTexture ShadowMapTexture;

        /// <summary>
        /// The rectangle to render to in the shadow map
        /// </summary>
        public Rectangle Rectangle;

        public ProfilingKey ProfilingKey { get; } = new ProfilingKey($"ShadowMapRenderView", ProfilingKeyFlags.GpuProfiling);
        
        /// <summary>
        /// Ignore depth planes in visibility test
        /// </summary>
        public bool VisiblityIgnoreDepthPlanes = true;

        internal ParameterCollection ViewParameters = new ParameterCollection();
    }
}
