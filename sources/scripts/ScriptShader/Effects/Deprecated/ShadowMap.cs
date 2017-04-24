// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Represents a ShadowMap.
    /// </summary>
    public class ShadowMap
    {
        internal ParameterCollection Parameters { get; set; }

        public ShadowMap(DirectionalLight light)
        {
            Light = light;

            Parameters = new ParameterCollection("ShadowMap parameters");

            // Inherits light parameters (to have its color and direction or viewproj matrix).
            Parameters.AddSources(light.Parameters);

            CasterParameters = new ParameterCollection("ShadowMap Caster Parameters");
            CasterParameters.AddSources(Parameters);
            ShadowDistance = 1000.0f;
            ShadowMapSize = 512;
        }

        /// <summary>
        /// Gets or sets the size of the shadow map (default: 1024)
        /// </summary>
        /// <value>
        /// The size of the shadow map.
        /// </value>
        public int ShadowMapSize { get; set; }

        public Texture Texture
        {
            get { return Parameters.TryGet(ShadowMapKeys.Texture); }
            set { Parameters.Set(ShadowMapKeys.Texture, value); }
        }

        /// <summary>
        /// Gets or sets the shadow distance.
        /// </summary>
        /// <value>
        /// The shadow distance.
        /// </value>
        /// <remarks>
        /// Maximum distance in camera space to render the shadow
        /// </remarks>
        public float ShadowDistance
        {
            get { return Parameters.TryGet(ShadowMapKeys.DistanceMax); }
            set { Parameters.Set(ShadowMapKeys.DistanceMax, value); }
        }

        /// <summary>
        /// Gets the associated light.
        /// </summary>
        public Light Light { get; protected set; }

        /// <summary>
        /// Gets the associated light, as a DirectionalLight.
        /// </summary>
        public DirectionalLight DirectionalLight { get { return (DirectionalLight)Light; } }
        
        /// <summary>
        /// Gets or sets the level of shadow maps (default: <see cref="CascadeShadowMapLevel.X4"/>)
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public CascadeShadowMapLevel Level { get; set; }

        public Vector4[] TextureCoordsBorder { get; set; }

        public ShadowMapFilter Filter { get; internal set; }

        /// <summary>
        /// Gets the level count.
        /// </summary>
        internal int LevelCount
        {
            get
            {
                return (int)Level;
            }
        }

        internal RenderPass[] Passes { get; set; }

        internal RenderTargetsPlugin[] Plugins { get; set; }

        internal ParameterCollection CasterParameters { get; set; }

        public ShadowMap SetFilter(Func<ShadowMap, ShadowMapFilter> initializeFilter)
        {
            Filter = initializeFilter(this);
            return this;
        }
    }
}
