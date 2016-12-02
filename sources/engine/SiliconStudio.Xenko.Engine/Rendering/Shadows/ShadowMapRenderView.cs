// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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

        /// <summary>
        /// Ignore depth planes in visibility test
        /// </summary>
        public bool VisiblityIgnoreDepthPlanes = true;

        internal ParameterCollection ViewParameters = new ParameterCollection();
    }
}
