// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Keys used for lighting.
    /// </summary>
    [DataContract]
    public partial class LightingKeys : ShaderMixinParameters
    {
        /// <summary>
        /// Maximum number of directional lights.
        /// </summary>
        public static readonly ParameterKey<int> MaxDirectionalLights = ParameterKeys.New(0);
        
        /// <summary>
        /// Maximum number of point lights.
        /// </summary>
        public static readonly ParameterKey<int> MaxPointLights = ParameterKeys.New(0);

        /// <summary>
        /// Maximum number of spot lights.
        /// </summary>
        public static readonly ParameterKey<int> MaxSpotLights = ParameterKeys.New(0);

        /// <summary>
        /// A flag stating if directional light loop should be unrolled.
        /// </summary>
        public static readonly ParameterKey<bool> UnrollDirectionalLightLoop = ParameterKeys.New(false);

        /// <summary>
        /// A flag stating if point light loop should be unrolled.
        /// </summary>
        public static readonly ParameterKey<bool> UnrollPointLightLoop = ParameterKeys.New(false);

        /// <summary>
        /// A flag stating if spot light loop should be unrolled.
        /// </summary>
        public static readonly ParameterKey<bool> UnrollSpotLightLoop = ParameterKeys.New(false);

        /// <summary>
        /// Flag stating if the mesh casts shadows.
        /// </summary>
        public static readonly ParameterKey<bool> CastShadows = ParameterKeys.New(false);

        /// <summary>
        /// Flag stating if the mesh receives shadows.
        /// </summary>
        public static readonly ParameterKey<bool> ReceiveShadows = ParameterKeys.New(false);

        /// <summary>
        /// Maximum number of supported nearest filtered shadows.
        /// </summary>
        public static readonly ParameterKey<int> MaxDirectionalNearestFilterShadowMap = ParameterKeys.New(0);

        /// <summary>
        /// Maximum number of supported Pcf shadows.
        /// </summary>
        public static readonly ParameterKey<int> MaxDirectionalPcfFilterShadowMap = ParameterKeys.New(0);
        
        /// <summary>
        /// Maximum number of deferred lights.
        /// </summary>
        public static readonly ParameterKey<int> MaxDeferredLights = ParameterKeys.New(64);

        /// <summary>
        /// A flag stating if deferred light loop should be unrolled.
        /// </summary>
        public static readonly ParameterKey<bool> UnrollDeferredLightLoop = ParameterKeys.New(false);
    }
}
