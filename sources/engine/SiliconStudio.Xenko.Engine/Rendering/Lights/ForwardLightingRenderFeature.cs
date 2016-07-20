using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Compute lighting shaders and data.
    /// </summary>
    public class ForwardLightingRenderFeature : SubRenderFeature
    {
        public class RenderViewLightData
        {
            /// <summary>
            /// Gets the lights without shadow per light type.
            /// </summary>
            /// <value>The lights.</value>
            public readonly Dictionary<Type, LightComponentCollectionGroup> ActiveLightGroups;

            internal readonly List<ActiveLightGroupRenderer> ActiveRenderers;

            public readonly List<LightComponent> VisibleLights;

            public readonly List<LightComponent> VisibleLightsWithShadows;

            public readonly Dictionary<LightComponent, LightShadowMapTexture> LightComponentsWithShadows;

            public int ViewIndex;

            internal ObjectId ViewLayoutHash;
            internal ParameterCollectionLayout ViewParameterLayout;
            internal ParameterCollection ViewParameters = new ParameterCollection();

            internal ObjectId DrawLayoutHash;
            internal ParameterCollectionLayout DrawParameterLayout;
            internal ParameterCollection DrawParameters = new ParameterCollection();

            public RenderViewLightData()
            {
                ActiveLightGroups = new Dictionary<Type, LightComponentCollectionGroup>(16);
                ActiveRenderers = new List<ActiveLightGroupRenderer>(16);

                VisibleLights = new List<LightComponent>(1024);
                VisibleLightsWithShadows = new List<LightComponent>(1024);
                LightComponentsWithShadows = new Dictionary<LightComponent, LightShadowMapTexture>(16);
            }
        }

        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        private const string DirectLightGroupsCompositionName = "directLightGroups";
        private const string EnvironmentLightsCompositionName = "environmentLights";

        private LightShaderPermutationEntry ShaderPermutation = new LightShaderPermutationEntry();

        private bool isShadowMapRendererSetUp;

        private LightProcessor lightProcessor;

        // Might be null if shadow mapping is not enabled (i.e. graphics device feature level too low)

        private readonly List<KeyValuePair<Type, LightGroupRendererBase>> lightRenderers;

        private readonly Dictionary<RenderView, RenderViewLightData> renderViewDatas;

        private readonly FastList<RenderView> renderViews = new FastList<RenderView>();

        private readonly Dictionary<ShaderSourceCollection, ShaderSourceCollection> shaderSourcesReadonlyCache = new Dictionary<ShaderSourceCollection, ShaderSourceCollection>();

        private LogicalGroupReference viewLightingKey;
        private LogicalGroupReference drawLightingKey;

        private static readonly string[] DirectLightGroupsCompositionNames;
        private static readonly string[] EnvironmentLightGroupsCompositionNames;

        public ShadowMapRenderer ShadowMapRenderer { get; private set; }

        public RenderStage ShadowMapRenderStage { get; set; }

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
            //directLightGroup = new LightGroupRenderer("directLightGroups", LightingKeys.DirectLightGroups);
            //environmentLightGroup = new LightGroupRenderer("environmentLights", LightingKeys.EnvironmentLights);
            lightRenderers = new List<KeyValuePair<Type, LightGroupRendererBase>>(16);

            renderViewDatas = new Dictionary<RenderView, RenderViewLightData>();
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // TODO: Make this pluggable
            if (Context.GraphicsDevice.Features.RequestedProfile >= GraphicsProfile.Level_10_0)
            {
                // Note: this renderer supports both Point and Spot lights
                var clusteredLightRenderer = new LightClusteredPointGroupRenderer();

                RegisterLightGroupRenderer(typeof(LightPoint), clusteredLightRenderer);
                RegisterLightGroupRenderer(typeof(LightSpot), new LightSpotGroupRenderer { NonShadowRenderer = clusteredLightRenderer.SpotRenderer });
            }
            else
            {
                RegisterLightGroupRenderer(typeof(LightPoint), new LightPointGroupRenderer());
                RegisterLightGroupRenderer(typeof(LightSpot), new LightSpotGroupRenderer());
            }

            RegisterLightGroupRenderer(typeof(LightDirectional), new LightDirectionalGroupRenderer());
            RegisterLightGroupRenderer(typeof(LightAmbient), new LightAmbientRenderer());
            RegisterLightGroupRenderer(typeof(LightSkybox), new LightSkyboxRenderer());

            renderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;

            viewLightingKey = ((RootEffectRenderFeature)RootRenderFeature).CreateViewLogicalGroup("Lighting");
            drawLightingKey = ((RootEffectRenderFeature)RootRenderFeature).CreateDrawLogicalGroup("Lighting");
        }

        public override void Unload()
        {
            // Unload light renderers
            foreach (var lightRenderer in lightRenderers)
            {
                lightRenderer.Value.Unload();
            }

            base.Unload();
        }

        public override void Collect()
        {
            // Initialize shadow map renderer
            if (!isShadowMapRendererSetUp && ShadowMapRenderStage != null)
            {
                // TODO: Shadow mapping is currently disabled in new render system
                // TODO: Make this pluggable
                // TODO: Shadows should work on mobile platforms
                if (RenderSystem.RenderContextOld.GraphicsDevice.Features.RequestedProfile >= GraphicsProfile.Level_10_0)
                {
                    ShadowMapRenderer = new ShadowMapRenderer(RenderSystem, ShadowMapRenderStage);
                    ShadowMapRenderer.Renderers.Add(typeof(LightDirectional), new LightDirectionalShadowMapRenderer());
                    ShadowMapRenderer.Renderers.Add(typeof(LightSpot), new LightSpotShadowMapRenderer());
                }

                isShadowMapRendererSetUp = true;
            }

            // Collect all visible lights
            CollectVisibleLights();

            // Prepare active renderers in an ordered list (by type and shadow on/off)
            CollectActiveLightRenderers(RenderSystem.RenderContextOld);

            // Collect shadow maps
            ShadowMapRenderer?.Collect(RenderSystem.RenderContextOld, renderViewDatas);
        }

        /// <inheritdoc/>
        public override void Extract()
        {
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void PrepareEffectPermutations(RenderDrawContext context)
        {
            var renderEffects = RootRenderFeature.RenderData.GetData(renderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            var shadowMapEffectSlot = ShadowMapRenderStage != null ? ((RootEffectRenderFeature)RootRenderFeature).GetEffectPermutationSlot(ShadowMapRenderStage) : EffectPermutationSlot.Invalid;

            // Counter number of RenderView to process
            renderViews.Clear();
            foreach (var view in RenderSystem.Views)
            {
                // TODO: Use another mechanism to filter light-indepepndent views (such as shadow casting views)
                if (view.GetType() != typeof(RenderView))
                    continue;

                RenderViewLightData renderViewData;
                if (!renderViewDatas.TryGetValue(view, out renderViewData))
                    continue;

                renderViewData.ViewIndex = renderViews.Count;
                renderViews.Add(view);
            }

            // Cleanup light renderers
            foreach (var lightRenderer in lightRenderers)
            {
                lightRenderer.Value.Reset();
                lightRenderer.Value.SetViews(renderViews);
            }

            // Cleanup shader group data
            // TODO: Cleanup end of frame instead of beginning of next one
            ShaderPermutation.Reset();

            foreach (var view in RenderSystem.Views)
            {
                // TODO: Use another mechanism to filter light-indepepndent views (such as shadow casting views)
                if (view.GetType() != typeof(RenderView))
                    continue;

                RenderViewLightData renderViewData;
                if (!renderViewDatas.TryGetValue(view, out renderViewData))
                    continue;

                // Prepare shader permutations
                PrepareLightGroups(context, renderViews, view, renderViewData, ShadowMapRenderer, EntityGroup.Group0);
            }

            // Add light shader groups using lightRenderers order to make sure we generate same shaders independently of light order
            foreach (var lightRenderer in lightRenderers)
            {
                lightRenderer.Value.UpdateShaderPermutationEntry(ShaderPermutation);
            }

            // TODO: Try to run that only if really required (i.e. actual layout change)
            // Notify light groups layout changed and generate shader permutation
            for (int index = 0; index < ShaderPermutation.DirectLightGroups.Count; index++)
            {
                var directLightGroup = ShaderPermutation.DirectLightGroups[index];
                directLightGroup.UpdateLayout(DirectLightGroupsCompositionNames[index]);

                // Generate shader permutation
                ShaderPermutation.DirectLightShaders.Add(directLightGroup.ShaderSource);
                if (directLightGroup.HasEffectPermutations)
                    ShaderPermutation.PermutationLightGroups.Add(directLightGroup);
            }
            for (int index = 0; index < ShaderPermutation.EnvironmentLights.Count; index++)
            {
                var environmentLight = ShaderPermutation.EnvironmentLights[index];
                environmentLight.UpdateLayout(EnvironmentLightGroupsCompositionNames[index]);

                // Generate shader permutation
                ShaderPermutation.EnvironmentLightShaders.Add(environmentLight.ShaderSource);
                if (environmentLight.HasEffectPermutations)
                    ShaderPermutation.PermutationLightGroups.Add(environmentLight);
            }

            // Make copy so that we can continue to mutate the ShaderPermutation ShaderSourceCollection during next frame
            var directLightShaders = GetReadonlyShaderSources(ShaderPermutation.DirectLightShaders);
            var environmentLightShaders = GetReadonlyShaderSources(ShaderPermutation.EnvironmentLightShaders);

            foreach (var renderObject in RootRenderFeature.RenderObjects)
            {
                var renderMesh = (RenderMesh)renderObject;

                if (!renderMesh.Material.IsLightDependent)
                    continue;

                var staticObjectNode = renderMesh.StaticObjectNode;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    // Don't apply lighting for shadow casters
                    if (i == shadowMapEffectSlot.Index)
                        continue;

                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    renderEffect.EffectValidator.ValidateParameter(LightingKeys.DirectLightGroups, directLightShaders);
                    renderEffect.EffectValidator.ValidateParameter(LightingKeys.EnvironmentLights, environmentLightShaders);

                    // Some light groups have additional effect permutation
                    foreach (var lightGroup in ShaderPermutation.PermutationLightGroups)
                        lightGroup.ApplyEffectPermutations(renderEffect);
                }
            }
        }

        /// <summary>
        /// Create a read-only copy of the given shader sources.
        /// </summary>
        /// <param name="shaderSources"></param>
        /// <returns></returns>
        private ShaderSourceCollection GetReadonlyShaderSources(ShaderSourceCollection shaderSources)
        {
            ShaderSourceCollection directLightShaders;
            if (!shaderSourcesReadonlyCache.TryGetValue(shaderSources, out directLightShaders))
            {
                shaderSourcesReadonlyCache.Add(shaderSources, directLightShaders = new ShaderSourceCollection(shaderSources));
            }
            return directLightShaders;
        }

        /// <inheritdoc/>
        public override void Prepare(RenderDrawContext context)
        {
            //var renderViewObjectInfoData = RootRenderFeature.RenderData.GetData(renderViewObjectInfoKey);
            foreach (var view in RenderSystem.Views)
            {
                var viewFeature = view.Features[RootRenderFeature.Index];

                RenderViewLightData renderViewData;
                if (!renderViewDatas.TryGetValue(view, out renderViewData) || viewFeature.Layouts.Count == 0)
                    continue;

                // Find a PerView layout from an effect in normal state
                ViewResourceGroupLayout firstViewLayout = null;
                foreach (var viewLayout in viewFeature.Layouts)
                {
                    // Only process view layouts in normal state
                    if (viewLayout.State != RenderEffectState.Normal)
                        continue;

                    var viewLighting = viewLayout.GetLogicalGroup(viewLightingKey);
                    if (viewLighting.Hash != ObjectId.Empty)
                    {
                        firstViewLayout = viewLayout;
                        break;
                    }
                }

                // Nothing found for this view (no effects in normal state)
                if (firstViewLayout == null)
                    continue;

                var viewParameterLayout = renderViewData.ViewParameterLayout;
                var viewParameters = renderViewData.ViewParameters;
                var firstViewLighting = firstViewLayout.GetLogicalGroup(viewLightingKey);

                // Prepare layout (should be similar for all PerView)
                if (firstViewLighting.Hash != renderViewData.ViewLayoutHash)
                {
                    renderViewData.ViewLayoutHash = firstViewLighting.Hash;

                    // Generate layout
                    viewParameterLayout = renderViewData.ViewParameterLayout = new ParameterCollectionLayout();
                    viewParameterLayout.ProcessLogicalGroup(firstViewLayout, ref firstViewLighting);

                    viewParameters.UpdateLayout(viewParameterLayout);
                }

                // Compute PerView lighting
                foreach (var directLightGroup in ShaderPermutation.DirectLightGroups)
                {
                    directLightGroup.ApplyViewParameters(context, renderViewData.ViewIndex, viewParameters);
                }
                foreach (var environmentLight in ShaderPermutation.EnvironmentLights)
                {
                    environmentLight.ApplyViewParameters(context, renderViewData.ViewIndex, viewParameters);
                }

                // Update PerView
                foreach (var viewLayout in viewFeature.Layouts)
                {
                    // Only process view layouts in normal state
                    if (viewLayout.State != RenderEffectState.Normal)
                        continue;

                    var viewLighting = viewLayout.GetLogicalGroup(viewLightingKey);
                    if (viewLighting.Hash == ObjectId.Empty)
                        continue;

                    Debug.Assert(viewLighting.Hash == firstViewLighting.Hash, "PerView Lighting layout differs between different RenderObject in the same RenderView");

                    var resourceGroup = viewLayout.Entries[view.Index].Resources;

                    // Update resources
                    resourceGroup.UpdateLogicalGroup(ref viewLighting, viewParameters);
                }

                // PerDraw
                var drawParameters = renderViewData.DrawParameters;

                foreach (var renderNodeReference in viewFeature.RenderNodes)
                {
                    var renderNode = RootRenderFeature.GetRenderNode(renderNodeReference);

                    // Ignore fallback effects
                    if (renderNode.RenderEffect.State != RenderEffectState.Normal)
                        continue;

                    var drawLayout = renderNode.RenderEffect.Reflection.PerDrawLayout;
                    if (drawLayout == null)
                        continue;

                    var drawLighting = drawLayout.GetLogicalGroup(drawLightingKey);
                    if (drawLighting.Hash == ObjectId.Empty)
                        continue;

                    // First time, let's build layout
                    if (drawLighting.Hash != renderViewData.DrawLayoutHash)
                    {
                        renderViewData.DrawLayoutHash = drawLighting.Hash;

                        // Generate layout
                        var drawParameterLayout = new ParameterCollectionLayout();
                        drawParameterLayout.ProcessLogicalGroup(drawLayout, ref drawLighting);

                        drawParameters.UpdateLayout(drawParameterLayout);
                    }

                    Debug.Assert(drawLighting.Hash == renderViewData.DrawLayoutHash, "PerDraw Lighting layout differs between different RenderObject in the same RenderView");

                    // Compute PerDraw lighting
                    foreach (var directLightGroup in ShaderPermutation.DirectLightGroups)
                    {
                        directLightGroup.ApplyDrawParameters(context, renderViewData.ViewIndex, drawParameters, ref renderNode.RenderObject.BoundingBox);
                    }
                    foreach (var environmentLight in ShaderPermutation.EnvironmentLights)
                    {
                        environmentLight.ApplyDrawParameters(context, renderViewData.ViewIndex, drawParameters, ref renderNode.RenderObject.BoundingBox);
                    }

                    // Update resources
                    renderNode.Resources.UpdateLogicalGroup(ref drawLighting, drawParameters);
                }
            }
        }

        protected void RegisterLightGroupRenderer(Type lightType, LightGroupRendererBase renderer)
        {
            if (lightType == null) throw new ArgumentNullException("lightType");
            if (renderer == null) throw new ArgumentNullException("renderer");
            lightRenderers.Add(new KeyValuePair<Type, LightGroupRendererBase>(lightType, renderer));
            renderer.Initialize(Context);
        }

        private void CollectActiveLightRenderers(RenderContext context)
        {
            foreach (var renderViewData in renderViewDatas)
            {
                var viewData = renderViewData.Value;
                viewData.ActiveRenderers.Clear();

                foreach (var lightTypeAndRenderer in lightRenderers)
                {
                    LightComponentCollectionGroup lightGroup;
                    viewData.ActiveLightGroups.TryGetValue(lightTypeAndRenderer.Key, out lightGroup);

                    var renderer = lightTypeAndRenderer.Value;
                    if (lightGroup != null && lightGroup.Count > 0)
                    {
                        viewData.ActiveRenderers.Add(new ActiveLightGroupRenderer(renderer, lightGroup));
                    }
                }
            }
        }

        /// <summary>
        /// Collects the visible lights by intersecting them with the frustum.
        /// </summary>
        private void CollectVisibleLights()
        {
            foreach (var renderView in RenderSystem.Views)
            {
                if (renderView.GetType() != typeof(RenderView))
                    continue;

                lightProcessor = renderView.SceneInstance.GetProcessor<LightProcessor>();

                // No light processors means no light in the scene, so we can early exit
                if (lightProcessor == null)
                    continue;

                RenderViewLightData renderViewLightData;
                if (!renderViewDatas.TryGetValue(renderView, out renderViewLightData))
                {
                    renderViewLightData = new RenderViewLightData();
                    renderViewDatas.Add(renderView, renderViewLightData);
                }
                else
                {
                    // 1) Clear the cache of current lights (without destroying collections but keeping previously allocated ones)
                    ClearCache(renderViewLightData.ActiveLightGroups);
                }

                renderViewLightData.VisibleLights.Clear();
                renderViewLightData.VisibleLightsWithShadows.Clear();

                // TODO GRAPHICS REFACTOR
                var sceneCullingMask = renderView.CullingMask;

                // 2) Cull lights with the frustum
                var frustum = renderView.Frustum;
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
                    var lightGroup = GetLightGroup(renderViewLightData, light);
                    lightGroup.PrepareLight(light);

                    // This is a visible light
                    renderViewLightData.VisibleLights.Add(light);

                    // Add light to a special list if it has shadows
                    if (directLight != null && directLight.Shadow.Enabled && ShadowMapRenderer != null)
                    {
                        // A visible light with shadows
                        renderViewLightData.VisibleLightsWithShadows.Add(light);
                    }
                }

                // 3) Allocate collection based on their culling mask
                AllocateCollectionsPerGroupOfCullingMask(renderViewLightData.ActiveLightGroups);

                // 4) Collect lights to the correct light collection group
                foreach (var light in renderViewLightData.VisibleLights)
                {
                    var lightGroup = GetLightGroup(renderViewLightData, light);
                    lightGroup.AddLight(light);
                }
            }
        }

        private static void PrepareLightGroups(RenderDrawContext context, FastList<RenderView> renderViews, RenderView renderView, RenderViewLightData renderViewData, ShadowMapRenderer shadowMapRenderer, EntityGroup group)
        {
            foreach (var activeRenderer in renderViewData.ActiveRenderers)
            {
                // Find lights
                var lightRenderer = activeRenderer.LightRenderer;
                var lightCollection = activeRenderer.LightGroup.FindLightCollectionByGroup(group);

                var processLightsParameters = new LightGroupRendererBase.ProcessLightsParameters
                {
                    Context = context,
                    ViewIndex = renderViewData.ViewIndex,
                    View = renderView,
                    Views = renderViews,
                    LightCollection = lightCollection,
                    LightType = activeRenderer.LightGroup.LightType,
                    LightStart = 0,
                    LightEnd = lightCollection.Count,
                    ShadowMapRenderer = shadowMapRenderer,
                    ShadowMapTexturesPerLight = renderViewData.LightComponentsWithShadows,
                };

                lightRenderer.ProcessLights(processLightsParameters);
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

        private LightComponentCollectionGroup GetLightGroup(RenderViewLightData renderViewData, LightComponent light)
        {
            LightComponentCollectionGroup lightGroup;

            var directLight = light.Type as IDirectLight;
            var lightGroups = renderViewData.ActiveLightGroups;

            var type = light.Type.GetType();
            if (!lightGroups.TryGetValue(type, out lightGroup))
            {
                lightGroup = new LightComponentCollectionGroup(type);
                lightGroups.Add(type, lightGroup);
            }
            return lightGroup;
        }

        public class LightShaderPermutationEntry
        {
            public LightShaderPermutationEntry()
            {
                DirectLightGroups = new FastListStruct<LightShaderGroup>(8);
                EnvironmentLights = new FastListStruct<LightShaderGroup>(8);

                PermutationLightGroups = new FastListStruct<LightShaderGroup>(2);

                DirectLightShaders = new ShaderSourceCollection();
                EnvironmentLightShaders = new ShaderSourceCollection();
            }

            public void Reset()
            {
                DirectLightGroups.Clear();
                DirectLightShaders.Clear();

                EnvironmentLights.Clear();
                EnvironmentLightShaders.Clear();

                PermutationLightGroups.Clear();
            }

            public FastListStruct<LightShaderGroup> DirectLightGroups;

            public readonly ShaderSourceCollection DirectLightShaders;

            public FastListStruct<LightShaderGroup> EnvironmentLights;

            public readonly ShaderSourceCollection EnvironmentLightShaders;

            /// <summary>
            /// Light groups that have <see cref="LightShaderGroup.HasEffectPermutations"/>.
            /// </summary>
            public FastListStruct<LightShaderGroup> PermutationLightGroups;
        }

        internal struct ActiveLightGroupRenderer
        {
            public ActiveLightGroupRenderer(LightGroupRendererBase lightRenderer, LightComponentCollectionGroup lightGroup)
            {
                LightRenderer = lightRenderer;
                LightGroup = lightGroup;
            }

            public readonly LightGroupRendererBase LightRenderer;

            public readonly LightComponentCollectionGroup LightGroup;
        }
    }
}
