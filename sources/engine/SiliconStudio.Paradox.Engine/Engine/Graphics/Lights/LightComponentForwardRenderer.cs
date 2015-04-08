// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Effects.Shadows;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
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

        private ShadowMapRenderer shadowMapRenderer;

        private readonly List<KeyValuePair<Type, LightGroupRendererBase>> lightRenderers;

        private ModelProcessor modelProcessor;

        private CameraComponent sceneCamera;

        private readonly List<LightComponent> visibleLights;

        private readonly List<LightComponent> visibleLightsWithShadows;

        private readonly List<KeyValuePair<LightGroupRendererBase, LightComponentCollectionGroup>> activeRenderers;

        private SceneCameraRenderer sceneCameraRenderer;

        private EntityGroupMask sceneCullingMask;

        private readonly Dictionary<ObjectId, ShaderEntry> shaderEntries;
        private readonly Dictionary<ObjectId, ParameterCollectionEntry> lightParameterEntries;

        private readonly List<LightEntry> directLightsPerModel = new List<LightEntry>();
        private FastListStruct<LightForwardShaderFullEntryKey> directLightShaderGroupEntryKeys;

        private readonly List<LightEntry> environmentLightsPerModel = new List<LightEntry>();
        private FastListStruct<LightForwardShaderFullEntryKey> environmentLightShaderGroupEntryKeys;
        private FastListStruct<LightForwardShaderFullEntryKey> directLightShaderGroupEntryKeysNoShadows;

        private PoolListStruct<ParameterCollectionEntry> parameterCollectionEntryPool;

        private ShaderEntry currentModelShadersEntry;

        private ParameterCollectionEntry currentModelShadersParameters;

        private bool currentModelShadersEntryChanged;
        private bool currentModelShadersParametersChanged;

        /// <summary>
        /// Gets the lights without shadow per light type.
        /// </summary>
        /// <value>The lights.</value>
        private readonly Dictionary<Type, LightComponentCollectionGroup> activeLightGroups;

        public LightComponentForwardRenderer()
        {
            directLightShaderGroupEntryKeys = new FastListStruct<LightForwardShaderFullEntryKey>(32);
            environmentLightShaderGroupEntryKeys = new FastListStruct<LightForwardShaderFullEntryKey>(32);
            directLightShaderGroupEntryKeysNoShadows = new FastListStruct<LightForwardShaderFullEntryKey>(32);
            parameterCollectionEntryPool = new PoolListStruct<ParameterCollectionEntry>(16, CreateParameterCollectionEntry);

            //directLightGroup = new LightGroupRenderer("directLightGroups", LightingKeys.DirectLightGroups);
            //environmentLightGroup = new LightGroupRenderer("environmentLights", LightingKeys.EnvironmentLights);
            lightRenderers = new List<KeyValuePair<Type, LightGroupRendererBase>>(16);

            visibleLights = new List<LightComponent>();
            visibleLightsWithShadows = new List<LightComponent>();

            shaderEntries = new Dictionary<ObjectId, ShaderEntry>();

            directLightsPerModel = new List<LightEntry>(16);
            activeLightGroups = new Dictionary<Type, LightComponentCollectionGroup>();
            activeRenderers = new List<KeyValuePair<LightGroupRendererBase, LightComponentCollectionGroup>>();

            lightParameterEntries = new Dictionary<ObjectId, ParameterCollectionEntry>();

            // TODO: Make this pluggable
            RegisterLightGroupRenderer(typeof(LightDirectional), new LightDirectionalGroupRenderer());
            RegisterLightGroupRenderer(typeof(LightSkybox), new LightSkyboxRenderer());
            RegisterLightGroupRenderer(typeof(LightAmbient), new LightAmbientRenderer());
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

            // Setup the callback on the ModelRenderer and shadow map renderer
            if (!isModelComponentRendererSetup)
            {
                var modelRenderer = ModelComponentRenderer.GetAttached(sceneCameraRenderer);
                if (modelRenderer == null)
                {
                    return;
                }

                modelRenderer.Callbacks.PreRenderModel += PrepareRenderModelForRendering;
                modelRenderer.Callbacks.PreRenderMesh += PreRenderMesh;
                shadowMapRenderer = new ShadowMapRenderer(modelRenderer.EffectName);
                isModelComponentRendererSetup = true;
            }

            // Collect all visible lights
            CollectVisibleLights();

            // Draw shadow maps
            shadowMapRenderer.Draw(context, visibleLightsWithShadows);

            // Prepare active renderers
            activeRenderers.Clear();
            foreach (var lightTypeAndRenderer in lightRenderers)
            {
                LightComponentCollectionGroup lightGroup;
                if (!activeLightGroups.TryGetValue(lightTypeAndRenderer.Key, out lightGroup) || lightGroup.Count == 0)
                {
                    continue;
                }

                var renderer = lightTypeAndRenderer.Value;
                renderer.Initialize(context);

                activeRenderers.Add(new KeyValuePair<LightGroupRendererBase, LightComponentCollectionGroup>(renderer, lightGroup));
            }

            currentModelShadersEntry = null;
            currentModelShadersEntryChanged = true;

            currentModelShadersParameters = null;
            currentModelShadersParametersChanged = true;

            // Clear the cache of parameter entries
            lightParameterEntries.Clear();
            parameterCollectionEntryPool.Clear();
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
                if (directLight != null && directLight.Shadow != null && directLight.Shadow.Enabled)
                {
                    // A visible light with shadows
                    visibleLightsWithShadows.Add(light);
                }
            }

            // 3) Allocate collection based on their culling mask
            AllocateCollectionsPerGroupOfCullingMask(activeLightGroups);

            // 4) Collect lights to the correct light collection group
            foreach (var light in visibleLights)
            {
                var lightGroup = GetLightGroup(light);
                lightGroup.AddLight(light);
            }
        }

        private void PrepareRenderModelForRendering(RenderContext context, RenderModel model)
        {
            var shaderKeyIdBuilder = new ObjectIdSimpleBuilder();
            var parametersKeyIdBuilder = new ObjectIdSimpleBuilder();
            //var idBuilder = new ObjectIdBuilder();

            var group = model.Group;
            var modelComponent = model.ModelComponent;
            var modelBoundingBox = modelComponent.BoundingBox;

            directLightsPerModel.Clear();
            directLightShaderGroupEntryKeys.Clear();
            directLightShaderGroupEntryKeysNoShadows.Clear();

            environmentLightsPerModel.Clear();
            environmentLightShaderGroupEntryKeys.Clear();

            // Iterate in the order of registered active renderers, to make sure we always calculate a shader key in an uniform way
            foreach (var rendererAndlightGroup in activeRenderers)
            {
                var lightRenderer = rendererAndlightGroup.Key;
                var lightCollection = rendererAndlightGroup.Value.FindGroup(group);

                int lightMaxCount = lightRenderer.LightMaxCount;
                var lightRendererId = lightRenderer.LightRendererId;
                var allocCountForNewLightType = lightRenderer.AllocateLightMaxCount ? (byte)lightMaxCount : (byte)1;

                var currentShaderKey = new LightForwardShaderEntryKey();

                // Path for environment lights
                if (lightRenderer.IsEnvironmentLight)
                {
                    // The loop is simpler for environment lights (single group per light, no shadow maps, no bounding box...etc)
                    foreach (var light in lightCollection)
                    {
                        currentShaderKey = new LightForwardShaderEntryKey(lightRendererId, 0, allocCountForNewLightType, 0);
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
                    // direct lights
                    int directLightCount = 0;
                    ILightShadowMapRenderer shadowRenderer = null;

                    foreach (var light in lightCollection)
                    {
                        var directLight = (IDirectLight)light.Type;
                        // If the light does not intersects the model, we can skip it
                        if (directLight.HasBoundingBox && !light.BoundingBox.Intersects(ref modelBoundingBox))
                        {
                            continue;
                        }

                        LightShadowMapTexture shadowTexture;
                        shadowRenderer = null;
                        LightShadowType shadowType = 0;
                        byte shadowTextureId = 0; 
                        
                        if (shadowMapRenderer.LightComponentsWithShadows.TryGetValue(light, out shadowTexture))
                        {
                            shadowType = shadowTexture.ShadowType;
                            shadowTextureId = shadowTexture.TextureId;
                            shadowRenderer = shadowTexture.Renderer;
                        }

                        if (directLightCount == 0)
                        {
                            currentShaderKey = new LightForwardShaderEntryKey(lightRendererId, shadowType, allocCountForNewLightType, shadowTextureId);
                        }
                        else
                        {
                            // We are already at the light max count of the renderer
                            if ((directLightCount + 1) == lightMaxCount)
                            {
                                continue;
                            }

                            if (currentShaderKey.LightRendererId == lightRendererId && currentShaderKey.ShadowType == shadowType && currentShaderKey.ShadowTextureId == shadowTextureId)
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

                                directLightShaderGroupEntryKeys.Add(new LightForwardShaderFullEntryKey(currentShaderKey, lightRenderer, shadowRenderer));
                                currentShaderKey = new LightForwardShaderEntryKey(lightRendererId, shadowType, allocCountForNewLightType, shadowTextureId);
                            }
                        }

                        directLightCount++;

                        parametersKeyIdBuilder.Write(light.Id);
                        directLightsPerModel.Add(new LightEntry(directLightShaderGroupEntryKeys.Count, directLightShaderGroupEntryKeysNoShadows.Count, light, shadowTexture));
                    }

                    if (directLightCount > 0)
                    {
                        directLightShaderGroupEntryKeysNoShadows.Add(new LightForwardShaderFullEntryKey(new LightForwardShaderEntryKey(lightRendererId, 0, (byte)directLightCount, 0), lightRenderer, null));

                        unsafe
                        {
                            shaderKeyIdBuilder.Write(*(uint*)&currentShaderKey);
                        }
                        directLightShaderGroupEntryKeys.Add(new LightForwardShaderFullEntryKey(currentShaderKey, lightRenderer, shadowRenderer));
                    }
                }
            }

            // If we have lights, find or create an existing shaders/parameters permutation
            if (environmentLightsPerModel.Count > 0 || directLightsPerModel.Count > 0)
            {
                // Build the keys for Shaders and Parameters permutations
                ObjectId shaderKeyId;
                ObjectId parametersKeyId;
                shaderKeyIdBuilder.ComputeHash(out shaderKeyId);
                parametersKeyIdBuilder.ComputeHash(out parametersKeyId);

                var previousModelShadersEntry = currentModelShadersEntry;

                // Calculate the shader parameters just once
                // If we don't have already this permutation, use it
                if (!shaderEntries.TryGetValue(shaderKeyId, out currentModelShadersEntry))
                {
                    currentModelShadersEntry = CalculateShaderEntry();
                    shaderEntries.Add(shaderKeyId, currentModelShadersEntry);
                }
                currentModelShadersEntryChanged = previousModelShadersEntry != currentModelShadersEntry;

                // Calculate the shader parameters just once per light combination and for this rendering pass
                var previousModelShadersParameters = currentModelShadersParameters;
                if (!lightParameterEntries.TryGetValue(parametersKeyId, out currentModelShadersParameters))
                {
                    currentModelShadersParameters = parameterCollectionEntryPool.Add();
                    
                    // TODO: Should we clear the parameters?
                    // currentModelShadersParameters.Parameters.Clear();
                    // currentModelShadersParameters.ParametersNoShadows.Clear();

                    UpdateLightParameters(currentModelShadersParameters);
                    lightParameterEntries.Add(parametersKeyId, currentModelShadersParameters);
                }
                currentModelShadersParametersChanged = previousModelShadersParameters != currentModelShadersParameters;
            }
        }

        private ShaderEntry CalculateShaderEntry()
        {
            var shaderEntry = new ShaderEntry();

            // Direct Lights (with or without shadows)
            for (int i = 0; i < directLightShaderGroupEntryKeys.Count; i++)
            {
                var shaderGroupEntry = directLightShaderGroupEntryKeys.Items[i];
                int lightCount = shaderGroupEntry.Key.LightCount;

                ILightShadowMapShaderGroupData shadowGroupData = null;
                if (shaderGroupEntry.Shadow != null)
                {
                    // TODO: Cache ShaderGroupData
                    shadowGroupData = shaderGroupEntry.Shadow.CreateShaderGroupData(DirectLightGroupsCompositionName, i, shaderGroupEntry.Key.ShadowType, lightCount);
                }
                // TODO: Cache LightShaderGroup
                var lightShaderGroup = shaderGroupEntry.Renderer.CreateLightShaderGroup(DirectLightGroupsCompositionName, i, lightCount, shadowGroupData);

                shaderEntry.DirectLightGroups.Add(lightShaderGroup);
                shaderEntry.DirectLightShaders.Add(lightShaderGroup.ShaderSource);
            }

            // All Direct Lights
            for (int i = 0; i < directLightShaderGroupEntryKeysNoShadows.Count; i++)
            {
                var shaderGroupEntry = directLightShaderGroupEntryKeysNoShadows.Items[i];

                // TODO: Cache LightShaderGroup
                var lightShaderGroup = shaderGroupEntry.Renderer.CreateLightShaderGroup(DirectLightGroupsCompositionName, i, shaderGroupEntry.Key.LightCount, null);

                shaderEntry.DirectLightGroupsNoShadows.Add(lightShaderGroup);
                shaderEntry.DirectLightShadersNoShadows.Add(lightShaderGroup.ShaderSource);
            }

            // All Environment lights
            for (int i = 0; i < environmentLightShaderGroupEntryKeys.Count; i++)
            {
                var shaderGroupEntry = environmentLightShaderGroupEntryKeys.Items[i];

                // TODO: Cache LightShaderGroup
                var lightShaderGroup = shaderGroupEntry.Renderer.CreateLightShaderGroup(EnvironmentLightsCompositionName, i, shaderGroupEntry.Key.LightCount, null);

                shaderEntry.EnvironmentLights.Add(lightShaderGroup);
                shaderEntry.EnvironmentLightShaders.Add(lightShaderGroup.ShaderSource);
            }

            return shaderEntry;
        }

        private void UpdateLightParameters(ParameterCollectionEntry parameterCollectionEntry)
        {
            var directLightGroups = currentModelShadersEntry.DirectLightGroups;
            var directLightGroupsNoShadows = currentModelShadersEntry.DirectLightGroupsNoShadows;
            var environmentLights = currentModelShadersEntry.EnvironmentLights;

            var parameters = parameterCollectionEntry.Parameters;
            var parametersNoShadows = parameterCollectionEntry.ParametersNoShadows;

            if (parameters.Get(LightingKeys.DirectLightGroups) != currentModelShadersEntry.DirectLightShaders)
                parameters.Set(LightingKeys.DirectLightGroups, currentModelShadersEntry.DirectLightShaders);

            if (parameters.Get(LightingKeys.EnvironmentLights) != currentModelShadersEntry.EnvironmentLightShaders)
                parameters.Set(LightingKeys.EnvironmentLights, currentModelShadersEntry.EnvironmentLightShaders);

            if (parametersNoShadows.Get(LightingKeys.DirectLightGroups) != currentModelShadersEntry.DirectLightShadersNoShadows)
                parametersNoShadows.Set(LightingKeys.DirectLightGroups, currentModelShadersEntry.DirectLightShadersNoShadows);

            if (parametersNoShadows.Get(LightingKeys.EnvironmentLights) != currentModelShadersEntry.EnvironmentLightShaders)
                parametersNoShadows.Set(LightingKeys.EnvironmentLights, currentModelShadersEntry.EnvironmentLightShaders);

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
            var type = light.Type.GetType();
            if (!activeLightGroups.TryGetValue(type, out lightGroup))
            {
                lightGroup = new LightComponentCollectionGroup();
                activeLightGroups.Add(type, lightGroup);
            }
            return lightGroup;
        }

        private void PreRenderMesh(RenderContext context, RenderMesh renderMesh)
        {
            if (currentModelShadersEntryChanged || currentModelShadersParametersChanged)
            {
                var contextParameters = context.Parameters;
                if (currentModelShadersEntry == null)
                {
                    contextParameters.Set(LightingKeys.DirectLightGroups, null);
                    contextParameters.Set(LightingKeys.EnvironmentLights, null);
                }
                else
                {
                    var isShadowReceiver = renderMesh.RenderModel.ModelComponent.IsShadowReceiver && renderMesh.MaterialInstance.IsShadowReceiver;
                    if (isShadowReceiver)
                    {
                        currentModelShadersParameters.Parameters.CopySharedTo(contextParameters);
                    }
                    else
                    {
                        currentModelShadersParameters.ParametersNoShadows.CopySharedTo(contextParameters);
                    }
                }
            }
        }

        private static ParameterCollectionEntry CreateParameterCollectionEntry()
        {
            return new ParameterCollectionEntry();
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
            public LightForwardShaderEntryKey(byte lightRendererId, LightShadowType shadowType, byte lightCount, byte shadowTextureId)
            {
                LightRendererId = lightRendererId;
                ShadowType = shadowType;
                LightCount = lightCount;
                ShadowTextureId = shadowTextureId;
            }

            public readonly byte LightRendererId;

            public readonly LightShadowType ShadowType;

            public byte LightCount;

            public readonly byte ShadowTextureId;
        }

        private struct LightForwardShaderFullEntryKey
        {
            public LightForwardShaderFullEntryKey(LightForwardShaderEntryKey key, LightGroupRendererBase renderer, ILightShadowMapRenderer shadowRenderer)
            {
                Key = key;
                Renderer = renderer;
                Shadow = shadowRenderer;
            }

            public readonly LightForwardShaderEntryKey Key;

            public readonly LightGroupRendererBase Renderer;

            public readonly ILightShadowMapRenderer Shadow;
        }

        private class ShaderEntry
        {
            public ShaderEntry()
            {
                DirectLightGroups = new List<LightShaderGroup>();
                DirectLightGroupsNoShadows = new List<LightShaderGroup>();
                DirectLightShadersNoShadows = new List<ShaderSource>();
                EnvironmentLights = new List<LightShaderGroup>();

                DirectLightShaders = new List<ShaderSource>();
                DirectLightGroupsNoShadows = new List<LightShaderGroup>();
                EnvironmentLightShaders = new List<ShaderSource>();
            }

            public readonly List<LightShaderGroup> DirectLightGroups;

            public readonly List<ShaderSource> DirectLightShaders;

            public readonly List<LightShaderGroup> DirectLightGroupsNoShadows;

            public readonly List<ShaderSource> DirectLightShadersNoShadows;

            public readonly List<LightShaderGroup> EnvironmentLights;

            public readonly List<ShaderSource> EnvironmentLightShaders;
        }

        private class ParameterCollectionEntry
        {
            public ParameterCollectionEntry()
            {
                Parameters = new ParameterCollection();
                ParametersNoShadows = new ParameterCollection();
            }

            public readonly ParameterCollection Parameters;

            public readonly ParameterCollection ParametersNoShadows;
        }
    }
}