// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Collections;
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
        private bool isModelComponentRendererSetup;

        private LightKey currentLightKey;

        private readonly Dictionary<LightKey, LightParameters> lightParametersCache;

        private LightProcessor lightProcessor;

        private ShadowMapRenderer shadowMapRenderer;

        private readonly List<KeyValuePair<Type, LightGroupRendererBase>> lightRenderers;

        private FastListStruct<LightForwardShaderEntryKey> lightShaderKeys;

        private readonly Dictionary<ParameterCompositeKey, ParameterKey> compositeKeys;

        private ModelProcessor modelProcessor;

        private CameraComponent sceneCamera;

        private readonly List<LightComponent> visibleLights;

        private readonly List<LightComponent> visibleLightsWithShadows;

        private readonly List<LightEntry> visibleLightsPerModel = new List<LightEntry>();
        private readonly List<KeyValuePair<LightGroupRendererBase, LightComponentCollectionGroup>> activeRenderers;

        private SceneCameraRenderer sceneCameraRenderer;

        private EntityGroupMask sceneCullingMask;

        private readonly Dictionary<ObjectId, ShaderEntry> shaderEntries;
        private readonly Dictionary<ObjectId, ParameterCollectionEntry> lightParameterEntries;

        private ShaderEntry currentModelShadersEntry;

        private ParameterCollectionEntry currentModelShadersParameters;

        private struct ShaderEntry
        {
            public List<ShaderSource> DirectLights;

            public List<ShaderSource> DirectLightsNoShadows;

            public List<ShaderSource> EnvironmentLights;
        }

        private struct ParameterCollectionEntry
        {
            public ParameterCollection Parameters;

            public ParameterCollection ParametersNoShadows;
        }


        /// <summary>
        /// Gets the lights without shadow per light type.
        /// </summary>
        /// <value>The lights.</value>
        private readonly Dictionary<Type, LightComponentCollectionGroup> activeLightGroups;

        public LightComponentForwardRenderer()
        {
            lightParametersCache = new Dictionary<LightKey, LightParameters>();
            //directLightGroup = new LightGroupRenderer("directLightGroups", LightingKeys.DirectLightGroups);
            //environmentLightGroup = new LightGroupRenderer("environmentLights", LightingKeys.EnvironmentLights);
            lightRenderers = new List<KeyValuePair<Type, LightGroupRendererBase>>(16);
            compositeKeys = new Dictionary<ParameterCompositeKey, ParameterKey>();
            lightShaderKeys = new FastListStruct<LightForwardShaderEntryKey>(256);

            visibleLights = new List<LightComponent>();
            visibleLightsWithShadows = new List<LightComponent>();

            shaderEntries = new Dictionary<ObjectId, ShaderEntry>();

            visibleLightsPerModel = new List<LightEntry>(16);
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

            // Reset the curent light key
            currentLightKey = new LightKey((EntityGroup)0xFFFFFFFF, false);

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

            currentModelShadersEntry = new ShaderEntry();
            currentModelShadersParameters = new ParameterCollectionEntry();

            // Process the lights
            // ProcessLights(context, lightProcessor.ActiveLights, shadowMapRenderer.LightComponentsWithShadows);
            // ProcessLights(context, lightProcessor.ActiveEnvironmentLights, shadowMapRenderer.LightComponentsWithShadows);
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
                var directLight = light.Type as IDirectLight;
                if (directLight != null && directLight.HasBoundingBox && !frustum.Contains(ref light.BoundingBoxExt))
                {
                    continue;
                }

                // If light is not part of the culling mask group, we can skip it
                var entityLightMask = (EntityGroupMask)(1 << (int)light.Entity.Group);
                if ((entityLightMask & sceneCullingMask) == 0 && (light.CullingMask & sceneCullingMask) == 0)
                {
                    continue;
                }

                var lightGroup = GetLightGroup(light);
                lightGroup.PrepareLight(light);

                visibleLights.Add(light);

                // Add light to a special list if it has shadows
                if (directLight != null && directLight.Shadow != null && directLight.Shadow.Enabled)
                {
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

            visibleLightsPerModel.Clear();

            var currentShaderKey = new LightForwardShaderEntryKey();

            // Iterate in the order of registered active renderers, to make sure we always calculate a shader key in an uniform way
            foreach (var rendererAndlightGroup in activeRenderers)
            {
                var lightRenderer = rendererAndlightGroup.Key;
                var lightCollection = rendererAndlightGroup.Value.FindGroup(group);

                int lightMaxCount = lightRenderer.LightMaxCount;
                byte lightType = lightRenderer.LightType;
                byte allocCountForNewLightType = lightRenderer.AllocateLightMaxCount ? (byte)lightMaxCount : (byte)1;

                // Iterate on all active lights for this renderer
                int currentLightCount = 0;
                foreach (var light in lightCollection)
                {
                    var directLight = light.Type as IDirectLight;

                    LightShadowMapTexture shadowTexture = null;
                    byte shadowType = 0;
                    byte shadowTextureId = 0;
                    if (directLight != null)
                    {
                        // Check if the light is intersecting the model
                        if (directLight.HasBoundingBox && light.BoundingBox.Intersects(ref modelBoundingBox))
                        {
                            continue;
                        }

                        if (shadowMapRenderer.LightComponentsWithShadows.TryGetValue(light, out shadowTexture))
                        {
                            shadowType = shadowTexture.ShadowType;
                            shadowTextureId = shadowTexture.TextureId;
                        }
                    }

                    if (shaderKeyIdBuilder.Length == 0)
                    {
                        currentShaderKey = new LightForwardShaderEntryKey(lightType, shadowType, allocCountForNewLightType, shadowTextureId);
                        currentLightCount = 1;
                    }
                    else
                    {
                        // We are already at the light max count of the renderer
                        if ((currentLightCount + 1) == lightMaxCount)
                        {
                            continue;
                        }

                        if (currentShaderKey.LightType == lightType && currentShaderKey.ShadowType == shadowType && currentShaderKey.ShadowTextureId == shadowTextureId)
                        {
                            if (!lightRenderer.AllocateLightMaxCount)
                            {
                                currentShaderKey.LightCount++;
                            }
                            currentLightCount++;
                        }
                        else
                        {
                            unsafe
                            {
                                shaderKeyIdBuilder.Write(*(uint*)&currentShaderKey);
                            }

                            currentShaderKey = new LightForwardShaderEntryKey(lightType, shadowType, allocCountForNewLightType, shadowTextureId);
                            currentLightCount = 1;
                        }
                    }

                    parametersKeyIdBuilder.Write(light.Id);
                    visibleLightsPerModel.Add(new LightEntry(lightRenderer, light, shadowTexture));
                }
            }

            var newShaderEntry = new ShaderEntry();
            var newParametersEntry = new ParameterCollectionEntry();
            if (visibleLightsPerModel.Count > 0)
            {
                unsafe
                {
                    shaderKeyIdBuilder.Write(*(uint*)&currentShaderKey);
                }

                ObjectId shaderKeyId;
                ObjectId parametersKeyId;
                shaderKeyIdBuilder.ComputeHash(out shaderKeyId);
                parametersKeyIdBuilder.ComputeHash(out parametersKeyId);

                if (!shaderEntries.TryGetValue(shaderKeyId, out newShaderEntry))
                {
                    newShaderEntry = CalculateShaderEntry();
                    shaderEntries.Add(shaderKeyId, newShaderEntry);
                }

                if (!lightParameterEntries.TryGetValue(parametersKeyId, out newParametersEntry))
                {
                    newParametersEntry = CalculateLightParameters();
                    lightParameterEntries.Add(parametersKeyId, newParametersEntry);
                }
            }

            var currentDirectLights = currentModelShadersEntry.DirectLights;
            if (currentDirectLights != newShaderEntry.DirectLights)
            {
                currentModelShadersEntry.DirectLights = newShaderEntry.DirectLights;
            }

            var currentEnvironmentLights = currentModelShadersEntry.EnvironmentLights;
            if (currentEnvironmentLights != newShaderEntry.EnvironmentLights)
            {
                currentModelShadersEntry.EnvironmentLights = newShaderEntry.EnvironmentLights;
            }

            if (!ReferenceEquals(currentModelShadersParameters.Parameters, newParametersEntry.Parameters))
            {
                currentModelShadersParameters.Parameters = newParametersEntry.Parameters;
            }

            if (!ReferenceEquals(currentModelShadersParameters.ParametersNoShadows, newParametersEntry.ParametersNoShadows))
            {
                currentModelShadersParameters.ParametersNoShadows = newParametersEntry.ParametersNoShadows;
            }
        }

        struct LightEntry
        {
            public LightEntry(LightGroupRendererBase renderer, LightComponent light, LightShadowMapTexture shadow)
            {
                Renderer = renderer;
                Light = light;
                Shadow = shadow;
            }

            public LightGroupRendererBase Renderer;

            public LightComponent Light;

            public LightShadowMapTexture Shadow;
        }

        private ShaderEntry CalculateShaderEntry()
        {
            foreach (var lightAndShadow in visibleLightsPerModel)
            {
                lightAndShadow.Key




            }
        }

        private ParameterCollectionEntry CalculateLightParameters()
        {
            throw new NotImplementedException();
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

        private void ProcessLights(RenderContext context, Dictionary<Type, LightComponentCollectionGroup> activeLights, Dictionary<LightComponent, LightShadowMapTexture> shadows)
        {
            foreach (var lightRenderProcessor in lightRenderers)
            {
                foreach (var lightType in lightRenderProcessor.SupportedLights)
                {
                    LightComponentCollectionGroup lights;
                    if (activeLights.TryGetValue(lightType, out lights))
                    {
                        foreach (var lightCollection in lights)
                        {
                            // TODO: Cache ShaderGenerator per light type
                            var shadowRenderer = shadowMapRenderer.FindRenderer(lightType);
                            var shaderGenerator = lightRenderProcessor.CreateShaderGenerator(lightCollection, shadowRenderer);

                            var result = shaderGenerator.GenerateShaders(context, shadows);
                            // TODO: handle result
                        }
                    }
                }
            }
        }

        private void CollectLightsForGroup(RenderContext context, EntityGroup group)
        {

            
        }

        private class LightParameters
        {
            public LightParameters()
            {
                Parameters = new ParameterCollection();
            }

            public readonly ParameterCollection Parameters;

        }

        private void PreRenderMesh(RenderContext context, RenderMesh renderMesh)
        {
            if (lightProcessor == null)
            {
                return;
            }

            var modelComponent = renderMesh.RenderModel.ModelComponent;
            var isShadowReceiver = renderMesh.RenderModel.ModelComponent.IsShadowReceiver && renderMesh.MaterialInstance.IsShadowReceiver;








            context.Parameters.Set(LightingKeys.DirectLightGroups, newShaderEntry.DirectLights);
            context.Parameters.Set(LightingKeys.EnvironmentLights, newShaderEntry.EnvironmentLights);
            currentModelShadersParameters.CopySharedTo(context.Parameters);








            var newKey = new LightKey(renderMesh.RenderModel.Group, renderMesh.RenderModel.ModelComponent.IsShadowReceiver);
            if (currentLightKey == newKey)
            {
                return;
            }

            LightParameters lightParameters;
            if (!lightParametersCache.TryGetValue(newKey, out lightParameters))
            {
                lightParameters = BuildLightParameters(newKey);
                lightParametersCache.Add(newKey, lightParameters);
            }

            var parameters = context.Parameters;
            lightParameters.Parameters.CopySharedTo(parameters);
        }

        private LightParameters BuildLightParameters(LightKey newKey)
        {
            // TODO: Use a pool
            var lightParameters = new LightParameters();

            foreach (var lightGroup in lightProcessor.ActiveLights)
            {
                var lightComponentCollection = lightGroup.Value.FindGroup(newKey.Group);



            }

            return null;
        }

        private ParameterKey GetComposedKey(ParameterKey key, string compositionName, int lightGroupIndex)
        {
            var compositeKey = new ParameterCompositeKey(key, lightGroupIndex);

            ParameterKey rawCompositeKey;
            if (!compositeKeys.TryGetValue(compositeKey, out rawCompositeKey))
            {
                rawCompositeKey = ParameterKeys.FindByName(string.Format("{0}.{1}[{2}]", key.Name, compositionName, lightGroupIndex));
                compositeKeys.Add(compositeKey, rawCompositeKey);
            }
            return rawCompositeKey;
        }

        private class LightModelRendererCache
        {
            private readonly LightPermutationsEntry[] lights;
            private readonly LightPermutationsEntry[] lightsNoShadows;

            public LightModelRendererCache()
            {
                lights = new LightPermutationsEntry[32];
                lightsNoShadows = new LightPermutationsEntry[32];
                for (int i = 0; i < 32; i++)
                {
                    lights[i] = new LightPermutationsEntry();
                    lightsNoShadows[i] = new LightPermutationsEntry();
                }
            }
        }

        /// <summary>
        /// We expect this class to be 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
        struct LightForwardShaderEntryKey
        {
            public LightForwardShaderEntryKey(byte lightType, byte shadowType, byte lightCount, byte shadowTextureId)
            {
                LightType = lightType;
                ShadowType = shadowType;
                LightCount = lightCount;
                ShadowTextureId = shadowTextureId;
            }

            public byte LightType;

            public byte ShadowType;

            public byte LightCount;

            public byte ShadowTextureId;
        }



        struct LightKey : IEquatable<LightKey>
        {
            public LightKey(EntityGroup group, bool hasShadows)
            {
                Group = group;
                HasShadows = hasShadows;
            }

            public readonly EntityGroup Group;

            public readonly bool HasShadows;

            public bool Equals(LightKey other)
            {
                return Group == other.Group && HasShadows.Equals(other.HasShadows);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is LightKey && Equals((LightKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)Group * 397) ^ HasShadows.GetHashCode();
                }
            }

            public static bool operator ==(LightKey left, LightKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(LightKey left, LightKey right)
            {
                return !left.Equals(right);
            }
        }



        /// <summary>
        /// An internal key to cache {Key,TransformIndex} => CompositeKey
        /// </summary>
        private struct ParameterCompositeKey : IEquatable<ParameterCompositeKey>
        {
            private readonly ParameterKey key;

            private readonly int index;

            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterCompositeKey"/> struct.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="transformIndex">Index of the transform.</param>
            public ParameterCompositeKey(ParameterKey key, int transformIndex)
            {
                if (key == null) throw new ArgumentNullException("key");
                this.key = key;
                index = transformIndex;
            }

            public bool Equals(ParameterCompositeKey other)
            {
                return key.Equals(other.key) && index == other.index;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ParameterCompositeKey && Equals((ParameterCompositeKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (key.GetHashCode() * 397) ^ index;
                }
            }
        }
    }
}