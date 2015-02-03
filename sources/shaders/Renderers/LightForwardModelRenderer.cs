// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Effects.ShadowMaps;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Renderers
{
    /// <summary>
    /// TODO: Evaluate if it would be possible to split this class with support for different lights instead of a big fat class
    /// TODO: Refactor this class
    /// </summary>
    internal class LightForwardModelRenderer
    {
        internal static Dictionary<ParameterKey, LightParamSemantic> LightParametersDict = new Dictionary<ParameterKey, LightParamSemantic>
        {
            { ShadingEyeNormalVSKeys.LightDirectionsVS, LightParamSemantic.DirectionVS },
            { ShadingEyeNormalVSKeys.LightPositionsVS,  LightParamSemantic.PositionVS },
            { ShadingEyeNormalWSKeys.LightDirectionsWS, LightParamSemantic.DirectionWS },
            { ShadingEyeNormalWSKeys.LightPositionsWS,  LightParamSemantic.PositionWS },
            { LightParametersKeys.LightColorsWithGamma, LightParamSemantic.ColorWithGamma },
            { LightParametersKeys.LightIntensities,     LightParamSemantic.Intensity },
            { LightParametersKeys.LightDecayStarts,     LightParamSemantic.Decay },
            { LightParametersKeys.LightSpotBeamAngle,   LightParamSemantic.SpotBeamAngle },
            { LightParametersKeys.LightSpotFieldAngle,  LightParamSemantic.SpotFieldAngle },
            { LightParametersKeys.LightCount,           LightParamSemantic.Count }
        };

        #region Private members

        private int maximumSupportedLights;

        private readonly Dictionary<Entity, Vector3> lightDirectionViewSpace;

        private readonly Dictionary<Entity, Vector3> lightDirectionWorldSpace;

        private readonly Dictionary<Entity, Color3> lightGammaColor;

        private float[] arrayFloat;

        private Vector3[] arrayVector3;

        private Color3[] arrayColor3;

        private readonly ShadowMapReceiverInfo[] receiverInfos;

        private readonly ShadowMapReceiverVsmInfo[] receiverVsmInfos;

        private readonly ShadowMapCascadeReceiverInfo[] cascadeInfos;

        private readonly List<EntityLightShadow> validLights;

        private readonly List<EntityLightShadow> directionalLights;

        private readonly List<EntityLightShadow> directionalLightsWithShadows;

        private readonly List<EntityLightShadow> pointLights;

        private readonly List<EntityLightShadow> spotLights;

        private readonly List<EntityLightShadow> spotLightsWithShadows;

        private readonly List<EntityLightShadow> directionalLightsForMesh;
        
        private readonly List<EntityLightShadow> directionalLightsWithShadowForMesh;

        private readonly List<EntityLightShadow> pointLightsForMesh;

        private readonly List<EntityLightShadow> spotLightsForMesh;

        private readonly List<EntityLightShadow> spotLightsWithShadowForMesh;

        private readonly Dictionary<ParameterKey, LightParamSemantic> lightingParameterSemantics;

        private readonly List<List<EntityLightShadow>> directionalLightsWithShadowForMeshGroups;

        private readonly List<List<EntityLightShadow>> spotLightsWithShadowForMeshGroups;

        private readonly List<List<ShadowMap>> shadowMapGroups;

        private LightingConfiguration lastConfiguration;

        #endregion

        #region Constructor

        public LightForwardModelRenderer(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException("services");
            Services = services;
            maximumSupportedLights = 128;

            lightDirectionViewSpace = new Dictionary<Entity, Vector3>();
            lightDirectionWorldSpace = new Dictionary<Entity, Vector3>();
            lightGammaColor = new Dictionary<Entity, Color3>();

            arrayFloat = new float[4 * maximumSupportedLights];
            arrayVector3 = new Vector3[2 * maximumSupportedLights];
            arrayColor3 = new Color3[maximumSupportedLights];
            //TODO: resize
            receiverInfos = new ShadowMapReceiverInfo[16];
            receiverVsmInfos = new ShadowMapReceiverVsmInfo[16];
            cascadeInfos = new ShadowMapCascadeReceiverInfo[128];

            validLights = new List<EntityLightShadow>();
            directionalLights = new List<EntityLightShadow>();
            directionalLightsWithShadows = new List<EntityLightShadow>();
            pointLights = new List<EntityLightShadow>();
            spotLights = new List<EntityLightShadow>();
            spotLightsWithShadows = new List<EntityLightShadow>();

            directionalLightsForMesh = new List<EntityLightShadow>();
            directionalLightsWithShadowForMesh = new List<EntityLightShadow>();
            directionalLightsWithShadowForMeshGroups = new List<List<EntityLightShadow>>();
            spotLightsWithShadowForMeshGroups = new List<List<EntityLightShadow>>();
            pointLightsForMesh = new List<EntityLightShadow>();
            spotLightsForMesh = new List<EntityLightShadow>();
            spotLightsWithShadowForMesh = new List<EntityLightShadow>();
            
            shadowMapGroups = new List<List<ShadowMap>>();

            lightingParameterSemantics = new Dictionary<ParameterKey, LightParamSemantic>();
        }

        #endregion

        public IServiceRegistry Services { get; private set; }

        #region Protected methods


        /// <summary>
        /// Filter out the inactive lights.
        /// </summary>
        /// <param name="context">The render context.</param>
        public void PreRender(RenderContext context)
        {
            // get the lightprocessor
            var entitySystem = Services.GetServiceAs<EntitySystem>();
            var lightProcessor = entitySystem.GetProcessor<LightShadowProcessor>();
            if (lightProcessor == null)
                return;

            foreach (var light in lightProcessor.Lights)
            {
                if (!light.Value.Light.Deferred && light.Value.Light.Enabled)
                {
                    validLights.Add(light.Value);

                    switch (light.Value.Light.Type)
                    {
                        case LightType.Point:
                            pointLights.Add(light.Value);
                            break;
                        case LightType.Spherical:
                            break;
                        case LightType.Directional:
                            if (light.Value.HasShadowMap && lightProcessor.ActiveShadowMaps.Contains(light.Value.ShadowMap))
                                directionalLightsWithShadows.Add(light.Value);
                            else
                                directionalLights.Add(light.Value);
                            break;
                        case LightType.Spot:
                            if (light.Value.HasShadowMap && lightProcessor.ActiveShadowMaps.Contains(light.Value.ShadowMap))
                                spotLightsWithShadows.Add(light.Value);
                            else
                                spotLights.Add(light.Value);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Clear the light lists.
        /// </summary>
        /// <param name="context">The render context.</param>
        public void PostRender(RenderContext context)
        {
            lightDirectionViewSpace.Clear();
            lightDirectionWorldSpace.Clear();
            lightGammaColor.Clear();
            validLights.Clear();
            directionalLights.Clear();
            directionalLightsWithShadows.Clear();
            pointLights.Clear();
            spotLights.Clear();
            spotLightsWithShadows.Clear();
            directionalLightsForMesh.Clear();
            directionalLightsWithShadowForMesh.Clear();
            foreach (var group in directionalLightsWithShadowForMeshGroups)
                group.Clear();
            foreach (var group in spotLightsWithShadowForMeshGroups)
                group.Clear();
            foreach (var group in shadowMapGroups)
                group.Clear();
            pointLightsForMesh.Clear();
            spotLightsForMesh.Clear();
            spotLightsWithShadowForMesh.Clear();
        }

        /// <summary>
        /// Update light lists and choose the new light configuration.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="renderMesh">The current RenderMesh (the same as <seealso cref="PostEffectUpdate"/>)</param>
        public void PreEffectUpdate(RenderContext context, RenderMesh renderMesh)
        {
            // TODO:
            // light selection based on:
            //    - from the same entity?
            //    - spot & point lights distances
            // rewrite shaders to handle all the cases?
            // TODO: other criterion to choose the light (distance?)

            directionalLightsForMesh.Clear();
            directionalLightsWithShadowForMesh.Clear();
            foreach (var group in directionalLightsWithShadowForMeshGroups)
                group.Clear();
            foreach (var group in spotLightsWithShadowForMeshGroups)
                group.Clear();
            foreach (var group in shadowMapGroups)
                group.Clear();
            pointLightsForMesh.Clear();
            spotLightsForMesh.Clear();
            spotLightsWithShadowForMesh.Clear();

            var receiveShadows = renderMesh.Parameters.Get(LightingKeys.ReceiveShadows);
            var renderLayers = renderMesh.Parameters.Get(RenderingParameters.RenderLayer);

            foreach (var light in directionalLights)
            {
                if ((light.Light.Layers & renderLayers) != 0)
                    directionalLightsForMesh.Add(light);
            }
            foreach (var light in directionalLightsWithShadows)
            {
                if ((light.Light.Layers & renderLayers) != 0)
                {
                    if (receiveShadows)
                        directionalLightsWithShadowForMesh.Add(light);
                    else
                        directionalLightsForMesh.Add(light);
                }
            }
            foreach (var light in pointLights)
            {
                if ((light.Light.Layers & renderLayers) != 0)
                    pointLightsForMesh.Add(light);
            }
            foreach (var light in spotLights)
            {
                if ((light.Light.Layers & renderLayers) != 0)
                    spotLightsForMesh.Add(light);
            }
            foreach (var light in spotLightsWithShadows)
            {
                if ((light.Light.Layers & renderLayers) != 0)
                {
                    if (receiveShadows)
                        spotLightsWithShadowForMesh.Add(light);
                    else
                        spotLightsForMesh.Add(light);
                }
            }

            var numDirectionalLights = directionalLightsForMesh.Count;
            var numPointLights = pointLightsForMesh.Count;
            var numSpotLights = spotLightsForMesh.Count;

            // TODO: improve detection - better heuristics
            // choose configuration
            var configurations = renderMesh.Parameters.Get(LightingKeys.LightingConfigurations);
            var lastConfigWithoutShadow = -1;
            LightingConfiguration foundConfiguration;
            if (configurations != null)
            {
                foundConfiguration.MaxNumDirectionalLight = 0;
                foundConfiguration.MaxNumPointLight = 0;
                foundConfiguration.MaxNumSpotLight = 0;
                foundConfiguration.UnrollDirectionalLightLoop = false;
                foundConfiguration.UnrollPointLightLoop = false;
                foundConfiguration.UnrollSpotLightLoop = false;
                var configurationIndex = -1;
                for (var i = 0; i < configurations.Configs.Length; ++i)
                {
                    if (configurations.Configs[i].ShadowConfigurations == null || configurations.Configs[i].ShadowConfigurations.Groups.Count == 0)
                        lastConfigWithoutShadow = i;

                    if (TestConfiguration(numDirectionalLights, numPointLights, numSpotLights, configurations.Configs[i]))
                    {
                        configurationIndex = i;
                        break;
                    }
                }

                // no correct configuration found
                if (configurationIndex < 0)
                {
                    if (lastConfigWithoutShadow != -1)// take the biggest one without shadow
                        configurationIndex = lastConfigWithoutShadow;
                    else // take the latest
                        configurationIndex = configurations.Configs.Length - 1;
                }

                foundConfiguration = configurations.Configs[configurationIndex];

                //create the parameters to get the correct shader
                if (configurationIndex != renderMesh.Parameters.Get(LightKeys.ConfigurationIndex))
                {
                    CreateParametersFromLightingConfiguration(foundConfiguration, renderMesh.Parameters);
                    renderMesh.Parameters.Set(LightKeys.ConfigurationIndex, configurationIndex);
                }
            }
            else
            {
                // set the configuration that perfectly matches the actual scene
                foundConfiguration = new LightingConfiguration();
                foundConfiguration.MaxNumPointLight = numPointLights;
                foundConfiguration.MaxNumDirectionalLight = numDirectionalLights;
                foundConfiguration.MaxNumSpotLight = numSpotLights;
                foundConfiguration.UnrollPointLightLoop = true;
                foundConfiguration.UnrollDirectionalLightLoop = true;
                foundConfiguration.UnrollSpotLightLoop = true;

                if (directionalLightsWithShadowForMesh.Count > 0
                    || spotLightsWithShadowForMesh.Count > 0)
                {
                    foundConfiguration.ShadowConfigurations = new ShadowConfigurationArray();
                    foundConfiguration.ShadowConfigurations.Groups = new List<ShadowConfiguration>();
                    foundConfiguration.ShadowConfigurations.Groups.AddRange(CreateShadowConfiguration(directionalLightsWithShadowForMesh, LightType.Directional));
                    foundConfiguration.ShadowConfigurations.Groups.AddRange(CreateShadowConfiguration(spotLightsWithShadowForMesh, LightType.Spot));
                }

                CreateParametersFromLightingConfiguration(foundConfiguration, renderMesh.Parameters);
            }
            
            var maxNumDirectionalLights = foundConfiguration.MaxNumDirectionalLight;
            var maxNumPointLights = foundConfiguration.MaxNumPointLight;
            var maxNumSpotLights = foundConfiguration.MaxNumSpotLight;

            // assign the shadow ligths to a specific group
            if (foundConfiguration.ShadowConfigurations != null)
                AssignGroups(foundConfiguration);

            var finalDirectionalLightCount = Math.Min(numDirectionalLights, maxNumDirectionalLights);
            var finalPointLightCount = Math.Min(numPointLights, maxNumPointLights);
            var finalSpotLightCount = Math.Min(numSpotLights, maxNumSpotLights);

            var maxLights = finalDirectionalLightCount;
            if (maxLights > finalPointLightCount)
                maxLights = finalPointLightCount;
            if (maxLights > finalSpotLightCount)
                maxLights = finalSpotLightCount;

            if (maxLights > maximumSupportedLights)
            {
                maximumSupportedLights = maxLights;
                arrayFloat = new float[4 * maxLights];
                arrayVector3 = new Vector3[2 * maxLights];
                arrayColor3 = new Color3[maxLights];
            }

            lastConfiguration = foundConfiguration;
        }

        /// <summary>
        /// Update the light values of the shader.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="renderMesh">The current RenderMesh (the same as <seealso cref="PreEffectUpdate"/>)</param>
        public void PostEffectUpdate(RenderContext context, RenderMesh renderMesh)
        {
            var lightingGroupInfo = LightingGroupInfo.GetOrCreate(renderMesh.Effect);

            // update the info if necessary
            if (!lightingGroupInfo.IsLightingSetup)
            {
                lightingGroupInfo.LightingParameters = CreateLightingUpdateInfo(renderMesh);
                if (lastConfiguration.ShadowConfigurations != null)
                    lightingGroupInfo.ShadowParameters = CreateEffectShadowParams(lastConfiguration);

                lightingGroupInfo.IsLightingSetup = true;
            }

            if (lightingGroupInfo.LightingParameters.Count == 0)
                return;

            // TODO: is it always available?
            var viewMatrix = context.CurrentPass.Parameters.Get(TransformationKeys.View);

            if (lightingGroupInfo.ShadowParameters != null)
            {
                for (var i = 0; i < lightingGroupInfo.ShadowParameters.Count; ++i)
                    UpdateShadowParameters(renderMesh.Parameters, lightingGroupInfo.ShadowParameters[i], shadowMapGroups[i]);
            }

            // Apply parameters
            foreach (var info in lightingGroupInfo.LightingParameters)
            {
                switch (info.Type)
                {
                    case LightingUpdateType.Point:
                        UpdateLightingParameters(info, ref renderMesh, ref viewMatrix, pointLightsForMesh);
                        break;
                    case LightingUpdateType.Directional:
                        UpdateLightingParameters(info, ref renderMesh, ref viewMatrix, directionalLightsForMesh);
                        break;
                    case LightingUpdateType.Spot:
                        UpdateLightingParameters(info, ref renderMesh, ref viewMatrix, spotLightsForMesh);
                        break;
                    case LightingUpdateType.DirectionalShadow:
                        UpdateLightingParameters(info, ref renderMesh, ref viewMatrix, directionalLightsWithShadowForMeshGroups[info.Index]);
                        break;
                    case LightingUpdateType.SpotShadow:
                        UpdateLightingParameters(info, ref renderMesh, ref viewMatrix, spotLightsWithShadowForMeshGroups[info.Index]);
                        break;
                    //TODO: implement later when shadow map are supported
                    case LightingUpdateType.PointShadow:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion

        #region Private methods

        private bool TestConfiguration(int numDirectionalLights, int numPointLights, int numSpotLights, LightingConfiguration config)
        {
            if (config.MaxNumDirectionalLight < numDirectionalLights || config.MaxNumPointLight < numPointLights || config.MaxNumSpotLight < numSpotLights)
                return false;

            //TODO change hardcoded 16
            var groupCounts = new int[16];
            var groupTextures = new Texture[16];

            // TODO: optimize OR consider that this will always be relatively small
            foreach (var light in directionalLightsWithShadowForMesh)
            {
                var notFound = true;
                if (config.ShadowConfigurations != null)
                {
                    for (var i = 0; i < config.ShadowConfigurations.Groups.Count; ++i)
                    {
                        if (BelongToGroup(light.Light, light.ShadowMap, config.ShadowConfigurations.Groups[i], groupCounts[i], groupTextures[i]))
                        {
                            groupCounts[i] += 1;
                            if (groupTextures[i] == null)
                                groupTextures[i] = light.ShadowMap.Texture.ShadowMapDepthTexture;
                            notFound = false;
                            break;
                        }
                    }
                }
                if (notFound)
                    return false;
            }
            foreach (var light in spotLightsWithShadowForMesh)
            {
                var notFound = true;
                if (config.ShadowConfigurations != null)
                {
                    for (var i = 0; i < config.ShadowConfigurations.Groups.Count; ++i)
                    {
                        if (BelongToGroup(light.Light, light.ShadowMap, config.ShadowConfigurations.Groups[i], groupCounts[i], groupTextures[i]))
                        {
                            groupCounts[i] += 1;
                            if (groupTextures[i] == null)
                                groupTextures[i] = light.ShadowMap.Texture.ShadowMapDepthTexture;
                            notFound = false;
                            break;
                        }
                    }
                }
                if (notFound)
                    return false;
            }
            return true;
        }

        private void AssignGroups(LightingConfiguration config)
        {
            // TODO: optimize the groups based on the maximum number of shadow maps so that when there is a solution, it is chosen

            // TODO: add shadow group and directional light shadow group (list) if necessary
            for (var i = shadowMapGroups.Count; i < config.ShadowConfigurations.Groups.Count; ++i)
                shadowMapGroups.Add(new List<ShadowMap>());
            
            for (var i = directionalLightsWithShadowForMeshGroups.Count; i < config.ShadowConfigurations.Groups.Count; ++i)
                directionalLightsWithShadowForMeshGroups.Add(new List<EntityLightShadow>());
            foreach (var light in directionalLightsWithShadowForMesh)
            {
                for (var i = 0; i < config.ShadowConfigurations.Groups.Count; ++i)
                {
                    if (BelongToGroup(light.Light, light.ShadowMap, config.ShadowConfigurations.Groups[i], shadowMapGroups[i].Count, shadowMapGroups[i].Count > 0 ? shadowMapGroups[i][0].Texture.ShadowMapDepthTexture : null))
                    {
                        shadowMapGroups[i].Add(light.ShadowMap);
                        directionalLightsWithShadowForMeshGroups[i].Add(light);
                        break;
                    }
                }
            }

            for (var i = spotLightsWithShadowForMeshGroups.Count; i < config.ShadowConfigurations.Groups.Count; ++i)
                spotLightsWithShadowForMeshGroups.Add(new List<EntityLightShadow>());
            foreach (var light in spotLightsWithShadowForMesh)
            {
                for (var i = 0; i < config.ShadowConfigurations.Groups.Count; ++i)
                {
                    if (BelongToGroup(light.Light, light.ShadowMap, config.ShadowConfigurations.Groups[i], shadowMapGroups[i].Count, shadowMapGroups[i].Count > 0 ? shadowMapGroups[i][0].Texture.ShadowMapDepthTexture : null))
                    {
                        shadowMapGroups[i].Add(light.ShadowMap);
                        spotLightsWithShadowForMeshGroups[i].Add(light);
                        break;
                    }
                }
            }
        }

        private void UpdateLightingParameters(LightingUpdateInfo info, ref RenderMesh renderMesh, ref Matrix viewMatrix, List<EntityLightShadow> lightsForMesh)
        {
            var maxLights = info.Count;
            if (maxLights > 0)
            {
                Matrix worldView;
                Vector3 lightDir;
                Vector3 lightPos;
                Vector3 direction;
                Vector3 position;
                var lightCount = 0;
                foreach (var light in lightsForMesh)
                {
                    if ((info.Semantic & LightParamSemantic.PositionDirectionVS) != 0)
                    {
                        if ((info.Semantic & LightParamSemantic.DirectionVS) != 0)
                        {
                            Matrix.Multiply(ref light.Entity.Transformation.WorldMatrix, ref viewMatrix, out worldView);
                            lightDir = light.Light.LightDirection;
                            Vector3.TransformNormal(ref lightDir, ref worldView, out direction);
                            arrayVector3[lightCount] = direction;
                        }
                        if ((info.Semantic & LightParamSemantic.PositionVS) != 0)
                        {
                            lightPos = light.Entity.Transformation.Translation;
                            Vector3.TransformCoordinate(ref lightPos, ref viewMatrix, out position);
                            arrayVector3[lightCount + maxLights] = position;
                        }
                    }
                    else if ((info.Semantic & LightParamSemantic.PositionDirectionWS) != 0)
                    {
                        if ((info.Semantic & LightParamSemantic.DirectionWS) != 0)
                        {
                            lightDir = light.Light.LightDirection;
                            Vector3.TransformNormal(ref lightDir, ref light.Entity.Transformation.WorldMatrix, out direction);
                            arrayVector3[lightCount] = direction;
                        }
                        if ((info.Semantic & LightParamSemantic.PositionWS) != 0)
                        {
                            lightPos = light.Entity.Transformation.Translation;
                            arrayVector3[lightCount + maxLights] = lightPos;
                        }
                    }
                    if ((info.Semantic & LightParamSemantic.ColorWithGamma) != 0)
                    {
                        //color.R = (float)Math.Pow(light.Light.Color.R, 2.2);
                        //color.G = (float)Math.Pow(light.Light.Color.G, 2.2);
                        //color.B = (float)Math.Pow(light.Light.Color.B, 2.2);
                        //arrayColor3[lightCount] = color;
                        arrayColor3[lightCount] = light.Light.Color;
                    }
                    if ((info.Semantic & LightParamSemantic.Intensity) != 0)
                    {
                        arrayFloat[lightCount] = light.Light.Intensity;
                    }
                    if ((info.Semantic & LightParamSemantic.Decay) != 0)
                    {
                        arrayFloat[lightCount + maxLights] = light.Light.DecayStart;
                    }
                    if ((info.Semantic & LightParamSemantic.SpotBeamAngle) != 0)
                    {
                        arrayFloat[lightCount + 2 * maxLights] = (float)Math.Cos(Math.PI * light.Light.SpotBeamAngle / 180);
                    }
                    if ((info.Semantic & LightParamSemantic.SpotFieldAngle) != 0)
                    {
                        arrayFloat[lightCount + 3 * maxLights] = (float)Math.Cos(Math.PI * light.Light.SpotFieldAngle / 180);
                    }

                    ++lightCount;
                    if (lightCount >= maxLights)
                        break;
                }

                if ((info.Semantic & LightParamSemantic.DirectionVS) != 0)
                    renderMesh.Parameters.Set(info.DirectionKey, arrayVector3, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.PositionVS) != 0)
                    renderMesh.Parameters.Set(info.PositionKey, arrayVector3, maxLights, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.DirectionWS) != 0)
                    renderMesh.Parameters.Set(info.DirectionKey, arrayVector3, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.PositionWS) != 0)
                    renderMesh.Parameters.Set(info.PositionKey, arrayVector3, maxLights, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.ColorWithGamma) != 0)
                    renderMesh.Parameters.Set(info.ColorKey, arrayColor3, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.Intensity) != 0)
                    renderMesh.Parameters.Set(info.IntensityKey, arrayFloat, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.Decay) != 0)
                    renderMesh.Parameters.Set(info.DecayKey, arrayFloat, maxLights, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.SpotBeamAngle) != 0)
                    renderMesh.Parameters.Set(info.SpotBeamAngleKey, arrayFloat, 2 * maxLights, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.SpotFieldAngle) != 0)
                    renderMesh.Parameters.Set(info.SpotFieldAngleKey, arrayFloat, 3 * maxLights, 0, lightCount);
                if ((info.Semantic & LightParamSemantic.Count) != 0)
                    renderMesh.Parameters.Set(info.LightCountKey, lightCount);
            }
            else
            {
                if ((info.Semantic & LightParamSemantic.Count) != 0)
                    renderMesh.Parameters.Set(info.LightCountKey, 0);
            }
        }

        private void UpdateShadowParameters(ParameterCollection parameters, ShadowUpdateInfo shadowUpdateInfo, List<ShadowMap> shadows)
        {
            if (shadows != null && shadows.Count > 0)
            {
                var count = 0;
                var cascadeCount = 0;
                foreach (var shadow in shadows)
                {
                    receiverInfos[count] = shadow.ReceiverInfo;
                    receiverVsmInfos[count] = shadow.ReceiverVsmInfo;
                    for (var i = 0; i < shadowUpdateInfo.CascadeCount; ++i)
                        cascadeInfos[cascadeCount + i] = shadow.Cascades[i].ReceiverInfo;
                    ++count;
                    cascadeCount += shadowUpdateInfo.CascadeCount;
                }

                parameters.Set((ParameterKey<ShadowMapReceiverInfo[]>)shadowUpdateInfo.ShadowMapReceiverInfoKey, receiverInfos, 0, count);
                parameters.Set((ParameterKey<ShadowMapCascadeReceiverInfo[]>)shadowUpdateInfo.ShadowMapLevelReceiverInfoKey, cascadeInfos, 0, cascadeCount);
                if (shadows[0].Filter == ShadowMapFilterType.Variance)
                {
                    parameters.Set((ParameterKey<ShadowMapReceiverVsmInfo[]>)shadowUpdateInfo.ShadowMapReceiverVsmInfoKey, receiverVsmInfos, 0, count);
                    parameters.Set(shadowUpdateInfo.ShadowMapTextureKey, shadows[0].Texture.ShadowMapTargetTexture);
                }
                else
                    parameters.Set(shadowUpdateInfo.ShadowMapTextureKey, shadows[0].Texture.ShadowMapDepthTexture);
                parameters.Set(shadowUpdateInfo.ShadowMapLightCountKey, count);
            }
            else
                parameters.Set(shadowUpdateInfo.ShadowMapLightCountKey, 0);
        }

        private List<LightingUpdateInfo> CreateLightingUpdateInfo(RenderMesh renderMesh)
        {
            var finalList = new List<LightingUpdateInfo>();
            var continueSearch = true;
            var index = 0;
            while (continueSearch)
            {
                continueSearch = SearchShadingGroup(renderMesh, index, "ShadingGroups", 0, finalList);
                ++index;
            }

            continueSearch = true;
            index = 0;
            while (continueSearch)
            {
                continueSearch = SearchShadingGroup(renderMesh, index, "shadows", 3, finalList);
                ++index;
            }

            return finalList;
        }

        private bool SearchShadingGroup(RenderMesh renderMesh, int index, string groupName, int typeOffset, List<LightingUpdateInfo> finalList)
        {
            var constantBuffers = renderMesh.Effect.Bytecode.Reflection.ConstantBuffers;
            var info = new LightingUpdateInfo();

            LightParamSemantic foundParameterSemantic;
            var foundParam = false;
            var lightTypeGuess = LightTypeGuess.None;
            
            UpdateLightingParameterSemantics(index, groupName);

            foreach (var constantBuffer in constantBuffers)
            {
                foreach (var member in constantBuffer.Members)
                {
                    if (lightingParameterSemantics.TryGetValue(member.Param.Key, out foundParameterSemantic))
                    {
                        info.Semantic = info.Semantic | foundParameterSemantic;
                        foundParam = true;
                        switch (foundParameterSemantic)
                        {
                            case LightParamSemantic.PositionVS:
                                info.PositionKey = (ParameterKey<Vector3[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.PositionKey, new Vector3[member.Count]);
                                info.Count = member.Count;
                                lightTypeGuess = lightTypeGuess | LightTypeGuess.Point;
                                break;
                            case LightParamSemantic.DirectionVS:
                                info.DirectionKey = (ParameterKey<Vector3[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.DirectionKey, new Vector3[member.Count]);
                                info.Count = member.Count;
                                lightTypeGuess = lightTypeGuess | LightTypeGuess.Directional;
                                break;
                            case LightParamSemantic.PositionWS:
                                info.PositionKey = (ParameterKey<Vector3[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.PositionKey, new Vector3[member.Count]);
                                info.Count = member.Count;
                                lightTypeGuess = lightTypeGuess | LightTypeGuess.Point;
                                break;
                            case LightParamSemantic.DirectionWS:
                                info.DirectionKey = (ParameterKey<Vector3[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.DirectionKey, new Vector3[member.Count]);
                                info.Count = member.Count;
                                lightTypeGuess = lightTypeGuess | LightTypeGuess.Directional;
                                break;
                            case LightParamSemantic.ColorWithGamma:
                                info.ColorKey = (ParameterKey<Color3[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.ColorKey, new Color3[member.Count]);
                                info.Count = member.Count;
                                break;
                            case LightParamSemantic.Intensity:
                                info.IntensityKey = (ParameterKey<float[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.IntensityKey, new float[member.Count]);
                                info.Count = member.Count;
                                break;
                            case LightParamSemantic.Decay:
                                info.DecayKey = (ParameterKey<float[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.DecayKey, new float[member.Count]);
                                info.Count = member.Count;
                                lightTypeGuess = lightTypeGuess | LightTypeGuess.Point;
                                break;
                            case LightParamSemantic.SpotBeamAngle:
                                info.SpotBeamAngleKey = (ParameterKey<float[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.SpotBeamAngleKey, new float[member.Count]);
                                info.Count = member.Count;
                                lightTypeGuess = lightTypeGuess | LightTypeGuess.Spot;
                                break;
                            case LightParamSemantic.SpotFieldAngle:
                                info.SpotFieldAngleKey = (ParameterKey<float[]>)member.Param.Key;
                                renderMesh.Parameters.Set(info.SpotFieldAngleKey, new float[member.Count]);
                                info.Count = member.Count;
                                lightTypeGuess = lightTypeGuess | LightTypeGuess.Spot;
                                break;
                            case LightParamSemantic.Count:
                                info.LightCountKey = (ParameterKey<int>)member.Param.Key;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            if (foundParam)
            {
                switch (lightTypeGuess)
                {
                    case LightTypeGuess.Directional:
                        info.Type = LightingUpdateType.Directional + typeOffset;
                        break;
                    case LightTypeGuess.Point:
                        info.Type = LightingUpdateType.Point + typeOffset;
                        break;
                    case LightTypeGuess.Spot:
                        info.Type = LightingUpdateType.Spot + typeOffset;
                        break;
                }
                if (lightTypeGuess != LightTypeGuess.None)
                {
                    info.Index = index;
                    finalList.Add(info);
                }
            }
            return foundParam;
        }

        private static List<ShadowUpdateInfo> CreateEffectShadowParams(LightingConfiguration config)
        {
            var configs = new List<ShadowUpdateInfo>();

            for (var i = 0; i < config.ShadowConfigurations.Groups.Count; ++i)
            {
                var group = LightingProcessorHelpers.CreateShadowUpdateInfo(i, config.ShadowConfigurations.Groups[i].CascadeCount);
                configs.Add(group);
            }

            return configs;
        }

        private void UpdateLightingParameterSemantics(int index, string compositionName)
        {
            lightingParameterSemantics.Clear();
            // TODO: use StringBuilder instead
            var lightGroupSubKey = string.Format("{0}[{1}]", compositionName, index);
            foreach (var param in LightParametersDict)
            {
                lightingParameterSemantics.Add(param.Key.ComposeWith(lightGroupSubKey), param.Value);
            }
        }

        #endregion

        #region Helpers

        [Flags]
        private enum LightTypeGuess
        {
            None = 0x0,
            Directional = 0x1, // direction is needed
            Point = 0x2, // position is needed
            Spot = Directional | Point // angle falloff is needed
        }

        private static bool BelongToGroup(LightComponent light, ShadowMap shadow, ShadowConfiguration config, int groupCount, Texture groupTexture)
        {
            return light.ShadowMapCascadeCount == config.CascadeCount
                && light.ShadowMapFilterType == config.FilterType
                && groupCount < config.ShadowCount
                && (groupTexture == null || groupTexture == shadow.Texture.ShadowMapDepthTexture);
        }

        private static void CreateParametersFromLightingConfiguration(LightingConfiguration config, ParameterCollection parameters)
        {
            // Apply parameters for effect change
            parameters.Set(LightingKeys.MaxDirectionalLights, config.MaxNumDirectionalLight);
            parameters.Set(LightingKeys.MaxPointLights, config.MaxNumPointLight);
            parameters.Set(LightingKeys.MaxSpotLights, config.MaxNumSpotLight);

            // TODO: cache some objects since it is done at each frame
            // TODO: try to reuse the parameter collections?

            if (config.ShadowConfigurations != null)
            {
                var groupCount = 0;
                foreach (var group in config.ShadowConfigurations.Groups)
                    groupCount += (group.ShadowCount > 0 ? 1 : 0);

                if (groupCount == 0)
                {
                    parameters.Remove(ShadowMapParameters.ShadowMaps);
                    return;
                }

                var shadowMapParameters = new ShadowMapParameters[groupCount];
                var index = 0;
                for (var i = 0; i < config.ShadowConfigurations.Groups.Count; ++i)
                {
                    if (config.ShadowConfigurations.Groups[i].ShadowCount > 0)
                    {
                        var shadowParams = new ShadowMapParameters();
                        shadowParams.Set(ShadowMapParameters.LightType, config.ShadowConfigurations.Groups[i].LightType);
                        shadowParams.Set(ShadowMapParameters.ShadowMapCount, config.ShadowConfigurations.Groups[i].ShadowCount);
                        shadowParams.Set(ShadowMapParameters.ShadowMapCascadeCount, config.ShadowConfigurations.Groups[i].CascadeCount);
                        shadowParams.Set(ShadowMapParameters.FilterType, config.ShadowConfigurations.Groups[i].FilterType);
                        shadowMapParameters[index] = shadowParams;
                        ++index;
                    }
                }
                parameters.Set(ShadowMapParameters.ShadowMaps, shadowMapParameters);
            }
            else
                parameters.Remove(ShadowMapParameters.ShadowMaps);
            
            //mesh.SharedParameters.Set(LightingKeys.UnrollDirectionalLightLoop, foundConfiguration.UnrollDirectionalLightLoop);
            //mesh.SharedParameters.Set(LightingKeys.UnrollPointLightLoop, foundConfiguration.UnrollPointLightLoop);
            //mesh.SharedParameters.Set(LightingKeys.UnrollSpotLightLoop, foundConfiguration.UnrollSpotLightLoop);
        }

        private struct ShadowMapGroup
        {
            public ShadowMapTexture Texture;
            public int CascadeCount;
            public ShadowMapFilterType FilterType;
        }

        private static List<ShadowConfiguration> CreateShadowConfiguration(List<EntityLightShadow> lights, LightType lightType)
        {
            var resultList = new List<ShadowConfiguration>();
            var groupedShadowLights = lights.GroupBy(x => new ShadowMapGroup() { Texture = x.ShadowMap.Texture, CascadeCount = x.ShadowMap.CascadeCount, FilterType = x.ShadowMap.Filter });
            foreach (var lightGroup in groupedShadowLights)
            {
                var shadowGroup = new ShadowConfiguration()
                {
                    CascadeCount = lightGroup.Key.CascadeCount,
                    FilterType = lightGroup.Key.FilterType,
                    LightType = lightType,
                    ShadowCount = lightGroup.Count()
                };
                resultList.Add(shadowGroup);
            }
            return resultList;
        }

        #endregion
    }

    public struct LightGroup
    {
        public Dictionary<ParameterKey, LightParamSemantic> LightingParameterSemantics;

        public LightGroup(int index, string compositionName)
        {
            LightingParameterSemantics = new Dictionary<ParameterKey, LightParamSemantic>();
            // TODO: use StringBuilder instead
            var lightGroupSubKey = string.Format("{0}[{1}]", compositionName, index);
            foreach (var param in LightForwardModelRenderer.LightParametersDict)
            {
                LightingParameterSemantics.Add(param.Key.ComposeWith(lightGroupSubKey), param.Value);
            }
        }
    }
}
