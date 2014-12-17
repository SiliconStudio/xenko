// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Data
{
    public partial class LightingConfigurationData
    {
        public int TotalLightCount 
        {
            get
            {
                var shadowLightCount = 0;
                if (ShadowConfigurations != null && ShadowConfigurations.Groups.Count > 0)
                {
                    foreach (var group in ShadowConfigurations.Groups)
                        shadowLightCount += group.ShadowCount;
                }
                return MaxNumDirectionalLight + MaxNumPointLight + MaxNumSpotLight + shadowLightCount;
            }
        }

        public LightingConfigurationData()
        {
            MaxNumDirectionalLight = 0;
            MaxNumPointLight = 0;
            MaxNumSpotLight = 0;

            UnrollDirectionalLightLoop = false;
            UnrollPointLightLoop = false;
            UnrollSpotLightLoop = false;
        }

        /// <summary>
        /// Creates a new LightingConfigurationData from a collection of parameters.
        /// </summary>
        /// <param name="collection">The parameter collection.</param>
        public LightingConfigurationData(ParameterCollection collection)
        {
            MaxNumDirectionalLight = collection.Get(LightingKeys.MaxDirectionalLights);
            MaxNumPointLight = collection.Get(LightingKeys.MaxPointLights);
            MaxNumSpotLight = collection.Get(LightingKeys.MaxSpotLights);

            UnrollDirectionalLightLoop = false;
            UnrollPointLightLoop = false;
            UnrollSpotLightLoop = false;

            var configs = collection.Get(ShadowMapParameters.ShadowConfigurations);
            if (configs != null)
            {
                ShadowConfigurations = new ShadowConfigurationArrayData();
                if (configs.Groups.Count > 0)
                {
                    ShadowConfigurations.Groups = new List<ShadowConfigurationData>();
                    for (var i = 0; i < configs.Groups.Count; ++i)
                    {
                        if (configs.Groups[i].ShadowCount > 0)
                        {
                            var configData = new ShadowConfigurationData();
                            configData.LightType = configs.Groups[i].LightType;
                            configData.ShadowCount = configs.Groups[i].ShadowCount;
                            configData.CascadeCount = configData.LightType == LightType.Directional ? configs.Groups[i].CascadeCount : 1;
                            configData.FilterType = configs.Groups[i].FilterType;
                            ShadowConfigurations.Groups.Add(configData);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the collection of parameters from the data.
        /// </summary>
        /// <returns>The parameter collection.</returns>
        public ParameterCollectionData GetCollection()
        {
            var parameters = new ParameterCollectionData();
            parameters.Set(LightingKeys.MaxDirectionalLights, MaxNumDirectionalLight);
            parameters.Set(LightingKeys.MaxPointLights, MaxNumPointLight);
            parameters.Set(LightingKeys.MaxSpotLights, MaxNumSpotLight);
            parameters.Set(LightingKeys.UnrollDirectionalLightLoop, UnrollDirectionalLightLoop);
            parameters.Set(LightingKeys.UnrollPointLightLoop, UnrollPointLightLoop);
            parameters.Set(LightingKeys.UnrollSpotLightLoop, UnrollSpotLightLoop);

            if (ShadowConfigurations != null && ShadowConfigurations.Groups != null && ShadowConfigurations.Groups.Count > 0)
            {
                var shadow = new List<ShadowMapParameters>();
                foreach (var shadowConfig in ShadowConfigurations.Groups)
                {
                    var shadowParams = new ShadowMapParameters();
                    shadowParams.Set(ShadowMapParameters.LightType, shadowConfig.LightType);
                    shadowParams.Set(ShadowMapParameters.ShadowMapCount, shadowConfig.ShadowCount);
                    shadowParams.Set(ShadowMapParameters.ShadowMapCascadeCount, shadowConfig.LightType == LightType.Directional ? shadowConfig.CascadeCount : 1);
                    shadowParams.Set(ShadowMapParameters.FilterType, shadowConfig.FilterType);
                    shadow.Add(shadowParams);
                }
                parameters.Set(ShadowMapParameters.ShadowMaps, shadow.ToArray());
            }
            return parameters;
        }

        public static void CheckUnrolls(LightingConfigurationData[] sortedConfigs)
        {
            // find unroll optimizations
            for (var i = 0; i < sortedConfigs.Length; ++i)
            {
                var currentConfig = sortedConfigs[i];
                var lightCount = currentConfig.MaxNumDirectionalLight + currentConfig.MaxNumPointLight + currentConfig.MaxNumSpotLight;
                for (var j = 0; j < sortedConfigs.Length; ++j)
                {
                    var localConfig = sortedConfigs[j];
                    var localCount = localConfig.MaxNumDirectionalLight + localConfig.MaxNumPointLight + localConfig.MaxNumSpotLight;
                    if (localCount == lightCount)
                        break;
                    if (localCount + 1 == lightCount)
                    {
                        if (localConfig.MaxNumDirectionalLight + 1 == currentConfig.MaxNumDirectionalLight)
                            currentConfig.UnrollDirectionalLightLoop = true;
                        if (localConfig.MaxNumPointLight + 1 == currentConfig.MaxNumPointLight)
                            currentConfig.UnrollPointLightLoop = true;
                        if (localConfig.MaxNumSpotLight + 1 == currentConfig.MaxNumSpotLight)
                            currentConfig.UnrollSpotLightLoop = true;
                    }
                }
            }
        }
    }
}
