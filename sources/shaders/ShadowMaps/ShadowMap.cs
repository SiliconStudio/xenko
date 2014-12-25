// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects.ShadowMaps
{
    /// <summary>
    /// Represents a shadow map for the <see cref="ShadowMapRenderer"/>.
    /// </summary>
    public class ShadowMap
    {
        public ShadowMap()
        {
            ShadowNearDistance = 1.0f;
            ShadowFarDistance = 50000.0f;
            ShadowMapSize = 512;
            CascadeCount = 4;
            Layers = RenderLayers.RenderLayerAll;
            Update = true;
        }

        /// <summary>
        /// A flag stating if the shadow map should be updated.
        /// </summary>
        public bool Update;

        /// <summary>
        /// The light direction.
        /// </summary>
        public Vector3 LightDirection;

        /// <summary>
        /// The light position.
        /// </summary>
        public Vector3 LightPosition;

        /// <summary>
        /// The fov of the light in radians.
        /// </summary>
        public float Fov;

        /// <value>
        /// The size of the shadow map.
        /// </value>
        public int ShadowMapSize;

        /// <summary>
        /// The shadow map near clipping distance.
        /// </summary>
        public float ShadowNearDistance;

        /// <summary>
        /// The shadow map far clipping distance.
        /// </summary>
        public float ShadowFarDistance;

        /// <summary>
        /// The number of shadow map cascades.
        /// </summary>
        public int CascadeCount;

        /// <summary>
        /// The light type.
        /// </summary>
        public LightType LightType;

        /// <summary>
        /// The shadow map filter.
        /// </summary>
        public ShadowMapFilterType Filter;

        /// <summary>
        /// The shadow map texture.
        /// </summary>
        public ShadowMapTexture Texture;

        /// <summary>
        /// The active layers for this shadow map.
        /// </summary>
        public RenderLayers Layers;

        internal ShadowMapCascadeInfo[] Cascades;
        internal ShadowMapReceiverInfo ReceiverInfo;
        internal ShadowMapReceiverVsmInfo ReceiverVsmInfo;
        internal Vector3 LightDirectionNormalized;
    }
}