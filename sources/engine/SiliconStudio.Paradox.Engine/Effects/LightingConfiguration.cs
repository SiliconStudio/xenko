// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Lighting configuration for a mesh. Stores the number of lights per type and the number of shadow maps.
    /// </summary>
    [DataContract]
    public partial struct LightingConfiguration
    {
        public int MaxNumDirectionalLight;
        
        public int MaxNumPointLight;

        public int MaxNumSpotLight;

        public bool UnrollDirectionalLightLoop;

        public bool UnrollPointLightLoop;

        public bool UnrollSpotLightLoop;

        public ShadowConfigurationArray ShadowConfigurations;
    }
}
