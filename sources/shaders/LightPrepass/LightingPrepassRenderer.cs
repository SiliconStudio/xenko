// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects.Modules.Shadowmap;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Effects.Modules.LightPrepass
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PointLightData
    {
        public Vector3 LightPosition;
        public float LightRadius;

        public Color3 DiffuseColor;
        public float LightIntensity;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SpotLightData
    {
        public Vector3 LightDirection;
        public float LightIntensity;

        public Vector3 LightPosition;
        public float SpotFieldAngle;

        public Color3 DiffuseColor;
        public float SpotBeamAngle;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DirectLightData
    {
        public Vector3 LightDirection;
        public float LightIntensity;

        public Color3 DiffuseColor;
    };

    public class LightingPrepassRenderer : Renderer
    {
        #region Static members

        public static readonly ParameterKey<PointLightData[]> PointLightInfos = ParameterKeys.New(new PointLightData[64]);

        public static readonly ParameterKey<DirectLightData[]> DirectLightInfos = ParameterKeys.New(new DirectLightData[64]);

        public static readonly ParameterKey<SpotLightData[]> SpotLightInfos = ParameterKeys.New(new SpotLightData[64]);

        public static readonly ParameterKey<int> LightCount = ParameterKeys.New(64);

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
        public const int MaxPointLightsPerTileDrawCall = 64;
        
        public const int MaxDirectLightsPerTileDrawCall = 1;

        public const int MaxSpotLightsPerTileDrawCall = 1;

        public const int MaxShadowLightsPerTileDrawCall = 1;

        #endregion

        #region Private members

        private string effectName;

        private ShadowMapReceiverInfo[] receiverInfos;

        private ShadowMapReceiverVsmInfo[] receiverVsmInfos;

        private ShadowMapCascadeReceiverInfo[] cascadeInfos;

        private List<EntityLightShadow> validLights;

        private List<EntityLightShadow> pointLights;

        private List<EntityLightShadow> spotLights;

        private List<EntityLightShadow> directionalLights;

        private List<EntityLightShadow> directionalShadowLights;

        private List<PointLightData> pointLightDatas;

        private List<SpotLightData> spotLightDatas;

        private List<DirectLightData> directionalLightDatas;

        private Dictionary<int, List<int>> regroupedTiles;

        private List<PointLightData>[] tilesGroups = new List<PointLightData>[TileCountX * TileCountY];

        private PointLightData[] currentPointLights = new PointLightData[MaxPointLightsPerTileDrawCall];

        private SpotLightData[] currentSpotLights = new SpotLightData[MaxSpotLightsPerTileDrawCall];

        private DirectLightData[] currentDirectLights = new DirectLightData[MaxDirectLightsPerTileDrawCall];

        private DirectLightData[] currentShadowLights = new DirectLightData[MaxShadowLightsPerTileDrawCall];

        private RenderTarget lightRenderTarget;

        private BlendState accumulationBlendState;

        private Effect pointLightingPrepassEffect;

        private Effect spotLightingPrepassEffect;

        private Effect directLightingPrepassEffect;

        private Dictionary<ShadowEffectInfo, Effect> shadowEffects;
        
        private Dictionary<ShadowEffectInfo, List<EntityLightShadow>> shadowLights;

        private Dictionary<ShadowEffectInfo, List<DirectLightData>> shadowLightDatas;

        private Dictionary<Effect, LightingDeferredParameters[]> lightingConfigurationsPerEffect;

        private Dictionary<ParameterKey, LightingDeferredSemantic> lightingParameterSemantics;

        private VertexArrayObject vertexArrayObject;

        private MeshDraw meshDraw;

        // External references
        private DepthStencilBuffer depthStencilBuffer;
        
        // External references
        private Texture2D gbufferTexture;

        #endregion

        #region Contructor

        public LightingPrepassRenderer(IServiceRegistry services, string effectName, DepthStencilBuffer depthStencilBuffer, Texture2D gbufferTexture)
            : base(services)
        {
            validLights = new List<EntityLightShadow>();
            pointLights = new List<EntityLightShadow>();
            spotLights = new List<EntityLightShadow>();
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
            shadowLightDatas = new Dictionary<ShadowEffectInfo, List<DirectLightData>>();

            receiverInfos = new ShadowMapReceiverInfo[4];
            receiverVsmInfos = new ShadowMapReceiverVsmInfo[4];
            cascadeInfos = new ShadowMapCascadeReceiverInfo[16];

            this.effectName = effectName;
            this.depthStencilBuffer = depthStencilBuffer;
            this.gbufferTexture = gbufferTexture;
        }

        #endregion

        #region Public methods

        /// <inheritdoc/>
        public override void Load()
        {
            // Initialize tile groups
            for (int i = 0; i < tilesGroups.Length; ++i)
                tilesGroups[i] = new List<PointLightData>();

            pointLightingPrepassEffect = EffectSystem.LoadEffect(effectName + ".ParadoxPointPrepassLighting");
            CreateLightingUpdateInfo(pointLightingPrepassEffect);

            spotLightingPrepassEffect = EffectSystem.LoadEffect(effectName + ".ParadoxSpotPrepassLighting");
            CreateLightingUpdateInfo(spotLightingPrepassEffect);

            // TODO: find a way to enumerate available shaders
            /*var parameters = new CompilerParameters();
            for (var i = 2; i <= 64; i = i + 62)
            {
                parameters.Set(LightingKeys.MaxDeferredLights, i);
                var effect = EffectSystem.LoadEffect("LightPrepassEffect", parameters);
                lightPrepassEffects.Add(i, effect);
                lightConfigurations.Add(i);

                CreateLightingUpdateInfo(effect);
            }*/

            // directional lights
            directLightingPrepassEffect = EffectSystem.LoadEffect(effectName + ".ParadoxDirectPrepassLighting");
            CreateLightingUpdateInfo(directLightingPrepassEffect);

            // shadow lights
            var parameters = new CompilerParameters();
            for (var cascadeCount = 1; cascadeCount < 5; ++cascadeCount)
            {
                AddShadowEffect(cascadeCount, ShadowMapFilterType.Nearest, parameters);
                AddShadowEffect(cascadeCount, ShadowMapFilterType.PercentageCloserFiltering, parameters);
                AddShadowEffect(cascadeCount, ShadowMapFilterType.Variance, parameters);
            }

            // Create lighting accumulation texture
            var lightTexture = Texture2D.New(GraphicsDevice, GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            lightRenderTarget = lightTexture.ToRenderTarget();

            // Set GBuffer and depth stencil as input, as well as light texture
            Pass.Parameters.Set(RenderTargetKeys.DepthStencilSource, depthStencilBuffer.Texture);
            Pass.Parameters.Set(GBufferBaseKeys.GBufferTexture, gbufferTexture);
            Pass.Parameters.Set(LightDeferredShadingKeys.LightTexture, lightTexture);
            Pass.Parameters.Set(MaterialKeys.SpecularIntensity, 1.0f);

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

            Pass.StartPass += RenderTiles;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            Pass.StartPass -= RenderTiles;
            lightRenderTarget.Dispose();
            lightRenderTarget.Texture.Dispose();
            lightRenderTarget = null;
        }

        #endregion

        #region Private methods

        private void AddShadowEffect(int cascadeCount, ShadowMapFilterType filterType, CompilerParameters  parameters)
        {
            ShadowEffectInfo sei;
            sei.CascadeCount = cascadeCount;
            sei.Filter = filterType;

            parameters.Set(ShadowMapParameters.ShadowMapCascadeCount, cascadeCount);
            parameters.Set(ShadowMapParameters.FilterType, filterType);

            var effect = EffectSystem.LoadEffect(effectName + ".ParadoxDirectShadowPrepassLighting", parameters);
            CreateLightingUpdateInfo(effect);
            effect.ShadowParameters = new List<ShadowUpdateInfo>();
            effect.ShadowParameters.Add(LightingProcessorHelpers.CreateShadowUpdateInfo(0, cascadeCount));

            shadowEffects.Add(sei, effect);
            shadowLights.Add(sei, new List<EntityLightShadow>());
            shadowLightDatas.Add(sei, new List<DirectLightData>());
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
                            spotLights.Add(light.Value);
                            break;
                    }
                }
            }

            var zero = Vector3.Zero;
            Vector3 lightPosition;
            foreach (var light in pointLights)
            {
                PointLightData data;
                data.DiffuseColor = light.Light.Color;
                // TODO: Linearize intensity
                data.LightIntensity = light.Light.Intensity;//(float)Math.Pow(light.Light.Intensity, 2.2f);
                Vector3.TransformCoordinate(ref zero, ref light.Entity.Transformation.WorldMatrix, out lightPosition);
                data.LightPosition = lightPosition;
                data.LightRadius = light.Light.DecayStart;

                // TODO: Linearize color
                //data.DiffuseColor.Pow(2.2f);

                pointLightDatas.Add(data);
            }

            Vector3 lightDir;
            Vector3 lightDirection;
            foreach (var light in directionalLights)
            {
                DirectLightData data;
                data.DiffuseColor = light.Light.Color;
                // TODO: Linearize intensity
                data.LightIntensity = light.Light.Intensity;//(float)Math.Pow(light.Light.Intensity, 2.2f);
                lightDir = light.Light.LightDirection;
                Vector3.TransformNormal(ref lightDir, ref light.Entity.Transformation.WorldMatrix, out lightDirection);
                data.LightDirection = lightDirection;

                // TODO: Linearize color
                //data.DiffuseColor.Pow(2.2f);

                directionalLightDatas.Add(data);
            }

            foreach (var light in directionalShadowLights)
            {
                ShadowEffectInfo sei;
                sei.CascadeCount = light.Light.ShadowMapCascadeCount;
                sei.Filter = light.Light.ShadowMapFilterType;
                
                List<DirectLightData> dataList;
                if (shadowLightDatas.TryGetValue(sei, out dataList))
                {
                    DirectLightData data;
                    data.DiffuseColor = light.Light.Color;
                    // TODO: Linearize intensity
                    data.LightIntensity = light.Light.Intensity;//(float)Math.Pow(light.Light.Intensity, 2.2f);
                    lightDir = light.Light.LightDirection;
                    Vector3.TransformNormal(ref lightDir, ref light.Entity.Transformation.WorldMatrix, out lightDirection);
                    data.LightDirection = lightDirection;

                    // TODO: Linearize color
                    //data.DiffuseColor.Pow(2.2f);

                    dataList.Add(data);
                    shadowLights[sei].Add(light);
                }
            }

            foreach (var light in spotLights)
            {
                SpotLightData data;
                lightDir = light.Light.LightDirection;
                Vector3.TransformNormal(ref lightDir, ref light.Entity.Transformation.WorldMatrix, out lightDirection);
                data.LightDirection = lightDirection;
                data.DiffuseColor = light.Light.Color;
                // TODO: Linearize intensity
                data.LightIntensity = light.Light.Intensity;//(float)Math.Pow(light.Light.Intensity, 2.2f);
                Vector3.TransformCoordinate(ref zero, ref light.Entity.Transformation.WorldMatrix, out lightPosition);
                data.LightPosition = lightPosition;

                data.SpotBeamAngle = (float)Math.Cos(Math.PI * light.Light.SpotBeamAngle / 180);
                data.SpotFieldAngle = (float)Math.Cos(Math.PI * light.Light.SpotFieldAngle / 180);

                // TODO: Linearize color
                //data.DiffuseColor.Pow(2.2f);

                spotLightDatas.Add(data);
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
            directionalLights.Clear();
            directionalShadowLights.Clear();
            pointLightDatas.Clear();
            spotLightDatas.Clear();
            directionalLightDatas.Clear();

            foreach(var lightList in shadowLights)
                lightList.Value.Clear();
            foreach (var dataList in shadowLightDatas)
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

        private void RenderTiles(RenderContext context)
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
            if (pointLights.Count == 0 && spotLights.Count == 0 && directionalLights.Count == 0 && directionalShadowLights.Count == 0)
            {
                GraphicsDevice.Clear(lightRenderTarget, new Color4(1.0f, 1.0f, 1.0f, 0.0f));
                return;
            }

            // Clear and set light accumulation target
            GraphicsDevice.Clear(lightRenderTarget, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
            GraphicsDevice.SetRenderTarget(lightRenderTarget);

            // Set default blend state
            GraphicsDevice.SetBlendState(null);

            UpdateTiles(Pass.Parameters);

            // direct lighting
            var hasPreviousLighting = RenderTileForDirectLights(context);

            // direct lighting with shadows
            foreach (var lightList in shadowLights)
            {
                if (lightList.Value.Count > 0)
                    hasPreviousLighting |= RenderTileForDirectShadowLights(context, hasPreviousLighting, shadowEffects[lightList.Key], lightList.Value, shadowLightDatas[lightList.Key], lightList.Key.Filter == ShadowMapFilterType.Variance);
            }

            // spot lights
            hasPreviousLighting |= RenderTileForSpotLights(context, hasPreviousLighting);

            // point lights
            RenderTilesForPointLights(context, hasPreviousLighting);
            
            EndRender(context);
        }

        private bool RenderTileForDirectLights(RenderContext context)
        {
            // only one tile since the directional lights affects the whole screen
            context.Parameters.Set(DeferredLightingShaderKeys.TileCountX, 1);
            context.Parameters.Set(DeferredLightingShaderKeys.TileCountY, 1);
            context.Parameters.Set(DeferredLightingShaderKeys.TileIndex, 0);

            int directLightCount = directionalLightDatas.Count;

            int drawCount = (directLightCount + MaxDirectLightsPerTileDrawCall - 1) / MaxDirectLightsPerTileDrawCall;
            var startLightIndex = 0;

            for (int i = 0; i < drawCount; ++i)
            {
                int lightCount = Math.Min(directLightCount - startLightIndex, MaxDirectLightsPerTileDrawCall);
            
                // prepare directional light datas
                for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                    currentDirectLights[lightIndex] = directionalLightDatas[startLightIndex + lightIndex];

                // Set data for shader
                LightingDeferredParameters[] deferredParameters = null;
                if (lightingConfigurationsPerEffect.TryGetValue(directLightingPrepassEffect, out deferredParameters))
                {
                    foreach (var deferredParam in deferredParameters)
                    {
                        if ((deferredParam.Semantic & LightingDeferredSemantic.DirectLightInfos) != 0)
                        {
                            context.Parameters.Set(deferredParam.DirectLightInfosKey, currentDirectLights);
                        }
                        if ((deferredParam.Semantic & LightingDeferredSemantic.LightCount) != 0)
                        {
                            context.Parameters.Set(deferredParam.LightCountKey, lightCount);
                        }
                    }
                }

                // Render this tile (we only need current pass and context parameters)
                var parameterCollections = new ParameterCollection[2];
                parameterCollections[0] = context.CurrentPass.Parameters;
                parameterCollections[1] = context.Parameters;

                // Apply effect & parameters
                directLightingPrepassEffect.Apply(parameterCollections);

                // direct lighting is first lighting
                if (i == 1)
                    GraphicsDevice.SetBlendState(accumulationBlendState);

                // Set VAO and draw tile
                GraphicsDevice.SetVertexArrayObject(vertexArrayObject);
                GraphicsDevice.Draw(meshDraw.PrimitiveType, meshDraw.DrawCount, meshDraw.StartLocation);

                startLightIndex += MaxDirectLightsPerTileDrawCall;
            }

            return (drawCount > 0);
        }

        private bool RenderTileForDirectShadowLights(RenderContext context, bool hasPreviousDraw, Effect effect, List<EntityLightShadow> lights, List<DirectLightData> lightDatas, bool varianceShadowMap)
        {
            // only one tile since the directional lights affects the whole screen
            context.Parameters.Set(DeferredShadowLightingShaderKeys.TileCountX, 1);
            context.Parameters.Set(DeferredShadowLightingShaderKeys.TileCountY, 1);
            context.Parameters.Set(DeferredShadowLightingShaderKeys.TileIndex, 0);

            int directShadowLightCount = lightDatas.Count;

            if (hasPreviousDraw)
                GraphicsDevice.SetBlendState(accumulationBlendState);

            int drawCount = (directShadowLightCount + MaxShadowLightsPerTileDrawCall - 1) / MaxShadowLightsPerTileDrawCall;
            var startLightIndex = 0;

            // TODO: change that to handle mutiple shadow maps in the same shader - works now since the shader only render with 1 shadow map at a time.
            var shadowUpdateInfo = effect.ShadowParameters[0];

            for (int i = 0; i < drawCount; ++i)
            {
                int lightCount = Math.Min(directShadowLightCount - startLightIndex, MaxShadowLightsPerTileDrawCall);
                var cascadeCount = 0;
                // prepare directional shadow light datas
                for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                {
                    currentShadowLights[lightIndex] = lightDatas[startLightIndex + lightIndex];

                    var shadowLight = lights[startLightIndex + lightIndex];
                    receiverInfos[lightIndex] = shadowLight.ShadowMap.ReceiverInfo;
                    receiverVsmInfos[lightIndex] = shadowLight.ShadowMap.ReceiverVsmInfo;
                    for (var cascade = 0; cascade < shadowUpdateInfo.CascadeCount; ++cascade)
                        cascadeInfos[cascadeCount + cascade] = shadowLight.ShadowMap.Cascades[cascade].ReceiverInfo;
                    cascadeCount += shadowUpdateInfo.CascadeCount;
                }

                // Set data for shader
                LightingDeferredParameters[] deferredParameters = null;
                if (lightingConfigurationsPerEffect.TryGetValue(effect, out deferredParameters))
                {
                    foreach (var deferredParam in deferredParameters)
                    {
                        if ((deferredParam.Semantic & LightingDeferredSemantic.DirectLightInfos) != 0)
                        {
                            context.Parameters.Set(deferredParam.DirectLightInfosKey, currentShadowLights);
                        }
                        if ((deferredParam.Semantic & LightingDeferredSemantic.LightCount) != 0)
                        {
                            context.Parameters.Set(deferredParam.LightCountKey, lightCount);
                        }
                    }
                }

                // update shadow parameters
                context.Parameters.Set((ParameterKey<ShadowMapReceiverInfo[]>)shadowUpdateInfo.ShadowMapReceiverInfoKey, receiverInfos, 0, lightCount);
                context.Parameters.Set((ParameterKey<ShadowMapCascadeReceiverInfo[]>)shadowUpdateInfo.ShadowMapLevelReceiverInfoKey, cascadeInfos, 0, cascadeCount);
                context.Parameters.Set(shadowUpdateInfo.ShadowMapLightCountKey, lightCount);
                // TODO: change texture set when multiple shadow maps will be handled.
                if (varianceShadowMap)
                    context.Parameters.Set(shadowUpdateInfo.ShadowMapTextureKey, lights[startLightIndex].ShadowMap.Texture.ShadowMapTargetTexture);
                else
                    context.Parameters.Set(shadowUpdateInfo.ShadowMapTextureKey, lights[startLightIndex].ShadowMap.Texture.ShadowMapDepthTexture);

                // Render this tile (we only need current pass and context parameters)
                var parameterCollections = new ParameterCollection[2];
                parameterCollections[0] = context.CurrentPass.Parameters;
                parameterCollections[1] = context.Parameters;

                // Apply effect & parameters
                effect.Apply(parameterCollections);
                
                // first lighting?
                if (!hasPreviousDraw && i == 1)
                    GraphicsDevice.SetBlendState(accumulationBlendState);

                // Set VAO and draw tile
                GraphicsDevice.SetVertexArrayObject(vertexArrayObject);
                GraphicsDevice.Draw(meshDraw.PrimitiveType, meshDraw.DrawCount, meshDraw.StartLocation);

                startLightIndex += MaxShadowLightsPerTileDrawCall;
            }

            return (drawCount > 0);
        }

        private bool RenderTileForSpotLights(RenderContext context, bool hasPreviousDraw)
        {
            // TODO: look for tiles covered by spot lights
            // only one tile for spot lights for now
            context.Parameters.Set(DeferredLightingShaderKeys.TileCountX, 1);
            context.Parameters.Set(DeferredLightingShaderKeys.TileCountY, 1);
            context.Parameters.Set(DeferredLightingShaderKeys.TileIndex, 0);

            int spotLightCount = spotLightDatas.Count;

            if (hasPreviousDraw)
                GraphicsDevice.SetBlendState(accumulationBlendState);

            int drawCount = (spotLightCount + MaxSpotLightsPerTileDrawCall - 1) / MaxSpotLightsPerTileDrawCall;
            var startLightIndex = 0;

            for (int i = 0; i < drawCount; ++i)
            {
                int lightCount = Math.Min(spotLightCount - startLightIndex, MaxSpotLightsPerTileDrawCall);
                
                // prepare spotlight datas
                for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                    currentSpotLights[lightIndex] = spotLightDatas[startLightIndex + lightIndex];

                // Set data for shader
                LightingDeferredParameters[] deferredParameters = null;
                if (lightingConfigurationsPerEffect.TryGetValue(spotLightingPrepassEffect, out deferredParameters))
                {
                    foreach (var deferredParam in deferredParameters)
                    {
                        if ((deferredParam.Semantic & LightingDeferredSemantic.SpotLightInfos) != 0)
                        {
                            context.Parameters.Set(deferredParam.SpotLightInfosKey, currentSpotLights);
                        }
                        if ((deferredParam.Semantic & LightingDeferredSemantic.LightCount) != 0)
                        {
                            context.Parameters.Set(deferredParam.LightCountKey, lightCount);
                        }
                    }
                }

                // Render this tile (we only need current pass and context parameters)
                var parameterCollections = new ParameterCollection[2];
                parameterCollections[0] = context.CurrentPass.Parameters;
                parameterCollections[1] = context.Parameters;

                // Apply effect & parameters
                spotLightingPrepassEffect.Apply(parameterCollections);

                // spot lighting is first lighting
                if (!hasPreviousDraw && i == 1)
                    GraphicsDevice.SetBlendState(accumulationBlendState);

                // Set VAO and draw tile
                GraphicsDevice.SetVertexArrayObject(vertexArrayObject);
                GraphicsDevice.Draw(meshDraw.PrimitiveType, meshDraw.DrawCount, meshDraw.StartLocation);

                startLightIndex += MaxSpotLightsPerTileDrawCall;
            }

            return (drawCount > 0);
        }

        private void RenderTilesForPointLights(RenderContext context, bool hasPreviousDraw)
        {
            context.Parameters.Set(DeferredLightingShaderKeys.TileCountX, TileCountX);
            context.Parameters.Set(DeferredLightingShaderKeys.TileCountY, TileCountY);

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
                context.Parameters.Set(DeferredLightingShaderKeys.TileIndex, tileIndex);

                int drawCount = (tilesGroup.Count + MaxPointLightsPerTileDrawCall - 1) / MaxPointLightsPerTileDrawCall;

                if (hasPreviousDraw)
                    GraphicsDevice.SetBlendState(accumulationBlendState);

                var startLightIndex = 0;

                // One draw for every MaxPointLightsPerTileDrawCall lights
                for (int i = 0; i < drawCount; ++i)
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

                    // Render this tile (we only need current pass and context parameters)
                    var parameterCollections = new ParameterCollection[2];
                    parameterCollections[0] = context.CurrentPass.Parameters;
                    parameterCollections[1] = context.Parameters;

                    // Apply effect & parameters
                    pointLightingPrepassEffect.Apply(parameterCollections);

                    // On second draw, switch to accumulation
                    if (!hasPreviousDraw && i == 1)
                        GraphicsDevice.SetBlendState(accumulationBlendState);

                    // Set VAO and draw tile
                    GraphicsDevice.SetVertexArrayObject(vertexArrayObject);
                    GraphicsDevice.Draw(meshDraw.PrimitiveType, meshDraw.DrawCount, meshDraw.StartLocation);

                    startLightIndex += MaxPointLightsPerTileDrawCall;
                }

                // Set default blend state for next draw (if accumulation blend state has been used)
                if (!hasPreviousDraw && drawCount > 1)
                    GraphicsDevice.SetBlendState(null);
            }
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

            for (int index = 0; index < directionalLightDatas.Count; index++)
            {
                var lightData = directionalLightDatas[index];

                // Transform light direction from WS to VS
                Vector3.TransformNormal(ref lightData.LightDirection, ref viewMatrix, out lightData.LightDirection);
                directionalLightDatas[index] = lightData;
            }

            foreach (var lightDatas in shadowLightDatas)
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

        private void CreateLightingUpdateInfo(Effect effect)
        {
            if (effect != null && (effect.UpdateLightingParameters || !lightingConfigurationsPerEffect.ContainsKey(effect)))
            {
                var finalList = new List<LightingDeferredParameters>();
                var continueSearch = true;
                var index = 0;
                while (continueSearch)
                {
                    continueSearch = SearchLightingGroup(effect, index, "lightingGroups", ref finalList);
                    ++index;
                }

                continueSearch = true;
                index = 0;
                while (continueSearch)
                {
                    continueSearch = SearchLightingGroup(effect, index, "shadows", ref finalList);
                    ++index;
                }

                lightingConfigurationsPerEffect.Remove(effect);

                if (finalList.Count > 0)
                    lightingConfigurationsPerEffect.Add(effect, finalList.ToArray());

                effect.UpdateLightingParameters = false;
            }
        }

        private bool SearchLightingGroup(Effect effect, int index, string groupName, ref List<LightingDeferredParameters> finalList)
        {
            var constantBuffers = effect.ConstantBuffers;
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
            var lightGroupSubKey = string.Format("." + compositionName + "[{0}]", index);
            foreach (var param in LightParametersDict)
            {
                lightingParameterSemantics.Add(param.Key.AppendKey(lightGroupSubKey), param.Value);
            }
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
            public int CascadeCount;
            public ShadowMapFilterType Filter;
        }

        #endregion
    }
}
