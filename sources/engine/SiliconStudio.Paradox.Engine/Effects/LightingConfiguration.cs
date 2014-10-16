// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Lighting configuration for a mesh. Stores the number of lights per type and the number of shadow maps.
    /// </summary>
    [DataConverter(AutoGenerate = true)]
    public struct LightingConfiguration
    {
        [DataMemberConvert]
        public int MaxNumDirectionalLight;
        
        [DataMemberConvert]
        public int MaxNumPointLight;

        [DataMemberConvert]
        public int MaxNumSpotLight;

        [DataMemberConvert]
        public bool UnrollDirectionalLightLoop;

        [DataMemberConvert]
        public bool UnrollPointLightLoop;

        [DataMemberConvert]
        public bool UnrollSpotLightLoop;

        [DataMemberConvert]
        public ShadowConfigurationArray ShadowConfigurations;
    }
}
