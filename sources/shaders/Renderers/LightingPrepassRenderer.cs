// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Effects.ShadowMaps;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Effects.Renderers
{
    public class LightingPrepassRenderer : Renderer
    {
        #region Static members

        /// <summary>
        /// The key linking the parameters of each point light.
        /// </summary>
        public static readonly ParameterKey<PointLightData[]> PointLightInfos = ParameterKeys.New(new PointLightData[LightingKeys.MaxDeferredPointLights]);

        /// <summary>
        /// The key linking the parameters of each directional light.
        /// </summary>
        public static readonly ParameterKey<DirectLightData[]> DirectLightInfos = ParameterKeys.New(new DirectLightData[LightingKeys.MaxDeferredPointLights]);

        /// <summary>
        /// The key linking the parameters of each spot light.
        /// </summary>
        public static readonly ParameterKey<SpotLightData[]> SpotLightInfos = ParameterKeys.New(new SpotLightData[LightingKeys.MaxDeferredPointLights]);

        /// <summary>
        /// The key setting the number of lights.
        /// </summary>
        public static readonly ParameterKey<int> LightCount = ParameterKeys.New(LightingKeys.MaxDeferredPointLights);

        #endregion

        #region Internal static members

        internal static Dictionary<ParameterKey, LightingDeferredSemantic> LightParametersDict = new Dictionary<ParameterKey, LightingDeferredSemantic>
        {
            { LightingPrepassRenderer.PointLightInfos,   LightingDeferredSemantic.PointLightInfos },
            { DeferredPointLightingKeys.LightAttenuationCutoff, LightingDeferredSemantic.LightAttenuationCutoff },
            { LightingPrepassRenderer.DirectLightInfos,  LightingDeferredSemantic.DirectLightInfos },
            { LightingPrepassRenderer.SpotLightInfos,    LightingDeferredSemantic.SpotLightInfos },
            { LightingPrepassRenderer.LightCount,        LightingDeferredSemantic.LightCount }
        };

        #endregion

        #region Constant values

        public const int TileCountX = 16;

        public const int TileCountY = 10;

        public const float AttenuationCutoff = 0.1f;

        // TODO: make this configurable or extract from the effect
        public const int MaxPointLightsPerTileDrawCall = LightingKeys.MaxDeferredPointLights;
        
        public const int MaxDirectLightsPerTileDrawCall = 1;

        public const int MaxSpotLightsPerTileDrawCall = 1;

        public const int MaxSpotShadowLightsPerTileDrawCall = 1;

        public const int MaxDirectShadowLightsPerTileDrawCall = 1;

        #endregion

        #region Private members

        private string effectName;

        private ShadowMapReceiverInfo[] receiverInfos;

        private ShadowMapReceiverVsmInfo[] receiverVsmInfos;

        private ShadowMapCascadeReceiverInfo[] cascadeInfos;

        private ParameterCollection[] effectParameterCollections;

        private List<EntityLightShadow> validLights;

        private List<EntityLightShadow> pointLights;

        private List<EntityLightShadow> spotLights;

        private List<EntityLightShadow> spotShadowLights;

        private List<EntityLightShadow> directionalLights;

        private List<EntityLightShadow> directionalShadowLights;

        private List<PointLightData> pointLightDatas;

        private List<SpotLightData> spotLightDatas;

        private List<DirectLightData> directionalLightDatas;

        private Dictionary<int, List<int>> regroupedTiles;

        private List<PointLightData>[] tilesGroups = new List<PointLightData>[TileCountX * TileCountY];

        private PointLightData[] currentPointLights = new PointLightData[MaxPointLightsPerTileDrawCall];

        private SpotLightData[] currentSpotLights = new SpotLightData[MaxSpotLightsPerTileDrawCall];

        private SpotLightData[] currentSpotShadowLights = new SpotLightData[MaxSpotShadowLightsPerTileDrawCall];

        private DirectLightData[] currentDirectLights = new DirectLightData[MaxDirectLightsPerTileDrawCall];

        private DirectLightData[] currentDirectShadowLights = new DirectLightData[MaxDirectShadowLightsPerTileDrawCall];

        private Texture lightTexture;

        private BlendState accumulationBlendState;

        private Effect pointLightingPrepassEffect;

        private Effect spotLightingPrepassEffect;

        private Effect directLightingPrepassEffect;

        private Dictionary<ShadowEffectInfo, Effect> shadowEffects;
        
        private Dictionary<ShadowEffectInfo, List<EntityLightShadow>> shadowLights;

        private Dictionary<ShadowEffectInfo, List<DirectLightData>> directShadowLightDatas;

        private Dictionary<ShadowEffectInfo, List<SpotLightData>> spotShadowLightDatas;

        private Dictionary<Effect, LightingDeferredParameters[]> lightingConfigurationsPerEffect;

        private Dictionary<ParameterKey, LightingDeferredSemantic> lightingParameterSemantics;

        private VertexArrayObject vertexArrayObject;

        private MeshDraw meshDraw;

        // External references
        private Texture depthStencilTexture;
        
        // External references
        private Texture gbufferTexture;

        #endregion

        #region Contructor

        /// <summary>
        /// LightPrepassRenderer constructor.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="effectName">The name of the effect used to compute lighting.</param>
        /// <param name="depthStencilTexture">The depth texture.</param>
        /// <param name="gbufferTexture">The gbuffer texture.</param>
        public LightingPrepassRenderer(IServiceRegistry services, string effectName, Texture depthStencilTexture, Texture gbufferTexture)
            : base(services)
        {
            validLights = new List<EntityLightShadow>();
            pointLights = new List<EntityLightShadow>();
            spotLights = new List<EntityLightShadow>();
            spotShadowLights = new List<EntityLightShadow>();
            directionalLights = new List<EntityLightShadow>();
            directionalShadowLights = new List<EntityLightShadow>();
            pointLightDatas = new List<PointLightData>();
            spotLightDatas = new List<SpotLightData>();
            directionalLightDatas = new List<DirectLightData>();
            regroupedTiles = new Dictionary<int, List<int>>();
            lightingConfigurationsPerEffect = new Dictionary<Effect, LightingDeferredParameters[]>();
            lightingParameterSemantics = new Dictionary<ParameterKey, LightingDeferredSemantic>();
            shadowEffects = new Dictionary<ShadowEffectInfo, Effect>();
            shadowLights = new Dictionary<ShadowEffectInfo, List<EntityLightShadow>>();
            directShadowLightDatas = new Dictionary<ShadowEffectInfo, List<DirectLightData>>();
            spotShadowLightDatas = new Dictionary<ShadowEffectInfo, List<SpotLightData>>();

            receiverInfos = new ShadowMapReceiverInfo[4];
            receiverVsmInfos = new ShadowMapReceiverVsmInfo[4];
            cascadeInfos = new ShadowMapCascadeReceiverInfo[16];
            effectParameterCollections = new ParameterCollection[2];

            this.effectName = effectName;
            this.depthStencilTexture = depthStencilTexture;
            this.gbufferTexture = gbufferTexture;
            DebugName = string.Format("LightingPrepass [{0}]", effectName);
        }

        #endregion

        #region Public methods

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();

            // Initialize tile groups
            for (int i = 0; i < tilesGroups.Length; ++i)
                tilesGroups[i] = new List<PointLightData>();

            var compilerParameters = GetDefaultCompilerParameters();
            // point lights
            pointLightingPrepassEffect = EffectSystem.LoadEffect(effectName + ".ParadoxPointPrepassLighting", compilerParameters);
            CreateLightingUpdateInfo(pointLightingPrepassEffect);

            // spot lights
            spotLightingPrepassEffect = EffectSystem.LoadEffect(effectName + ".ParadoxSpotPrepassLighting", compilerParameters);
            CreateLightingUpdateInfo(spotLightingPrepassEffect);

            // directional lights
            directLightingPrepassEffect = EffectSystem.LoadEffect(effectName + ".ParadoxDirectPrepassLighting", compilerParameters);
            CreateLightingUpdateInfo(directLightingPrepassEffect);

            // shadow lights
            for (var cascadeCount = 1; cascadeCount < 5; ++cascadeCount)
            {
                AddShadowEffect(cascadeCount, ShadowMapFilterType.Nearest, compilerParameters);
                AddShadowEffect(cascadeCount, ShadowMapFilterType.PercentageCloserFiltering, compilerParameters);
                AddShadowEffect(cascadeCount, ShadowMapFilterType.Variance, compilerParameters);
            }

            // Create lighting accumulation texture
            lightTexture = Texture.New2D(GraphicsDevice, GraphicsDevice.BackBuffer.ViewWidth, GraphicsDevice.BackBuffer.ViewHeight, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);

            // Set GBuffer and depth stencil as input, as well as light texture
            Pass.Parameters.Set(RenderTargetKeys.DepthStencilSource, depthStencilTexture);
            Pass.Parameters.Set(GBufferBaseKeys.GBufferTexture, gbufferTexture);
            Pass.Parameters.Set(LightDeferredShadingKeys.LightTexture, lightTexture);

            // Generates a quad for post effect rendering (should be utility function)
            var vertices = new[]
                {
                    -1.0f,  1.0f, 
                     1.0f,  1.0f,
                    -1.0f, -1.0f, 
                     1.0f, -1.0f,
                };

            // Create the quad used for tile rendering
            meshDraw = new MeshDraw
            {
                DrawCount = 4,
                PrimitiveType = PrimitiveType.TriangleStrip,
                VertexBuffers = new[]
                                {
                                    new VertexBufferBinding(Buffer.Vertex.New(GraphicsDevice, vertices), new VertexDeclaration(VertexElement.Position<Vector2>()), 4)
                                }
            };

            // TODO: fix the effect signature (not safe here)
            // Prepare VAO
            vertexArrayObject = VertexArrayObject.New(GraphicsDevice, pointLightingPrepassEffect.InputSignature, meshDraw.VertexBuffers);

            // Prepare blend state used for second pass (accumulation)
            var blendStateDesc = new BlendStateDescription();
            blendStateDesc.SetDefaults();
            blendStateDesc.AlphaToCoverageEnable = false;
            blendStateDesc.IndependentBlendEnable = false;
            blendStateDesc.RenderTargets[0].BlendEnable = true;

            blendStateDesc.RenderTargets[0].AlphaBlendFunction = BlendFunction.Add;
            blendStateDesc.RenderTargets[0].AlphaSourceBlend = Blend.One;
            blendStateDesc.RenderTargets[0].AlphaDestinationBlend = Blend.One;

            blendStateDesc.RenderTargets[0].ColorBlendFunction = BlendFunction.Add;
            blendStateDesc.RenderTargets[0].ColorSourceBlend = Blend.One;
            blendStateDesc.RenderTargets[0].ColorDestinationBlend = Blend.One;

            blendStateDesc.RenderTargets[0].ColorWriteChannels = ColorWriteChannels.All;

            accumulationBlendState = BlendState.New(GraphicsDevice, blendStateDesc);
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            base.Unload();

            lightTexture.Dispose();
            lightTexture = null;
        }

        #endregion

        #region Private methods

        private void AddShadowEffect(int cascadeCount, ShadowMapFilterType filterType, CompilerParameters parameters)
        {
            parameters.Set(ShadowMapParameters.ShadowMapCascadeCount.ComposeWith("shadows[0]"), cascadeCount);
            parameters.Set(ShadowMapParameters.FilterType, filterType);

            //////////////////////////////////////////////
            // DIRECTIONAL LIGHT
            parameters.Set(ShadowMapParameters.LightType.ComposeWith("shadows[0]"), LightType.Directional);

            ShadowEffectInfo seiDirect;
            seiDirect.LightType = LightType.Directional;
            seiDirect.CascadeCount = cascadeCount;
            seiDirect.Filter = filterType;
            AddEffect(seiDirect, cascadeCount, parameters, directShadowLightDatas);

            //////////////////////////////////////////////
            // SPOT LIGHT
            parameters.Set(ShadowMapParameters.LightType.ComposeWith("shadows[0]"), LightType.Spot);

            ShadowEffectInfo seiSpot;
            seiSpot.LightType = LightType.Spot;
            seiSpot.CascadeCount = cascadeCount;
            seiSpot.Filter = filterType;

            AddEffect(seiSpot, cascadeCount, parameters, spotShadowLightDatas);
        }

        private void AddEffect<T>(ShadowEffectInfo sei, int cascadeCount, CompilerParameters parameters, Dictionary<ShadowEffectInfo, List<T>> lightDatas)
        {
            var effect = EffectSystem.LoadEffect(effectName + ".ParadoxShadowPrepassLighting", parameters);
            var lightingGroupInfo = CreateLightingUpdateInfo(effect);
            lightingGroupInfo.ShadowParameters = new List<ShadowUpdateInfo>
            {
                LightingProcessorHelpers.CreateShadowUpdateInfo(0, cascadeCount)
            };

            shadowEffects.Add(sei, effect);
            shadowLights.Add(sei, new List<EntityLightShadow>());
            lightDatas.Add(sei, new List<T>());
        }

        /// <summary>
        /// Update light lists and choose the new light configuration.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void UpdateLightDatas(RenderContext context)
        {
            // get the lightprocessor
            var entitySystem = Services.GetServiceAs<EntitySystem>();
            if (entitySystem == null)
                return;
            var lightProcessor = entitySystem.GetProcessor<LightShadowProcessor>();
            if (lightProcessor == null)
                return;

            // TODO: better detection of shadows?
            var hasShadowRenderer = RenderSystem.Pipeline.GetProcessor<ShadowMapRenderer>() != null;

            // filter out the non-deferred lights
            foreach (var light in lightProcessor.Lights)
            {
                if (light.Value.Light.Deferred && light.Value.Light.Enabled)
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
                            if (hasShadowRenderer && light.Value.HasShadowMap && lightProcessor.ActiveShadowMaps.Contains(light.Value.ShadowMap))
                                directionalShadowLights.Add(light.Value);
                            else
                                directionalLights.Add(light.Value);
                            break;
                        case LightType.Spot:
                            if (hasShadowRenderer && light.Value.HasShadowMap && lightProcessor.ActiveShadowMaps.Contains(light.Value.ShadowMap))
                                spotShadowLights.Add(light.Value);
                            else
                                spotLights.Add(light.Value);
                            break;
                    }
                }
            }

            foreach (var light in pointLights)
            {
                PointLightData data;
                data.DiffuseColor = light.Light.Color;
                // TODO: Linearize intensity
                data.LightIntensity = light.Light.Intensity;//(float)Math.Pow(light.Light.Intensity, 2.2f);
                data.LightPosition = new Vector3(light.Entity.Transformation.WorldMatrix.M41, light.Entity.Transformation.WorldMatrix.M42, light.Entity.Transformation.WorldMatrix.M43);
                data.LightPosition /= light.Entity.Transformation.WorldMatrix.M44;
                data.LightRadius = light.Light.DecayStart;

                // TODO: Linearize color
                //data.DiffuseColor.Pow(2.2f);

                pointLightDatas.Add(data);
            }

            foreach (var light in directionalLights)
                directionalLightDatas.Add(GetDirectLightData(light));

            foreach (var light in directionalShadowLights)
            {
                ShadowEffectInfo sei;
                sei.LightType = LightType.Directional;
                sei.CascadeCount = light.Light.ShadowMapCascadeCount;
                sei.Filter = light.Light.ShadowMapFilterType;
                
                List<DirectLightData> dataList;
                if (directShadowLightDatas.TryGetValue(sei, out dataList))
                {
                    dataList.Add(GetDirectLightData(light));
                    shadowLights[sei].Add(light);
                }
            }

            foreach (var light in spotLights)
                spotLightDatas.Add(GetSpotLightData(light));

            foreach (var light in spotShadowLights)
            {
                ShadowEffectInfo sei;
                sei.LightType = LightType.Spot;
                sei.CascadeCount = light.Light.ShadowMapCascadeCount;
                sei.Filter = light.Light.ShadowMapFilterType;

                List<SpotLightData> dataList;
                if (spotShadowLightDatas.TryGetValue(sei, out dataList))
                {
                    dataList.Add(GetSpotLightData(light));
                    shadowLights[sei].Add(light);
                }
            }
        }

        

        /// <summary>
        /// Clear the lists.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void EndRender(RenderContext context)
        {
            validLights.Clear();
            pointLights.Clear();
            spotLights.Clear();
            spotShadowLights.Clear();
            directionalLights.Clear();
            directionalShadowLights.Clear();
            pointLightDatas.Clear();
            spotLightDatas.Clear();
            directionalLightDatas.Clear();

            foreach(var lightList in shadowLights)
                lightList.Value.Clear();
            foreach (var dataList in directShadowLightDatas)
                dataList.Value.Clear();
            foreach (var dataList in spotShadowLightDatas)
                dataList.Value.Clear();

            regroupedTiles.Clear();
            lightingParameterSemantics.Clear();
        }

        /*private void RegroupTilesPerShader()
        {
            var maxLightCountperTile = new int[tilesGroups.Length];

            for (var tileIndex = 0; tileIndex < tilesGroups.Length; ++tileIndex)
            {
                int lightsInShader = lightConfigurations[lightConfigurations.Count - 1];
                foreach (var config in lightConfigurations)
                {
                    if (config >= tilesGroups[tileIndex].Count)
                    {
                        lightsInShader = config;
                        break;
                    }
                }
                maxLightCountperTile[tileIndex] = lightsInShader;
            }

            for (var tileIndex = 0; tileIndex < tilesGroups.Length; ++tileIndex)
            {
                var lightCount = maxLightCountperTile[tileIndex];
                if (!regroupedTiles.ContainsKey(lightCount))
                    regroupedTiles.Add(lightCount, new List<int>());
                regroupedTiles[lightCount].Add(tileIndex);
            }
        }*/

        protected override void OnRendering(RenderContext context)
        {
            // update the effects
            CreateLightingUpdateInfo(pointLightingPrepassEffect);
            CreateLightingUpdateInfo(spotLightingPrepassEffect);
            CreateLightingUpdateInfo(directLightingPrepassEffect);
            foreach (var shadowEffect in shadowEffects)
                CreateLightingUpdateInfo(shadowEffect.Value);
            
            // update the list of lights
            UpdateLightDatas(context);

            // if there is no light, use albedo
            if (pointLights.Count == 0 && spotLights.Count == 0 && directionalLights.Count == 0 && directionalShadowLights.Count == 0 && spotShadowLights.Count == 0)
            {
                GraphicsDevice.Clear(lightTexture, new Color4(1.0f, 1.0f, 1.0f, 0.0f));
            }
            else
            {
                // Clear and set light accumulation target
                GraphicsDevice.Clear(lightTexture, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
                GraphicsDevice.SetRenderTarget(lightTexture); // no depth buffer
                // TODO: make sure that the lightRenderTarget.Texture is not bound to any shader to prevent some warnings

                // Set default blend state
                GraphicsDevice.SetBlendState(null);

                // set default depth stencil test
                GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

                // TODO: remove this?
                // override specular intensity
                context.Parameters.Set(MaterialKeys.SpecularIntensity, 1.0f);

                UpdateTiles(Pass.Parameters);

                // direct lighting
                var hasPreviousLighting = RenderTileForDirectLights(context);

                // spot lights
                hasPreviousLighting |= RenderTileForSpotLights(context, hasPreviousLighting);

                // lighting with shadows
                foreach (var lightList in shadowLights)
                {
                    if (lightList.Value.Count > 0)
                    {
                        var effect = shadowEffects[lightList.Key];
                        // direct light or spot light
                        if ((lightList.Key.LightType == LightType.Directional && RenderTileForDirectShadowLights(context, hasPreviousLighting, effect, lightList.Value, directShadowLightDatas[lightList.Key], lightList.Key.Filter == ShadowMapFilterType.Variance))
                            || lightList.Key.LightType == LightType.Spot && RenderTileForSpotShadowLights(context, hasPreviousLighting, effect, lightList.Value, spotShadowLightDatas[lightList.Key], lightList.Key.Filter == ShadowMapFilterType.Variance))
                        {
                            effect.UnbindResources();
                            hasPreviousLighting = true;
                        }
                    }
                }

                // point lights
                RenderTilesForPointLights(context, hasPreviousLighting);

                // Reset default blend state
                GraphicsDevice.SetBlendState(null);
            }
            
            // reset the light infos
            EndRender(context);

            // TDO: remove this
            // Reset some values
            context.Parameters.Reset(MaterialKeys.SpecularIntensity);
        }

        private void SetDirectLightParameters(RenderContext context, Effect effect, DirectLightData[] data, int lightCount)
        {
            LightingDeferredParameters[] deferredParameters = null;
            if (lightingConfigurationsPerEffect.TryGetValue(effect, out deferredParameters))
            {
                foreach (var deferredParam in deferredParameters)
                {
                    if ((deferredParam.Semantic & LightingDeferredSemantic.DirectLightInfos) != 0)
                        context.Parameters.Set(deferredParam.DirectLightInfosKey, data);
                    if ((deferredParam.Semantic & LightingDeferredSemantic.LightCount) != 0)
                        context.Parameters.Set(deferredParam.LightCountKey, lightCount);
                }
            }
        }

        private void SetSpotLightParameters(RenderContext context, Effect effect, SpotLightData[] data, int lightCount)
        {
            LightingDeferredParameters[] deferredParameters = null;
            if (lightingConfigurationsPerEffect.TryGetValue(effect, out deferredParameters))
            {
                foreach (var deferredParam in deferredParameters)
                {
                    if ((deferredParam.Semantic & LightingDeferredSemantic.SpotLightInfos) != 0)
                        context.Parameters.Set(deferredParam.SpotLightInfosKey, data);
                    if ((deferredParam.Semantic & LightingDeferredSemantic.LightCount) != 0)
                        context.Parameters.Set(deferredParam.LightCountKey, lightCount);
                }
            }
        }

        private int SetCascadeInfo(List<EntityLightShadow> lights, int startLightIndex, int lightIndex, int cascadeCount, ShadowUpdateInfo shadowUpdateInfo)
        {
            var shadowLight = lights[startLightIndex + lightIndex];
            receiverInfos[lightIndex] = shadowLight.ShadowMap.ReceiverInfo;
            receiverVsmInfos[lightIndex] = shadowLight.ShadowMap.ReceiverVsmInfo;
            for (var cascade = 0; cascade < shadowUpdateInfo.CascadeCount; ++cascade)
                cascadeInfos[cascadeCount + cascade] = shadowLight.ShadowMap.Cascades[cascade].ReceiverInfo;

            return cascadeCount + shadowUpdateInfo.CascadeCount;
        }

        private void RenderTile(RenderContext context, Effect effect, bool hasPreviousDraw, int currentDrawIndex)
        {
            // Render this tile (we only need current pass and context parameters)
            effectParameterCollections[0] = context.CurrentPass.Parameters;
            effectParameterCollections[1] = context.Parameters;

            // Apply effect & parameters
            effect.Apply(effectParameterCollections);

            // the blend state is null at the beginning of the renderer. We should switch to accumulation blend state at the second render.
            // Any further render do not need to switch the blend state since it is already corerctly set.
            if (!hasPreviousDraw && currentDrawIndex == 1)
                GraphicsDevice.SetBlendState(accumulationBlendState);

            // Set VAO and draw tile
            GraphicsDevice.SetVertexArrayObject(vertexArrayObject);
            GraphicsDevice.Draw(meshDraw.PrimitiveType, meshDraw.DrawCount, meshDraw.StartLocation);
        }

        private void RenderShadowLight(RenderContext context, List<EntityLightShadow> lights, int startLightIndex, int lightCount, int cascadeCount, bool varianceShadowMap, bool hasPreviousDraw, int currentDrawIndex, ShadowUpdateInfo shadowUpdateInfo, Effect effect)
        {
            // update shadow parameters
            context.Parameters.Set((ParameterKey<ShadowMapReceiverInfo[]>)shadowUpdateInfo.ShadowMapReceiverInfoKey, receiverInfos, 0, lightCount);
            context.Parameters.Set((ParameterKey<ShadowMapCascadeReceiverInfo[]>)shadowUpdateInfo.ShadowMapLevelReceiverInfoKey, cascadeInfos, 0, cascadeCount);
            context.Parameters.Set(shadowUpdateInfo.ShadowMapLightCountKey, lightCount);
            // TODO: change texture set when multiple shadow maps will be handled.
            if (varianceShadowMap)
            {
                context.Parameters.Set((ParameterKey<ShadowMapReceiverVsmInfo[]>)shadowUpdateInfo.ShadowMapReceiverVsmInfoKey, receiverVsmInfos, 0, cascadeCount);
                context.Parameters.Set(shadowUpdateInfo.ShadowMapTextureKey, lights[startLightIndex].ShadowMap.Texture.ShadowMapTargetTexture);
            }
            else
                context.Parameters.Set(shadowUpdateInfo.ShadowMapTextureKey, lights[startLightIndex].ShadowMap.Texture.ShadowMapDepthTexture);

            RenderTile(context, effect, hasPreviousDraw, currentDrawIndex);
        }

        private bool RenderTileForDirectLights(RenderContext context)
        {
            // only one tile since the directional lights affects the whole screen
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountX, 1);
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountY, 1);
            context.Parameters.Set(DeferredTiledShaderKeys.TileIndex, 0);

            int directLightCount = directionalLightDatas.Count;

            int drawCount = (directLightCount + MaxDirectLightsPerTileDrawCall - 1) / MaxDirectLightsPerTileDrawCall;
            var startLightIndex = 0;

            for (int currentDrawIndex = 0; currentDrawIndex < drawCount; ++currentDrawIndex)
            {
                int lightCount = Math.Min(directLightCount - startLightIndex, MaxDirectLightsPerTileDrawCall);
            
                // prepare directional light datas
                for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                    currentDirectLights[lightIndex] = directionalLightDatas[startLightIndex + lightIndex];

                // Set data for shader
                SetDirectLightParameters(context, directLightingPrepassEffect, currentDirectLights, lightCount);

                RenderTile(context, directLightingPrepassEffect, false, currentDrawIndex);

                startLightIndex += MaxDirectLightsPerTileDrawCall;
            }

            if (drawCount > 0)
            {
                directLightingPrepassEffect.UnbindResources();
                return true;
            }

            return false;
        }

        private bool RenderTileForDirectShadowLights(RenderContext context, bool hasPreviousDraw, Effect effect, List<EntityLightShadow> lights, List<DirectLightData> lightDatas, bool varianceShadowMap)
        {
            // only one tile since the directional lights affects the whole screen
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountX, 1);
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountY, 1);
            context.Parameters.Set(DeferredTiledShaderKeys.TileIndex, 0);

            int directShadowLightCount = lightDatas.Count;

            if (hasPreviousDraw)
                GraphicsDevice.SetBlendState(accumulationBlendState);

            int drawCount = (directShadowLightCount + MaxDirectShadowLightsPerTileDrawCall - 1) / MaxDirectShadowLightsPerTileDrawCall;
            var startLightIndex = 0;

            // TODO: change that to handle mutiple shadow maps in the same shader - works now since the shader only render with 1 shadow map at a time.
            var lightingGroupInfo = LightingGroupInfo.GetOrCreate(effect);
            var shadowUpdateInfo = lightingGroupInfo.ShadowParameters[0];

            for (int i = 0; i < drawCount; ++i)
            {
                int lightCount = Math.Min(directShadowLightCount - startLightIndex, MaxDirectShadowLightsPerTileDrawCall);
                var cascadeCount = 0;
                // prepare directional shadow light datas
                for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                {
                    currentDirectShadowLights[lightIndex] = lightDatas[startLightIndex + lightIndex];
                    cascadeCount = SetCascadeInfo(lights, startLightIndex, lightIndex, cascadeCount, shadowUpdateInfo);
                }

                // Set data for shader
                SetDirectLightParameters(context, effect, currentDirectShadowLights, lightCount);
                
                RenderShadowLight(context, lights, startLightIndex, lightCount, cascadeCount, varianceShadowMap, hasPreviousDraw, i, shadowUpdateInfo, effect);

                startLightIndex += MaxDirectShadowLightsPerTileDrawCall;
            }

            return (drawCount > 0);
        }

        private bool RenderTileForSpotShadowLights(RenderContext context, bool hasPreviousDraw, Effect effect, List<EntityLightShadow> lights, List<SpotLightData> lightDatas, bool varianceShadowMap)
        {
            // TODO: look for tiles covered by spot lights
            // only one tile for spot lights for now
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountX, 1);
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountY, 1);
            context.Parameters.Set(DeferredTiledShaderKeys.TileIndex, 0);

            int spotShadowLightCount = lightDatas.Count;

            if (hasPreviousDraw)
                GraphicsDevice.SetBlendState(accumulationBlendState);

            int drawCount = (spotShadowLightCount + MaxSpotShadowLightsPerTileDrawCall - 1) / MaxSpotShadowLightsPerTileDrawCall;
            var startLightIndex = 0;

            // TODO: change that to handle mutiple shadow maps in the same shader - works now since the shader only render with 1 shadow map at a time.
            var lightGroupInfo = LightingGroupInfo.GetOrCreate(effect);
            var shadowUpdateInfo = lightGroupInfo.ShadowParameters[0];

            for (int i = 0; i < drawCount; ++i)
            {
                int lightCount = Math.Min(spotShadowLightCount - startLightIndex, MaxSpotShadowLightsPerTileDrawCall);
                int cascadeCount = 0;
                // prepare spot shadow light datas
                for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                {
                    currentSpotShadowLights[lightIndex] = lightDatas[startLightIndex + lightIndex];
                    cascadeCount = SetCascadeInfo(lights, startLightIndex, lightIndex, cascadeCount, shadowUpdateInfo);
                }

                // Set data for shader
                SetSpotLightParameters(context, effect, currentSpotShadowLights, lightCount);

                RenderShadowLight(context, lights, startLightIndex, lightCount, cascadeCount, varianceShadowMap, hasPreviousDraw, i, shadowUpdateInfo, effect);

                startLightIndex += MaxSpotShadowLightsPerTileDrawCall;
            }

            if (drawCount > 0)
            {
                effect.UnbindResources();
                return true;
            }

            return false;
        }

        private bool RenderTileForSpotLights(RenderContext context, bool hasPreviousDraw)
        {
            // TODO: look for tiles covered by spot lights
            // only one tile for spot lights for now
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountX, 1);
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountY, 1);
            context.Parameters.Set(DeferredTiledShaderKeys.TileIndex, 0);

            int spotLightCount = spotLightDatas.Count;

            if (hasPreviousDraw)
                GraphicsDevice.SetBlendState(accumulationBlendState);

            int drawCount = (spotLightCount + MaxSpotLightsPerTileDrawCall - 1) / MaxSpotLightsPerTileDrawCall;
            var startLightIndex = 0;

            for (int currentDrawIndex = 0; currentDrawIndex < drawCount; ++currentDrawIndex)
            {
                int lightCount = Math.Min(spotLightCount - startLightIndex, MaxSpotLightsPerTileDrawCall);
                
                // prepare spotlight datas
                for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                    currentSpotLights[lightIndex] = spotLightDatas[startLightIndex + lightIndex];

                // Set data for shader
                SetSpotLightParameters(context, spotLightingPrepassEffect, currentSpotLights, lightCount);

                RenderTile(context, spotLightingPrepassEffect, hasPreviousDraw, currentDrawIndex);

                startLightIndex += MaxSpotLightsPerTileDrawCall;
            }

            if (drawCount > 0)
            {
                spotLightingPrepassEffect.UnbindResources();
                return true;
            }

            return false;
        }

        private void RenderTilesForPointLights(RenderContext context, bool hasPreviousDraw)
        {
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountX, TileCountX);
            context.Parameters.Set(DeferredTiledShaderKeys.TileCountY, TileCountY);

            var hasDrawn = false;
            
            for (var tileIndex = 0; tileIndex < TileCountX * TileCountY; ++tileIndex)
            {
                var tilesGroup = this.tilesGroups[tileIndex];

                /*var lastEffect = lightPrepassEffects.Last();
                var effect = lastEffect.Value;
                var maxLightCount = lastEffect.Key;
                foreach (var effectKP in lightPrepassEffects)
                {
                    if (effectKP.Key >= tilesGroup.Count)
                    {
                        effect = effectKP.Value;
                        maxLightCount = effectKP.Key;
                    }
                }*/

                // Set tile index
                context.Parameters.Set(DeferredTiledShaderKeys.TileIndex, tileIndex);

                int drawCount = (tilesGroup.Count + MaxPointLightsPerTileDrawCall - 1) / MaxPointLightsPerTileDrawCall;

                if (hasPreviousDraw)
                    GraphicsDevice.SetBlendState(accumulationBlendState);
                else
                    GraphicsDevice.SetBlendState(null);

                var startLightIndex = 0;

                // One draw for every MaxPointLightsPerTileDrawCall lights
                for (int currentDrawIndex = 0; currentDrawIndex < drawCount; ++currentDrawIndex)
                {
                    // prepare PointLightData[]
                    int lightCount = Math.Min(tilesGroup.Count - startLightIndex, MaxPointLightsPerTileDrawCall);
                    for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                        currentPointLights[lightIndex] = tilesGroup[startLightIndex + lightIndex];

                    // Set data for shader
                    LightingDeferredParameters[] deferredParameters = null;
                    if (lightingConfigurationsPerEffect.TryGetValue(pointLightingPrepassEffect, out deferredParameters))
                    {
                        foreach (var deferredParam in deferredParameters)
                        {
                            if ((deferredParam.Semantic & LightingDeferredSemantic.PointLightInfos) != 0)
                            {
                                context.Parameters.Set(deferredParam.PointLightInfosKey, currentPointLights);
                            }
                            if ((deferredParam.Semantic & LightingDeferredSemantic.LightCount) != 0)
                            {
                                context.Parameters.Set(deferredParam.LightCountKey, lightCount);
                            }
                            if ((deferredParam.Semantic & LightingDeferredSemantic.LightAttenuationCutoff) != 0)
                            {
                                context.Parameters.Set(deferredParam.LightAttenuationCutoffKey, 0.1f);
                            }
                        }
                    }

                    RenderTile(context, pointLightingPrepassEffect, hasPreviousDraw, currentDrawIndex);
                    
                    startLightIndex += MaxPointLightsPerTileDrawCall;
                }

                hasDrawn |= (drawCount > 0);
            }

            if (hasDrawn)
                pointLightingPrepassEffect.UnbindResources();
        }

        private void UpdateTiles(ParameterCollection viewParameters)
        {
            for (int i = 0; i < tilesGroups.Length; ++i)
                tilesGroups[i].Clear();

            var lightAttenuationCutoff = AttenuationCutoff;

            Matrix viewMatrix;
            viewParameters.Get(TransformationKeys.View, out viewMatrix);
            Matrix projMatrix;
            viewParameters.Get(TransformationKeys.Projection, out projMatrix);

            for (int index = 0; index < pointLightDatas.Count; index++)
            {
                var lightData = pointLightDatas[index];

                // Transform light position from WS to VS
                Vector3.TransformCoordinate(ref lightData.LightPosition, ref viewMatrix, out lightData.LightPosition);

                // ------------------------------------------------------------------------------------------
                // TEMPORARY FIX FOR DEFERRED LIGHTS
                // ------------------------------------------------------------------------------------------
                //lightData2[index].DiffuseColor.Pow(1 / 4.0f);
                //lightData2[index].LightIntensity = (float)Math.Pow(lightData2[index].LightIntensity, 1.0f / 2.2f);
                // ------------------------------------------------------------------------------------------

                float lightDistanceMax = CalculateMaxDistance(lightData.LightIntensity, lightData.LightRadius, lightAttenuationCutoff);

                // Check if this light is really visible (not behind us)
                if (lightData.LightPosition.Z - lightDistanceMax > 0.0f)
                    continue;

                var clipRegion = ComputeClipRegion(lightData.LightPosition, lightDistanceMax, ref projMatrix);

                var tileStartX = (int)((clipRegion.X * 0.5f + 0.5f) * TileCountX);
                var tileEndX = (int)((clipRegion.Z * 0.5f + 0.5f) * TileCountX);
                var tileStartY = (int)((clipRegion.Y * 0.5f + 0.5f) * TileCountY);
                var tileEndY = (int)((clipRegion.W * 0.5f + 0.5f) * TileCountY);

                for (int y = tileStartY; y <= tileEndY; ++y)
                {
                    if (y < 0 || y >= TileCountY)
                        continue;
                    for (int x = tileStartX; x <= tileEndX; ++x)
                    {
                        if (x < 0 || x >= TileCountX)
                            continue;
                        tilesGroups[y * TileCountX + x].Add(lightData);
                    }
                }
            }

            for (int index = 0; index < spotLightDatas.Count; index++)
            {
                var lightData = spotLightDatas[index];

                // Transform light direction from WS to VS
                Vector3.TransformNormal(ref lightData.LightDirection, ref viewMatrix, out lightData.LightDirection);
                // Transform light position from WS to VS
                Vector3.TransformCoordinate(ref lightData.LightPosition, ref viewMatrix, out lightData.LightPosition);
                spotLightDatas[index] = lightData;
            }

            foreach (var lightDatas in spotShadowLightDatas)
            {
                for (int index = 0; index < lightDatas.Value.Count; index++)
                {
                    var lightData = lightDatas.Value[index];

                    // Transform light direction from WS to VS
                    Vector3.TransformNormal(ref lightData.LightDirection, ref viewMatrix, out lightData.LightDirection);
                    // Transform light position from WS to VS
                    Vector3.TransformCoordinate(ref lightData.LightPosition, ref viewMatrix, out lightData.LightPosition);
                    lightDatas.Value[index] = lightData;
                }
            }

            for (int index = 0; index < directionalLightDatas.Count; index++)
            {
                var lightData = directionalLightDatas[index];

                // Transform light direction from WS to VS
                Vector3.TransformNormal(ref lightData.LightDirection, ref viewMatrix, out lightData.LightDirection);
                directionalLightDatas[index] = lightData;
            }

            foreach (var lightDatas in directShadowLightDatas)
            {
                for (int index = 0; index < lightDatas.Value.Count; index++)
                {
                    var lightData = lightDatas.Value[index];

                    // Transform light direction from WS to VS
                    Vector3.TransformNormal(ref lightData.LightDirection, ref viewMatrix, out lightData.LightDirection);
                    lightDatas.Value[index] = lightData;
                }
            }
        }

        private static float CalculateMaxDistance(float lightIntensity, float lightRadius, float cutoff)
        {
            // DMax = r + r * (sqrt(Li/Ic) - 1) = r * sqrt(Li/Ic)
            return (float)(lightRadius * Math.Sqrt(lightIntensity / cutoff));
        }

        private static void UpdateClipRegionRoot(float nc,          // Tangent plane x/y normal coordinate (view space)
                            float lc,          // Light x/y coordinate (view space)
                            float lz,          // Light z coordinate (view space)
                            float lightRadius,
                            float cameraScale, // Project scale for coordinate (_11 or _22 for x/y respectively)
                            ref float clipMin,
                            ref float clipMax)
        {
            float nz = (lightRadius - nc * lc) / lz;
            float pz = (lc * lc + lz * lz - lightRadius * lightRadius) /
                        (lz - (nz / nc) * lc);

            if (pz > 0.0f)
            {
                float c = -nz * cameraScale / nc;
                if (nc > 0.0f)
                {        // Left side boundary
                    clipMin = Math.Max(clipMin, c);
                }
                else
                {                          // Right side boundary
                    clipMax = Math.Min(clipMax, c);
                }
            }
        }

        private static void UpdateClipRegion(float lc,          // Light x/y coordinate (view space)
                                float lz,          // Light z coordinate (view space)
                                float lightRadius,
                                float cameraScale, // Project scale for coordinate (_11 or _22 for x/y respectively)
                                ref float clipMin,
                                ref float clipMax)
        {
            float rSq = lightRadius * lightRadius;
            float lcSqPluslzSq = lc * lc + lz * lz;
            float d = rSq * lc * lc - lcSqPluslzSq * (rSq - lz * lz);

            if (d > 0)
            {
                float a = lightRadius * lc;
                float b = (float)Math.Sqrt(d);
                float nx0 = (a + b) / lcSqPluslzSq;
                float nx1 = (a - b) / lcSqPluslzSq;

                UpdateClipRegionRoot(nx0, lc, lz, lightRadius, cameraScale, ref clipMin, ref clipMax);
                UpdateClipRegionRoot(nx1, lc, lz, lightRadius, cameraScale, ref clipMin, ref clipMax);
            }
        }

        private static Vector4 ComputeClipRegion(Vector3 lightPosView, float lightRadius, ref Matrix projection)
        {
            var clipRegion = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
            var clipMin = new Vector2(-1.0f, -1.0f);
            var clipMax = new Vector2(1.0f, 1.0f);

            UpdateClipRegion(lightPosView.X, -lightPosView.Z, lightRadius, projection.M11, ref clipMin.X, ref clipMax.X);
            UpdateClipRegion(lightPosView.Y, -lightPosView.Z, lightRadius, projection.M22, ref clipMin.Y, ref clipMax.Y);

            clipRegion = new Vector4(clipMin.X, clipMin.Y, clipMax.X, clipMax.Y);

            return clipRegion;
        }

        private LightingGroupInfo CreateLightingUpdateInfo(Effect effect)
        {
            var lightingGroupInfo = LightingGroupInfo.GetOrCreate(effect);

            if (!lightingGroupInfo.IsLightingSetup || !lightingConfigurationsPerEffect.ContainsKey(effect))
            {
                var finalList = new List<LightingDeferredParameters>();
                var continueSearch = true;
                var index = 0;
                while (continueSearch)
                {
                    continueSearch = SearchLightingGroup(effect, index, "lightingGroups", finalList);
                    ++index;
                }

                continueSearch = true;
                index = 0;
                while (continueSearch)
                {
                    continueSearch = SearchLightingGroup(effect, index, "shadows", finalList);
                    ++index;
                }

                lightingConfigurationsPerEffect.Remove(effect);

                if (finalList.Count > 0)
                    lightingConfigurationsPerEffect.Add(effect, finalList.ToArray());

                lightingGroupInfo.IsLightingSetup = true;
            }

            return lightingGroupInfo;
        }

        private bool SearchLightingGroup(Effect effect, int index, string groupName, List<LightingDeferredParameters> finalList)
        {
            var constantBuffers = effect.Bytecode.Reflection.ConstantBuffers;
            var info = new LightingDeferredParameters();

            LightingDeferredSemantic foundParameterSemantic;
            var foundParam = false;

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
                            case LightingDeferredSemantic.PointLightInfos:
                                info.Count = member.Count;
                                info.PointLightInfosKey = (ParameterKey<PointLightData[]>)member.Param.Key;
                                break;
                            case LightingDeferredSemantic.DirectLightInfos:
                                info.Count = member.Count;
                                info.DirectLightInfosKey = (ParameterKey<DirectLightData[]>)member.Param.Key;
                                break;
                            case LightingDeferredSemantic.SpotLightInfos:
                                info.Count = member.Count;
                                info.SpotLightInfosKey = (ParameterKey<SpotLightData[]>)member.Param.Key;
                                break;
                            case LightingDeferredSemantic.LightAttenuationCutoff:
                                info.LightAttenuationCutoffKey = (ParameterKey<float>)member.Param.Key;
                                break;
                            case LightingDeferredSemantic.LightCount:
                                info.LightCountKey = (ParameterKey<int>)member.Param.Key;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            if (foundParam)
                finalList.Add(info);
            return foundParam;
        }

        private void UpdateLightingParameterSemantics(int index, string compositionName)
        {
            lightingParameterSemantics.Clear();
            // TODO: use StringBuilder instead
            var lightGroupSubKey = string.Format(compositionName + "[{0}]", index);
            foreach (var param in LightParametersDict)
            {
                lightingParameterSemantics.Add(param.Key.ComposeWith(lightGroupSubKey), param.Value);
            }
        }

        #endregion

        #region Private static methods

        private static DirectLightData GetDirectLightData(EntityLightShadow light)
        {
            DirectLightData data;
            Vector3 lightDirection;
            data.DiffuseColor = light.Light.Color;
            // TODO: Linearize intensity
            data.LightIntensity = light.Light.Intensity;//(float)Math.Pow(light.Light.Intensity, 2.2f);
            var lightDir = light.Light.LightDirection;
            Vector3.TransformNormal(ref lightDir, ref light.Entity.Transformation.WorldMatrix, out lightDirection);
            data.LightDirection = lightDirection;

            // TODO: Linearize color
            //data.DiffuseColor.Pow(2.2f);

            return data;
        }

        private static SpotLightData GetSpotLightData(EntityLightShadow light)
        {
            SpotLightData data;
            Vector3 lightDirection;
            Vector3 lightPosition;
            var zero = Vector3.Zero;
            var lightDir = light.Light.LightDirection;
            Vector3.TransformNormal(ref lightDir, ref light.Entity.Transformation.WorldMatrix, out lightDirection);
            data.LightDirection = lightDirection;
            data.DiffuseColor = light.Light.Color;
            // TODO: Linearize intensity
            data.LightIntensity = light.Light.Intensity;//(float)Math.Pow(light.Light.Intensity, 2.2f);
            data.LightPosition = new Vector3(light.Entity.Transformation.WorldMatrix.M41, light.Entity.Transformation.WorldMatrix.M42, light.Entity.Transformation.WorldMatrix.M43);
            data.LightPosition /= light.Entity.Transformation.WorldMatrix.M44;

            data.SpotBeamAngle = (float)Math.Cos(Math.PI * light.Light.SpotBeamAngle / 180);
            data.SpotFieldAngle = (float)Math.Cos(Math.PI * light.Light.SpotFieldAngle / 180);

            data.Range = light.Light.DecayStart;

            // TODO: Linearize color
            //data.DiffuseColor.Pow(2.2f);

            return data;
        }

        #endregion

        #region Helper structures

        private class LightingDeferredParameters
        {
            public LightingDeferredSemantic Semantic;
            public int Count;
            public ParameterKey<int> LightCountKey;
            public ParameterKey<PointLightData[]> PointLightInfosKey;
            public ParameterKey<DirectLightData[]> DirectLightInfosKey;
            public ParameterKey<SpotLightData[]> SpotLightInfosKey;
            public ParameterKey<float> LightAttenuationCutoffKey;

            public LightingDeferredParameters()
            {
                Semantic = LightingDeferredSemantic.None;
                Count = 0;
                PointLightInfosKey = null;
                DirectLightInfosKey = null;
                SpotLightInfosKey = null;
                LightCountKey = null;
                LightAttenuationCutoffKey = null;
            }
        }

        [Flags]
        internal enum LightingDeferredSemantic
        {
            None = 0x0,
            PointLightInfos = 0x1,
            DirectLightInfos = 0x2,
            SpotLightInfos = 0x4,
            LightAttenuationCutoff = 0x8,
            LightCount = 0x10
        }

        private struct ShadowEffectInfo
        {
            public LightType LightType;
            public int CascadeCount;
            public ShadowMapFilterType Filter;
        }

        #endregion
    }
}
