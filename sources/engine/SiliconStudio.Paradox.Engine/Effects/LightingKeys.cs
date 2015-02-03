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
        /// <userdoc>
        /// Maximum number of supported directional lights.
        /// </userdoc>
        public static readonly ParameterKey<int> MaxDirectionalLights = ParameterKeys.New(0);
        
        /// <summary>
        /// Maximum number of point lights.
        /// </summary>
        /// <userdoc>
        /// Maximum number of supported point lights.
        /// </userdoc>
        public static readonly ParameterKey<int> MaxPointLights = ParameterKeys.New(0);

        /// <summary>
        /// Maximum number of spot lights.
        /// </summary>
        /// <userdoc>
        /// Maximum number of supported spot lights.
        /// </userdoc>
        public static readonly ParameterKey<int> MaxSpotLights = ParameterKeys.New(0);

        /// <summary>
        /// A flag stating if directional light loop should be unrolled.
        /// </summary>
        /// <userdoc>
        /// When checked, the effect will support exactly the set number of directional lights.
        /// </userdoc>
        public static readonly ParameterKey<bool> UnrollDirectionalLightLoop = ParameterKeys.New(false);

        /// <summary>
        /// A flag stating if point light loop should be unrolled.
        /// </summary>
        /// <userdoc>
        /// When checked, the effect will support exactly the set number of point lights.
        /// </userdoc>
        public static readonly ParameterKey<bool> UnrollPointLightLoop = ParameterKeys.New(false);

        /// <summary>
        /// A flag stating if spot light loop should be unrolled.
        /// </summary>
        /// <userdoc>
        /// When checked, the effect will support exactly the set number of spot lights.
        /// </userdoc>
        public static readonly ParameterKey<bool> UnrollSpotLightLoop = ParameterKeys.New(false);

        /// <summary>
        /// Flag stating if the mesh casts shadows.
        /// </summary>
        /// <userdoc>
        /// When checked, the mesh will cast shadows.
        /// </userdoc>
        public static readonly ParameterKey<bool> CastShadows = ParameterKeys.New(true);

        /// <summary>
        /// Flag stating if the mesh receives shadows.
        /// </summary>
        /// <userdoc>
        /// When checked, the mesh will receive shadows.
        /// </userdoc>
        public static readonly ParameterKey<bool> ReceiveShadows = ParameterKeys.New(true);

        /// <summary>
        /// Supported lighting configurations.
        /// </summary>
        /// <userdoc>
        /// The supported lighting configurations.
        /// </userdoc>
        public static readonly ParameterKey<LightingConfigurationsSet> LightingConfigurations = ParameterKeys.New<LightingConfigurationsSet>();

        /// <summary>
        /// The maximum number of deferred point lights that can be rendered at the same time (in one draw call).
        /// </summary>
        public const int MaxDeferredPointLights = 32;

        /// <summary>
        /// Maximum number of deferred lights.
        /// </summary>
        /// <userdoc>
        /// Maximum number of deferred point lights.
        /// </userdoc>
        public static readonly ParameterKey<int> MaxDeferredLights = ParameterKeys.New(MaxDeferredPointLights);

        /// <summary>
        /// A flag stating if deferred light loop should be unrolled.
        /// </summary>
        /// <userdoc>
        /// When checked, the deferred effect will support exactly the set number of point lights.
        /// </userdoc>
        public static readonly ParameterKey<bool> UnrollDeferredLightLoop = ParameterKeys.New(false);
    }
}
