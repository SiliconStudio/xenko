// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects
{
    [DataContract]
    public partial class ShadowMapParameters : ShaderMixinParameters
    {
        /// <summary>
        /// List of all the shadow map configurations.
        /// </summary>
        public static readonly ParameterKey<ShadowMapParameters[]> ShadowMaps = ParameterKeys.New<ShadowMapParameters[]>();

        /// <summary>
        /// Filter type of the shadow map.
        /// </summary>
        public static readonly ParameterKey<LightType> LightType = ParameterKeys.New<LightType>();

        /// <summary>
        /// Filter type of the shadow map.
        /// </summary>
        public static readonly ParameterKey<ShadowMapFilterType> FilterType = ParameterKeys.New<ShadowMapFilterType>();

        /// <summary>
        /// Number of shadow maps.
        /// </summary>
        public static readonly ParameterKey<int> ShadowMapCount = ParameterKeys.New<int>(0);

        /// <summary>
        /// number of shadow map cascades.
        /// </summary>
        public static readonly ParameterKey<int> ShadowMapCascadeCount = ParameterKeys.New<int>(4);

        /// <summary>
        /// Name of the atlas.
        /// </summary>
        public static readonly ParameterKey<string> AtlasKey = ParameterKeys.New<string>();

        /// <summary>
        /// The key to use to create shadow groups.
        /// </summary>
        /// <userdoc>
        /// The supported shadow configurations.
        /// </userdoc>
        public static readonly ParameterKey<ShadowConfigurationArray> ShadowConfigurations = ParameterKeys.New<ShadowConfigurationArray>(null);
    }
}
