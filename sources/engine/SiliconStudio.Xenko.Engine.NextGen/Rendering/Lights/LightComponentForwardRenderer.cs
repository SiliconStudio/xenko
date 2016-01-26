// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// TODO: Refactor this class
    /// </summary>
    public class LightComponentForwardRenderer : RendererBase
    {
        private const string DirectLightGroupsCompositionName = "directLightGroups";
        private const string EnvironmentLightsCompositionName = "environmentLights";

        private bool isModelComponentRendererSetup;

        private LightProcessor lightProcessor;

        // Might be null if shadow mapping is not enabled (i.e. graphics device feature level too low)
        private ShadowMapRenderer shadowMapRenderer;

        private readonly List<KeyValuePair<Type, LightGroupRendererBase>> lightRenderers;

        private ModelProcessor modelProcessor;

        private CameraComponent sceneCamera;

        private readonly List<LightComponent> visibleLights;

        private readonly List<LightComponent> visibleLightsWithShadows;

        private readonly List<ActiveLightGroupRenderer> activeRenderers;

        private SceneCameraRenderer sceneCameraRenderer;

        private EntityGroupMask sceneCullingMask;

        private readonly Dictionary<ObjectId, LightShaderPermutationEntry> shaderEntries;
        private readonly Dictionary<ObjectId, LightParametersPermutationEntry> lightParameterEntries;

        private readonly List<LightEntry> directLightsPerModel = new List<LightEntry>();
        private FastListStruct<LightForwardShaderFullEntryKey> directLightShaderGroupEntryKeys;

        private readonly List<LightEntry> environmentLightsPerModel = new List<LightEntry>();
        private FastListStruct<LightForwardShaderFullEntryKey> environmentLightShaderGroupEntryKeys;
        private FastListStruct<LightForwardShaderFullEntryKey> directLightShaderGroupEntryKeysNoShadows;

        private PoolListStruct<LightParametersPermutationEntry> parameterCollectionEntryPool;

        private LightShaderPermutationEntry currentModelLightShadersPermutationEntry;

        private LightParametersPermutationEntry currentModelShadersParameters;

        private bool currentShadowReceiver;

        private readonly Dictionary<RenderModel, RenderModelLights> modelToLights;

        /// <summary>
        /// Gets the lights without shadow per light type.
        /// </summary>
        /// <value>The lights.</value>
        private readonly Dictionary<Type, LightComponentCollectionGroup> activeLightGroups;

        /// <summary>
        /// Gets the lights without shadow per light type.
        /// </summary>
        /// <value>The lights.</value>
        private readonly Dictionary<Type, LightComponentCollectionGroup> activeLightGroupsWithShadows;

        private static readonly string[] DirectLightGroupsCompositionNames;
        private static readonly string[] EnvironmentLightGroupsCompositionNames;

        static LightComponentForwardRenderer()
        {
            // TODO: 32 is hardcoded and will generate a NullReferenceException in CreateShaderPermutationEntry
            DirectLightGroupsCompositionNames = new string[32];
            for (int i = 0; i < DirectLightGroupsCompositionNames.Length; i++)
            {
                DirectLightGroupsCompositionNames[i] = DirectLightGroupsCompositionName + "[" + i + "]";
            }
            EnvironmentLightGroupsCompositionNames = new string[32];
            for (int i = 0; i < EnvironmentLightGroupsCompositionNames.Length; i++)
            {
                EnvironmentLightGroupsCompositionNames[i] = EnvironmentLightsCompositionName + "[" + i + "]";
            }
        }

        public LightComponentForwardRenderer()
        {
            modelToLights = new Dictionary<RenderModel, RenderModelLights>(1024);
            directLightShaderGroupEntryKeys = new FastListStruct<LightForwardShaderFullEntryKey>(32);
            environmentLightShaderGroupEntryKeys = new FastListStruct<LightForwardShaderFullEntryKey>(32);
            directLightShaderGroupEntryKeysNoShadows = new FastListStruct<LightForwardShaderFullEntryKey>(32);
            parameterCollectionEntryPool = new PoolListStruct<LightParametersPermutationEntry>(16, CreateParameterCollectionEntry);

            //directLightGroup = new LightGroupRenderer("directLightGroups", LightingKeys.DirectLightGroups);
            //environmentLightGroup = new LightGroupRenderer("environmentLights", LightingKeys.EnvironmentLights);
            lightRenderers = new List<KeyValuePair<Type, LightGroupRendererBase>>(16);

            visibleLights = new List<LightComponent>(1024);
            visibleLightsWithShadows = new List<LightComponent>(1024);

            shaderEntries = new Dictionary<ObjectId, LightShaderPermutationEntry>(1024);

            directLightsPerModel = new List<LightEntry>(16);
            activeLightGroups = new Dictionary<Type, LightComponentCollectionGroup>(16);
            activeLightGroupsWithShadows = new Dictionary<Type, LightComponentCollectionGroup>(16);
            activeRenderers = new List<ActiveLightGroupRenderer>(16);

            lightParameterEntries = new Dictionary<ObjectId, LightParametersPermutationEntry>(32);

            // TODO: Make this pluggable
            RegisterLightGroupRenderer(typeof(LightDirectional), new LightDirectionalGroupRenderer());
            RegisterLightGroupRenderer(typeof(LightSpot), new LightSpotGroupRenderer());
            RegisterLightGroupRenderer(typeof(LightPoint), new LightPointGroupRenderer());
            RegisterLightGroupRenderer(typeof(LightAmbient), new LightAmbientRenderer());
            RegisterLightGroupRenderer(typeof(LightSkybox), new LightSkyboxRenderer());
        }

        protected void RegisterLightGroupRenderer(Type lightType, LightGroupRendererBase renderer)
        {
            if (lightType == null) throw new ArgumentNullException("lightType");
            if (renderer == null) throw new ArgumentNullException("renderer");
            lightRenderers.Add(new KeyValuePair<Type, LightGroupRendererBase>(lightType, renderer));
        }

        protected override void DrawCore(RenderContext context)
        {
            modelProcessor = SceneInstance.GetCurrent(context).GetProcessor<ModelProcessor>();
            lightProcessor = SceneInstance.GetCurrent(context).GetProcessor<LightProcessor>();

            // No light processors means no light in the scene, so we can early exit
            if (lightProcessor == null || modelProcessor == null)
            {
                return;
            }

            // Not in the context of a SceneCameraRenderer? just exit
            sceneCameraRenderer = context.Tags.Get(SceneCameraRenderer.Current);
            sceneCamera = context.Tags.Get(CameraComponentRenderer.Current);
            if (sceneCameraRenderer == null || sceneCamera == null)
            {
                return;
            }
            sceneCullingMask = sceneCameraRenderer.CullingMask;

            // Setup the callback on the ModelRenderer and shadow map LightGroupRenderer
            if (!isModelComponentRendererSetup)
            {
                // TODO: Check if we could discover declared renderers in a better way than just hacking the tags of a component
                var modelRenderer = ModelComponentRenderer.GetAttached(sceneCameraRenderer);
                if (modelRenderer == null)
                {
                    return;
                }

                //modelRenderer.Callbacks.PreRenderModel += PrepareRenderModelForRendering;
                //modelRenderer.Callbacks.PreRenderMesh += PreRenderMesh;
                //
                //// TODO: Make this pluggable
                //// TODO: Shadows should work on mobile platforms
                //if (context.GraphicsDevice.Features.Profile >= GraphicsProfile.Level_10_0
                //    && (Platform.Type == PlatformType.Windows || Platform.Type == PlatformType.WindowsStore || Platform.Type == PlatformType.Windows10))
                //{
                //    shadowMapRenderer = new ShadowMapRenderer(modelRenderer.EffectName);
                //    shadowMapRenderer.Renderers.Add(typeof(LightDirectional), new LightDirectionalShadowMapRenderer());
                //    shadowMapRenderer.Renderers.Add(typeof(LightSpot), new LightSpotShadowMapRenderer());
                //}

                isModelComponentRendererSetup = true;
            }

            // Collect all visible lights
            CollectVisibleLights();

            // Draw shadow maps
            if (shadowMapRenderer != null)
                shadowMapRenderer.Draw(context, visibleLightsWithShadows);

            // Prepare active renderers in an ordered list (by type and shadow on/off)
            CollectActiveLightRenderers(context);

            currentModelLightShadersPermutationEntry = null;
            currentModelShadersParameters = null;
            currentShadowReceiver = true;

            // Clear the cache of parameter entries
            lightParameterEntries.Clear();
            parameterCollectionEntryPool.Clear();

            // Clear association between model and lights
            modelToLights.Clear();

            // Clear all data generated by shader entries
            foreach (var shaderEntry in shaderEntries)
            {
                shaderEntry.Value.ResetGroupDatas();
            }
        }

        private void CollectActiveLightRenderers(RenderContext context)
        {
            activeRenderers.Clear();
            foreach (var lightTypeAndRenderer in lightRenderers)
            {
                LightComponentCollectionGroup lightGroup;
                activeLightGroups.TryGetValue(lightTypeAndRenderer.Key, out lightGroup);

                var renderer = lightTypeAndRenderer.Value;
                bool rendererToInitialize = false;
                if (lightGroup != null && lightGroup.Count > 0)
                {
                    activeRenderers.Add(new ActiveLightGroupRenderer(renderer, lightGroup));
                    rendererToInitialize = true;
                }

                if (renderer.CanHaveShadows)
                {
                    LightComponentCollectionGroup lightGroupWithShadows;
                    activeLightGroupsWithShadows.TryGetValue(lightTypeAndRenderer.Key, out lightGroupWithShadows);

                    if (lightGroupWithShadows != null && lightGroupWithShadows.Count > 0)
                    {
                        activeRenderers.Add(new ActiveLightGroupRenderer(renderer, lightGroupWithShadows));
                        rendererToInitialize = true;
                    }
                }

                if (rendererToInitialize)
                {
                    renderer.Initialize(context);
                }
            }
        }

        /// <summary>
        /// Collects the visible lights by intersecting them with the frustum.
        /// </summary>
        private void CollectVisibleLights()
        {
            visibleLights.Clear();
            visibleLightsWithShadows.Clear();

            // 1) Clear the cache of current lights (without destroying collections but keeping previously allocated ones)
            ClearCache(activeLightGroups);
            ClearCache(activeLightGroupsWithShadows);

            // 2) Cull lights with the frustum
            var frustum = sceneCamera.Frustum;
            foreach (var light in lightProcessor.Lights)
            {
                // If light is not part of the culling mask group, we can skip it
                var entityLightMask = (EntityGroupMask)(1 << (int)light.Entity.Group);
                if ((entityLightMask & sceneCullingMask) == 0 && (light.CullingMask & sceneCullingMask) == 0)
                {
                    continue;
                }

                // If light is not in the frustum, we can skip it
                var directLight = light.Type as IDirectLight;
                if (directLight != null && directLight.HasBoundingBox && !frustum.Contains(ref light.BoundingBoxExt))
                {
                    continue;
                }

                // Find the group for this light
                var lightGroup = GetLightGroup(light);
                lightGroup.PrepareLight(light);

                // This is a visible light
                visibleLights.Add(light);

                // Add light to a special list if it has shadows
                if (directLight != null && directLight.Shadow.Enabled && shadowMapRenderer != null)
                {
                    // A visible light with shadows
                    visibleLightsWithShadows.Add(light);
                }
            }

            // 3) Allocate collection based on their culling mask
            AllocateCollectionsPerGroupOfCullingMask(activeLightGroups);
            AllocateCollectionsPerGroupOfCullingMask(activeLightGroupsWithShadows);

            // 4) Collect lights to the correct light collection group
            foreach (var light in visibleLights)
            {
                var lightGroup = GetLightGroup(light);
                lightGroup.AddLight(light);
            }
        }

        private bool PrepareRenderModelForRendering(RenderContext context, RenderModel model)
        {
            var shaderKeyIdBuilder = new ObjectIdSimpleBuilder();
            var parametersKeyIdBuilder = new ObjectIdSimpleBuilder();
            //var idBuilder = new ObjectIdBuilder();

            var modelComponent = model.ModelComponent;
            var group = modelComponent.Entity.Group;
            var modelBoundingBox = modelComponent.BoundingBox;

            directLightsPerModel.Clear();
            directLightShaderGroupEntryKeys.Clear();
            directLightShaderGroupEntryKeysNoShadows.Clear();

            environmentLightsPerModel.Clear();
            environmentLightShaderGroupEntryKeys.Clear();

            // This loop is looking for visible lights per render model and calculate a ShaderId and ParametersId
            // TODO: Part of this loop could be processed outisde of the PrepareRenderModelForRendering
            // For example: Environment lights or directional lights are always active, so we could pregenerate part of the 
            // id and groups outside this loop. Also considering that each light renderer has a maximum of lights
            // we could pre
            foreach (var activeRenderer in activeRenderers)
            {
                var lightRenderer = activeRenderer.LightRenderer;
                var lightCollection = activeRenderer.LightGroup.FindLightCollectionByGroup(group);

                var lightCount = lightCollection == null ? 0 : lightCollection.Count;
                int lightMaxCount = Math.Min(lightCount, lightRenderer.LightMaxCount);
                var lightRendererId = lightRenderer.LightRendererId;
                var allocCountForNewLightType = lightRenderer.AllocateLightMaxCount ? (byte)lightRenderer.LightMaxCount : (byte)1;

                var currentShaderKey = new LightForwardShaderEntryKey();

                // Path for environment lights
                if (lightRenderer.IsEnvironmentLight)
                {
                    // The loop is simpler for environment lights (single group per light, no shadow maps, no bounding box...etc)

                    for (int i = 0; i < lightMaxCount; i++)
                    {
                        var light = lightCollection[i];
                        currentShaderKey = new LightForwardShaderEntryKey(lightRendererId, 0, allocCountForNewLightType);
                        unsafe
                        {
                            shaderKeyIdBuilder.Write(*(uint*)&currentShaderKey);
                        }
                        parametersKeyIdBuilder.Write(light.Id);

                        environmentLightsPerModel.Add(new LightEntry(environmentLightShaderGroupEntryKeys.Count, 0, light, null));
                        environmentLightShaderGroupEntryKeys.Add(new LightForwardShaderFullEntryKey(currentShaderKey, lightRenderer, null));
                    }
                }
                else
                {
                    ILightShadowMapRenderer currentShadowRenderer = null;

                    for (int i = 0; i < lightMaxCount; i++)
                    {
                        var light = lightCollection[i];
                        var directLight = (IDirectLight)light.Type;
                        // If the light does not intersects the model, we can skip it
                        if (directLight.HasBoundingBox && !light.BoundingBox.Intersects(ref modelBoundingBox))
                        {
                            continue;
                        }

                        LightShadowMapTexture shadowTexture = null;
                        LightShadowType shadowType = 0;
                        ILightShadowMapRenderer newShadowRenderer = null;

                        if (shadowMapRenderer != null && shadowMapRenderer.LightComponentsWithShadows.TryGetValue(light, out shadowTexture))
                        {
                            shadowType = shadowTexture.ShadowType;
                            newShadowRenderer = shadowTexture.Renderer;
                        }

                        if (i == 0)
                        {
                            currentShaderKey = new LightForwardShaderEntryKey(lightRendererId, shadowType, allocCountForNewLightType);
                            currentShadowRenderer = newShadowRenderer;
                        }
                        else
                        {
                            if (currentShaderKey.LightRendererId == lightRendererId && currentShaderKey.ShadowType == shadowType)
                            {
                                if (!lightRenderer.AllocateLightMaxCount)
                                {
                                    currentShaderKey.LightCount++;
                                }
                            }
                            else
                            {
                                unsafe
                                {
                                    shaderKeyIdBuilder.Write(*(uint*)&currentShaderKey);
                                }

                                directLightShaderGroupEntryKeys.Add(new LightForwardShaderFullEntryKey(currentShaderKey, lightRenderer, currentShadowRenderer));
                                currentShaderKey = new LightForwardShaderEntryKey(lightRendererId, shadowType, allocCountForNewLightType);
                                currentShadowRenderer = newShadowRenderer;
                            }
                        }

                        parametersKeyIdBuilder.Write(light.Id);
                        directLightsPerModel.Add(new LightEntry(directLightShaderGroupEntryKeys.Count, directLightShaderGroupEntryKeysNoShadows.Count, light, shadowTexture));
                    }

                    if (directLightsPerModel.Count > 0)
                    {
                        directLightShaderGroupEntryKeysNoShadows.Add(new LightForwardShaderFullEntryKey(new LightForwardShaderEntryKey(lightRendererId, 0, (byte)directLightsPerModel.Count), lightRenderer, null));

                        unsafe
                        {
                            shaderKeyIdBuilder.Write(*(uint*)&currentShaderKey);
                        }
                        directLightShaderGroupEntryKeys.Add(new LightForwardShaderFullEntryKey(currentShaderKey, lightRenderer, currentShadowRenderer));
                    }
                }
            }

            // Find or create an existing shaders/parameters permutation

            // Build the keys for Shaders and Parameters permutations
            ObjectId shaderKeyId;
            ObjectId parametersKeyId;
            shaderKeyIdBuilder.ComputeHash(out shaderKeyId);
            parametersKeyIdBuilder.ComputeHash(out parametersKeyId);

            // Calculate the shader parameters just once
            // If we don't have already this permutation, use it
            LightShaderPermutationEntry newLightShaderPermutationEntry;
            if (!shaderEntries.TryGetValue(shaderKeyId, out newLightShaderPermutationEntry))
            {
                newLightShaderPermutationEntry = CreateShaderPermutationEntry();
                shaderEntries.Add(shaderKeyId, newLightShaderPermutationEntry);
            }

            LightParametersPermutationEntry newShaderEntryParameters;
            // Calculate the shader parameters just once per light combination and for this rendering pass
            if (!lightParameterEntries.TryGetValue(parametersKeyId, out newShaderEntryParameters))
            {
                newShaderEntryParameters = CreateParametersPermutationEntry(newLightShaderPermutationEntry);
                lightParameterEntries.Add(parametersKeyId, newShaderEntryParameters);
            }

            modelToLights.Add(model, new RenderModelLights(newLightShaderPermutationEntry, newShaderEntryParameters));

            return true;
        }

        private void PreRenderMesh(RenderContext context, RenderMesh renderMesh)
        {
            var contextParameters = context.Parameters;
            RenderModelLights renderModelLights;
            if (!modelToLights.TryGetValue(renderMesh.RenderModel, out renderModelLights))
            {
                contextParameters.Set(LightingKeys.DirectLightGroups, null);
                contextParameters.Set(LightingKeys.EnvironmentLights, null);
                return;
            }

            // TODO: copy shadow receiver info to mesh
            var isShadowReceiver = renderMesh.Material.IsShadowReceiver;
            if (currentModelLightShadersPermutationEntry != renderModelLights.LightShadersPermutation || currentModelShadersParameters != renderModelLights.Parameters || currentShadowReceiver != isShadowReceiver)
            {
                currentModelLightShadersPermutationEntry = renderModelLights.LightShadersPermutation;
                currentModelShadersParameters = renderModelLights.Parameters;
                currentShadowReceiver = isShadowReceiver;

                if (currentShadowReceiver)
                {
                    currentModelShadersParameters.Parameters.CopySharedTo(contextParameters);
                }
                else
                {
                    currentModelShadersParameters.ParametersNoShadows.CopySharedTo(contextParameters);
                }
            }
        }

        private LightShaderPermutationEntry CreateShaderPermutationEntry()
        {
            var shaderEntry = new LightShaderPermutationEntry();

            // Direct Lights (with or without shadows)
            for (int i = 0; i < directLightShaderGroupEntryKeys.Count; i++)
            {
                var shaderGroupEntry = directLightShaderGroupEntryKeys.Items[i];
                int lightCount = shaderGroupEntry.Key.LightCount;

                ILightShadowMapShaderGroupData shadowGroupData = null;
                if (shaderGroupEntry.ShadowRenderer != null)
                {
                    // TODO: Cache ShaderGroupData
                    shadowGroupData = shaderGroupEntry.ShadowRenderer.CreateShaderGroupData(DirectLightGroupsCompositionNames[i], shaderGroupEntry.Key.ShadowType, lightCount);
                }
                // TODO: Cache LightShaderGroup
                var lightShaderGroup = shaderGroupEntry.LightGroupRenderer.CreateLightShaderGroup(DirectLightGroupsCompositionNames[i], lightCount, shadowGroupData);

                shaderEntry.DirectLightGroups.Add(lightShaderGroup);
                shaderEntry.DirectLightShaders.Add(lightShaderGroup.ShaderSource);
            }

            // All Direct Lights
            for (int i = 0; i < directLightShaderGroupEntryKeysNoShadows.Count; i++)
            {
                var shaderGroupEntry = directLightShaderGroupEntryKeysNoShadows.Items[i];

                // TODO: Cache LightShaderGroup
                var lightShaderGroup = shaderGroupEntry.LightGroupRenderer.CreateLightShaderGroup(DirectLightGroupsCompositionNames[i], shaderGroupEntry.Key.LightCount, null);

                shaderEntry.DirectLightGroupsNoShadows.Add(lightShaderGroup);
                shaderEntry.DirectLightShadersNoShadows.Add(lightShaderGroup.ShaderSource);
            }

            // All Environment lights
            for (int i = 0; i < environmentLightShaderGroupEntryKeys.Count; i++)
            {
                var shaderGroupEntry = environmentLightShaderGroupEntryKeys.Items[i];

                // TODO: Cache LightShaderGroup
                var lightShaderGroup = shaderGroupEntry.LightGroupRenderer.CreateLightShaderGroup(EnvironmentLightGroupsCompositionNames[i], shaderGroupEntry.Key.LightCount, null);

                shaderEntry.EnvironmentLights.Add(lightShaderGroup);
                shaderEntry.EnvironmentLightShaders.Add(lightShaderGroup.ShaderSource);
            }

            return shaderEntry;
        }

        private LightParametersPermutationEntry CreateParametersPermutationEntry(LightShaderPermutationEntry lightShaderPermutationEntry)
        {
            var parameterCollectionEntry = parameterCollectionEntryPool.Add();
            parameterCollectionEntry.Clear();

            var directLightGroups = parameterCollectionEntry.DirectLightGroupDatas;
            var directLightGroupsNoShadows = parameterCollectionEntry.DirectLightGroupsNoShadowDatas;
            var environmentLights = parameterCollectionEntry.EnvironmentLightDatas;

            foreach (var directLightGroup in lightShaderPermutationEntry.DirectLightGroups)
            {
                directLightGroups.Add(directLightGroup.CreateGroupData());
            }

            foreach (var directLightGroupNoShadow in lightShaderPermutationEntry.DirectLightGroupsNoShadows)
            {
                directLightGroupsNoShadows.Add(directLightGroupNoShadow.CreateGroupData());
            }

            foreach (var environmentLightGroup in lightShaderPermutationEntry.EnvironmentLights)
            {
                environmentLights.Add(environmentLightGroup.CreateGroupData());
            }

            var parameters = parameterCollectionEntry.Parameters;
            var parametersNoShadows = parameterCollectionEntry.ParametersNoShadows;

            parameters.Set(LightingKeys.DirectLightGroups, lightShaderPermutationEntry.DirectLightShaders);
            parameters.Set(LightingKeys.EnvironmentLights, lightShaderPermutationEntry.EnvironmentLightShaders);
            parametersNoShadows.Set(LightingKeys.DirectLightGroups, lightShaderPermutationEntry.DirectLightShadersNoShadows);
            parametersNoShadows.Set(LightingKeys.EnvironmentLights, lightShaderPermutationEntry.EnvironmentLightShaders);

            foreach (var lightEntry in directLightsPerModel)
            {
                directLightGroups[lightEntry.GroupIndex].AddLight(lightEntry.Light, lightEntry.Shadow);
                directLightGroupsNoShadows[lightEntry.GroupIndexNoShadows].AddLight(lightEntry.Light, null);
            }

            foreach (var lightEntry in environmentLightsPerModel)
            {
                environmentLights[lightEntry.GroupIndex].AddLight(lightEntry.Light, null);
            }

            foreach (var lightGroup in directLightGroups)
            {
                lightGroup.ApplyParameters(parameters);
            }

            foreach (var lightGroup in directLightGroupsNoShadows)
            {
                lightGroup.ApplyParameters(parametersNoShadows);
            }

            foreach (var lightGroup in environmentLights)
            {
                lightGroup.ApplyParameters(parameters);
                lightGroup.ApplyParameters(parametersNoShadows);
            }

            return parameterCollectionEntry;
        }

        private static void AllocateCollectionsPerGroupOfCullingMask(Dictionary<Type, LightComponentCollectionGroup> lights)
        {
            foreach (var lightPair in lights)
            {
                lightPair.Value.AllocateCollectionsPerGroupOfCullingMask();
            }
        }

        private static void ClearCache(Dictionary<Type, LightComponentCollectionGroup> lights)
        {
            foreach (var lightPair in lights)
            {
                lightPair.Value.Clear();
            }
        }

        private LightComponentCollectionGroup GetLightGroup(LightComponent light)
        {
            LightComponentCollectionGroup lightGroup;

            var directLight = light.Type as IDirectLight;
            var lightGroups = directLight != null && directLight.Shadow.Enabled && shadowMapRenderer != null
                ? activeLightGroupsWithShadows
                : activeLightGroups;

            var type = light.Type.GetType();
            if (!lightGroups.TryGetValue(type, out lightGroup))
            {
                lightGroup = new LightComponentCollectionGroup();
                lightGroups.Add(type, lightGroup);
            }
            return lightGroup;
        }

        private static LightParametersPermutationEntry CreateParameterCollectionEntry()
        {
            return new LightParametersPermutationEntry();
        }

        private struct LightEntry
        {
            public LightEntry(int currentLightGroupIndex, int currentLightGroupIndexNoShadows, LightComponent light, LightShadowMapTexture shadow)
            {
                GroupIndex = currentLightGroupIndex;
                GroupIndexNoShadows = currentLightGroupIndexNoShadows;
                Light = light;
                Shadow = shadow;
            }

            public readonly int GroupIndex;

            public readonly int GroupIndexNoShadows;

            public readonly LightComponent Light;

            public readonly LightShadowMapTexture Shadow;
        }

        /// <summary>
        /// We expect this class to be 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
        private struct LightForwardShaderEntryKey
        {
            public LightForwardShaderEntryKey(byte lightRendererId, LightShadowType shadowType, byte lightCount)
            {
                LightRendererId = lightRendererId;
                ShadowType = shadowType;
                LightCount = lightCount;
            }

            public readonly byte LightRendererId;

            public byte LightCount;

            public readonly LightShadowType ShadowType;
        }

        private struct LightForwardShaderFullEntryKey
        {
            public LightForwardShaderFullEntryKey(LightForwardShaderEntryKey key, LightGroupRendererBase lightGroupRenderer, ILightShadowMapRenderer shadowRenderer)
            {
                Key = key;
                LightGroupRenderer = lightGroupRenderer;
                ShadowRenderer = shadowRenderer;
            }

            public readonly LightForwardShaderEntryKey Key;

            public readonly LightGroupRendererBase LightGroupRenderer;

            public readonly ILightShadowMapRenderer ShadowRenderer;
        }

        private class LightShaderPermutationEntry
        {
            public LightShaderPermutationEntry()
            {
                DirectLightGroups = new List<LightShaderGroup>();
                DirectLightGroupsNoShadows = new List<LightShaderGroup>();
                DirectLightShadersNoShadows = new List<ShaderSource>();
                EnvironmentLights = new List<LightShaderGroup>();

                DirectLightShaders = new List<ShaderSource>();
                DirectLightGroupsNoShadows = new List<LightShaderGroup>();
                EnvironmentLightShaders = new List<ShaderSource>();
            }

            public void ResetGroupDatas()
            {
                foreach (var lightShaderGroup in DirectLightGroups)
                {
                    lightShaderGroup.Reset();
                }

                foreach (var lightShaderGroup in DirectLightGroupsNoShadows)
                {
                    lightShaderGroup.Reset();
                }

                foreach (var lightShaderGroup in EnvironmentLights)
                {
                    lightShaderGroup.Reset();
                }
            }

            public readonly List<LightShaderGroup> DirectLightGroups;

            public readonly List<ShaderSource> DirectLightShaders;

            public readonly List<LightShaderGroup> DirectLightGroupsNoShadows;

            public readonly List<ShaderSource> DirectLightShadersNoShadows;

            public readonly List<LightShaderGroup> EnvironmentLights;

            public readonly List<ShaderSource> EnvironmentLightShaders;
        }

        private struct ActiveLightGroupRenderer
        {
            public ActiveLightGroupRenderer(LightGroupRendererBase lightRenderer, LightComponentCollectionGroup lightGroup)
            {
                LightRenderer = lightRenderer;
                LightGroup = lightGroup;
            }

            public readonly LightGroupRendererBase LightRenderer;

            public readonly LightComponentCollectionGroup LightGroup;
        }

        private class LightParametersPermutationEntry
        {
            public LightParametersPermutationEntry()
            {
                Parameters = new ParameterCollection();
                ParametersNoShadows = new ParameterCollection();
                DirectLightGroupDatas = new List<LightShaderGroupData>();
                DirectLightGroupsNoShadowDatas = new List<LightShaderGroupData>();
                EnvironmentLightDatas = new List<LightShaderGroupData>();
            }

            public void Clear()
            {
                DirectLightGroupDatas.Clear();
                DirectLightGroupsNoShadowDatas.Clear();
                EnvironmentLightDatas.Clear();
            }

            public readonly List<LightShaderGroupData> DirectLightGroupDatas;

            public readonly List<LightShaderGroupData> DirectLightGroupsNoShadowDatas;

            public readonly List<LightShaderGroupData> EnvironmentLightDatas;

            public readonly ParameterCollection Parameters;

            public readonly ParameterCollection ParametersNoShadows;
        }

        struct RenderModelLights
        {
            public RenderModelLights(LightShaderPermutationEntry lightShadersPermutation, LightParametersPermutationEntry parameters)
            {
                LightShadersPermutation = lightShadersPermutation;
                Parameters = parameters;
            }

            public readonly LightShaderPermutationEntry LightShadersPermutation;

            public readonly LightParametersPermutationEntry Parameters;
        }
    }
}