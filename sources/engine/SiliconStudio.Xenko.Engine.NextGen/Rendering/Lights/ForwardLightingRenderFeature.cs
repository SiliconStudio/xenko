using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;
using ILightShadowMapRenderer = SiliconStudio.Xenko.Rendering.Shadows.NextGen.ILightShadowMapRenderer;
using LightDirectionalShadowMapRenderer = SiliconStudio.Xenko.Rendering.Shadows.NextGen.LightDirectionalShadowMapRenderer;
using LightSpotShadowMapRenderer = SiliconStudio.Xenko.Rendering.Shadows.NextGen.LightSpotShadowMapRenderer;
using ShadowMapRenderer = SiliconStudio.Xenko.Rendering.Shadows.NextGen.ShadowMapRenderer;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Compute and upload skinning info.
    /// </summary>
    public class ForwardLightingRenderFeature : SubRenderFeature
    {
        private StaticEffectObjectPropertyKey<RenderEffect> renderEffectKey;

        private EffectDescriptorSetReference perLightingDescriptorSetSlot;

        private const string DirectLightGroupsCompositionName = "directLightGroups";
        private const string EnvironmentLightsCompositionName = "environmentLights";

        private bool isShadowMapRendererSetUp;

        private LightProcessor lightProcessor;

        // Might be null if shadow mapping is not enabled (i.e. graphics device feature level too low)
        private ShadowMapRenderer shadowMapRenderer;

        private readonly List<KeyValuePair<Type, LightGroupRendererBase>> lightRenderers;

        private NextGenModelProcessor modelProcessor;

        private CameraComponent sceneCamera;

        private readonly List<LightComponent> visibleLights;

        private readonly List<LightComponent> visibleLightsWithShadows;

        private readonly List<ActiveLightGroupRenderer> activeRenderers;

        private SceneCameraRenderer sceneCameraRenderer;

        private EntityGroupMask sceneCullingMask;

        private readonly Dictionary<ObjectId, LightShaderPermutationEntry> shaderEntries;
        private readonly Dictionary<ObjectId, LightParametersPermutationEntry> lightParameterEntries;
        private readonly Dictionary<RenderModelLightsKey, RenderModelLights> renderModelLightsCache;

        private readonly List<LightEntry> directLightsPerModel = new List<LightEntry>();
        private FastListStruct<LightForwardShaderFullEntryKey> directLightShaderGroupEntryKeys;

        private readonly List<LightEntry> environmentLightsPerModel = new List<LightEntry>();
        private FastListStruct<LightForwardShaderFullEntryKey> environmentLightShaderGroupEntryKeys;
        private FastListStruct<LightForwardShaderFullEntryKey> directLightShaderGroupEntryKeysNoShadows;

        private PoolListStruct<LightParametersPermutationEntry> parameterCollectionEntryPool;

        private ObjectPropertyKey<RenderModelLights> renderModelObjectInfoKey;

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

        public RenderStage ShadowmapRenderStage { get; set; }

        static ForwardLightingRenderFeature()
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

        public ForwardLightingRenderFeature()
        {
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

            renderModelLightsCache = new Dictionary<RenderModelLightsKey, RenderModelLights>(32);

            // TODO: Make this pluggable
            RegisterLightGroupRenderer(typeof(LightDirectional), new LightDirectionalGroupRenderer());
            RegisterLightGroupRenderer(typeof(LightSpot), new LightSpotGroupRenderer());
            RegisterLightGroupRenderer(typeof(LightPoint), new LightPointGroupRenderer());
            RegisterLightGroupRenderer(typeof(LightAmbient), new LightAmbientRenderer());
            RegisterLightGroupRenderer(typeof(LightSkybox), new LightSkyboxRenderer());
        }

        public override void Initialize()
        {
            base.Initialize();

            renderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;
            renderModelObjectInfoKey = RootRenderFeature.CreateObjectKey<RenderModelLights>();

            perLightingDescriptorSetSlot = ((RootEffectRenderFeature)RootRenderFeature).GetOrCreateEffectDescriptorSetSlot("PerLighting");
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            modelProcessor = SceneInstance.GetCurrent(RenderSystem.RenderContextOld).GetProcessor<NextGenModelProcessor>();
            lightProcessor = SceneInstance.GetCurrent(RenderSystem.RenderContextOld).GetProcessor<LightProcessor>();

            // No light processors means no light in the scene, so we can early exit
            if (lightProcessor == null || modelProcessor == null)
            {
                return;
            }

            // Not in the context of a SceneCameraRenderer? just exit
            sceneCameraRenderer = RenderSystem.RenderContextOld.Tags.Get(SceneCameraRenderer.Current);
            sceneCamera = RenderSystem.RenderContextOld.Tags.Get(CameraComponentRenderer.Current);
            if (sceneCameraRenderer == null || sceneCamera == null)
            {
                return;
            }
            sceneCullingMask = sceneCameraRenderer.CullingMask;

            // Setup the callback on the ModelRenderer and shadow map LightGroupRenderer
            if (!isShadowMapRendererSetUp)
            {
                // TODO: Shadow mapping is currently disabled in new render system
                // TODO: Make this pluggable
                // TODO: Shadows should work on mobile platforms
                if (RenderSystem.RenderContextOld.GraphicsDevice.Features.Profile >= GraphicsProfile.Level_10_0
                    && (Platform.Type == PlatformType.Windows || Platform.Type == PlatformType.WindowsStore || Platform.Type == PlatformType.Windows10))
                {
                    shadowMapRenderer = new ShadowMapRenderer(RenderSystem, ShadowmapRenderStage);
                    shadowMapRenderer.Renderers.Add(typeof(LightDirectional), new LightDirectionalShadowMapRenderer());
                    shadowMapRenderer.Renderers.Add(typeof(LightSpot), new LightSpotShadowMapRenderer());
                }

                isShadowMapRendererSetUp = true;
            }

            // Collect all visible lights
            CollectVisibleLights();

            // Draw shadow maps
            shadowMapRenderer?.Extract(RenderSystem.RenderContextOld, visibleLightsWithShadows);

            // Prepare active renderers in an ordered list (by type and shadow on/off)
            CollectActiveLightRenderers(RenderSystem.RenderContextOld);


            // Clear the cache of parameter entries
            lightParameterEntries.Clear();
            parameterCollectionEntryPool.Clear();
            renderModelLightsCache.Clear();

            // Clear all data generated by shader entries
            foreach (var shaderEntry in shaderEntries)
            {
                shaderEntry.Value.ResetGroupDatas();
            }
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutations()
        {
            var renderObjectInfoData = RootRenderFeature.GetData(renderModelObjectInfoKey);

            // TODO: Old code is working with RenderModel, but we should probably directly work with RenderMesh
            foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;
                PrepareRenderModelForRendering(renderMesh.RenderModel, out renderObjectInfoData.Data[objectNodeReference.Index]);
            }

            var renderEffects = RootRenderFeature.GetData(renderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            foreach (var objectNodeReference in RootRenderFeature.ObjectNodeReferences)
            {
                var objectNode = RootRenderFeature.GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;
                var staticObjectNode = renderMesh.StaticObjectNode;
                var renderObjectInfo = renderObjectInfoData[objectNodeReference];

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode.CreateEffectReference(effectSlotCount, i);
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    RenderModelLights modelLights;
                    if (renderObjectInfo == null)
                        continue;

                    renderEffect.EffectValidator.ValidateParameter(LightingKeys.DirectLightGroups, renderObjectInfo.LightShadersPermutation.DirectLightShaders);
                    renderEffect.EffectValidator.ValidateParameter(LightingKeys.EnvironmentLights, renderObjectInfo.LightShadersPermutation.EnvironmentLightShaders);
                }
            }
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void Prepare(NextGenRenderContext context)
        {
            var renderObjectInfoData = RootRenderFeature.GetData(renderModelObjectInfoKey);

            // Copy data to cbuffer
            // TODO: Rewrite it with new render system in mind (should be much faster/efficient)
            var resourceGroupPool = ((RootEffectRenderFeature)RootRenderFeature).ResourceGroupPool;
            for (int renderNodeIndex = 0; renderNodeIndex < RootRenderFeature.renderNodes.Count; renderNodeIndex++)
            {
                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var renderNode = RootRenderFeature.renderNodes[renderNodeIndex];
                var renderMesh = (RenderMesh)renderNode.RenderObject;
                var renderObjectInfo = renderObjectInfoData[renderNode.RenderObject.ObjectNode];

                if (renderObjectInfo == null)
                    continue;

                if (renderObjectInfo.Info == null)
                {
                    var renderEffect = renderNode.RenderEffect;
                    var modelLightInfos = renderObjectInfo.Info = new RenderModelLightInfo();
                    var descriptorLayout = renderEffect.Reflection.Binder.DescriptorReflection.GetLayout("PerLighting");

                    var parameterCollectionLayout = new NextGenParameterCollectionLayout();
                    parameterCollectionLayout.ProcessResources(descriptorLayout);
                    modelLightInfos.ResourceCount = parameterCollectionLayout.ResourceCount;

                    // First time?
                    // Find lighting cbuffer
                    var lightingConstantBuffer = renderEffect.Effect.Bytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerLighting");

                    // Process cbuffer (if any)
                    if (lightingConstantBuffer != null)
                    {
                        modelLightInfos.ConstantBufferReflection = lightingConstantBuffer;
                        parameterCollectionLayout.ProcessConstantBuffer(lightingConstantBuffer);
                    }

                    modelLightInfos.PerLightingLayout = ResourceGroupLayout.New(RenderSystem.GraphicsDevice, descriptorLayout, renderEffect.Effect.Bytecode, "PerLighting");
                    NextGenParameterCollectionLayoutExtensions.PrepareResourceGroup(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, RenderSystem.BufferPool, modelLightInfos.PerLightingLayout, BufferPoolAllocationType.UsedMultipleTime, modelLightInfos.Resources);

                    // Chose parameter 
                    var isShadowReceiver = renderMesh.Material.IsShadowCaster;
                    var parameters = isShadowReceiver ? renderObjectInfo.Parameters.Parameters : renderObjectInfo.Parameters.ParametersNoShadows;
                    parameters.UpdateLayout(parameterCollectionLayout);

                    // Set resource bindings in PerMaterial resource set
                    for (int resourceSlot = 0; resourceSlot < modelLightInfos.ResourceCount; ++resourceSlot)
                    {
                        modelLightInfos.Resources.DescriptorSet.SetValue(resourceSlot, parameters.ResourceValues[resourceSlot]);
                    }

                    // Process PerMaterial cbuffer
                    if (modelLightInfos.ConstantBufferReflection != null)
                    {
                        var mappedCB = modelLightInfos.Resources.ConstantBuffer.Data;
                        Utilities.CopyMemory(mappedCB, parameters.DataValues, modelLightInfos.Resources.ConstantBuffer.Size);
                    }
                }

                var resourceGroupPoolOffset = ((RootEffectRenderFeature)RootRenderFeature).ComputeResourceGroupOffset(renderNodeReference);
                resourceGroupPool[resourceGroupPoolOffset + perLightingDescriptorSetSlot.Index] = renderObjectInfo.Info.Resources;
            }
        }

        /// <inheritdoc/>
        public override void Draw(NextGenRenderContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
        }

        protected void RegisterLightGroupRenderer(Type lightType, LightGroupRendererBase renderer)
        {
            if (lightType == null) throw new ArgumentNullException("lightType");
            if (renderer == null) throw new ArgumentNullException("renderer");
            lightRenderers.Add(new KeyValuePair<Type, LightGroupRendererBase>(lightType, renderer));
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

        private void PrepareRenderModelForRendering(RenderModel model, out RenderModelLights renderObjectInfo)
        {
            // Already processed?
            //if (modelToLights.ContainsKey(model))
            //    return;

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
                            newShadowRenderer = (ILightShadowMapRenderer)shadowTexture.Renderer;
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

            // Create and cache RenderModelLights for this shader and parameter permutation
            var renderModelLightsKey = new RenderModelLightsKey(newLightShaderPermutationEntry, newShaderEntryParameters);
            RenderModelLights renderModelLights;
            if (!renderModelLightsCache.TryGetValue(renderModelLightsKey, out renderModelLights))
            {
                renderModelLights = new RenderModelLights(newLightShaderPermutationEntry, newShaderEntryParameters);
                renderModelLightsCache.Add(renderModelLightsKey, renderModelLights);
            }

            renderObjectInfo = renderModelLights;
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

            //parameters.Set(LightingKeys.DirectLightGroups, lightShaderPermutationEntry.DirectLightShaders);
            //parameters.Set(LightingKeys.EnvironmentLights, lightShaderPermutationEntry.EnvironmentLightShaders);
            //parametersNoShadows.Set(LightingKeys.DirectLightGroups, lightShaderPermutationEntry.DirectLightShadersNoShadows);
            //parametersNoShadows.Set(LightingKeys.EnvironmentLights, lightShaderPermutationEntry.EnvironmentLightShaders);

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
                Parameters = new NextGenParameterCollection();
                ParametersNoShadows = new NextGenParameterCollection();
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

            public readonly NextGenParameterCollection Parameters;

            public readonly NextGenParameterCollection ParametersNoShadows;
        }

        struct RenderModelLightsKey : IEquatable<RenderModelLightsKey>
        {
            public RenderModelLightsKey(LightShaderPermutationEntry lightShadersPermutation, LightParametersPermutationEntry parameters)
            {
                LightShadersPermutation = lightShadersPermutation;
                Parameters = parameters;
            }

            public readonly LightShaderPermutationEntry LightShadersPermutation;

            public readonly LightParametersPermutationEntry Parameters;

            public bool Equals(RenderModelLightsKey other)
            {
                return LightShadersPermutation.Equals(other.LightShadersPermutation) && Parameters.Equals(other.Parameters);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is RenderModelLightsKey && Equals((RenderModelLightsKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (LightShadersPermutation.GetHashCode() * 397) ^ Parameters.GetHashCode();
                }
            }

            public static bool operator ==(RenderModelLightsKey left, RenderModelLightsKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(RenderModelLightsKey left, RenderModelLightsKey right)
            {
                return !left.Equals(right);
            }
        }

        class RenderModelLights
        {
            public RenderModelLights(LightShaderPermutationEntry lightShadersPermutation, LightParametersPermutationEntry parameters)
            {
                LightShadersPermutation = lightShadersPermutation;
                Parameters = parameters;
                Info = null;
            }

            public readonly LightShaderPermutationEntry LightShadersPermutation;

            public readonly LightParametersPermutationEntry Parameters;

            public RenderModelLightInfo Info;
        }

        class RenderModelLightInfo
        {
            public ResourceGroupLayout PerLightingLayout;
            public ResourceGroup Resources = new ResourceGroup();
            public int ResourceCount;
            public ShaderConstantBufferDescription ConstantBufferReflection;
        }
    }
}