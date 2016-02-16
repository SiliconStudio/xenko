// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows.NextGen
{
    /// <summary>
    /// Renders a shadow map from a directional light.
    /// </summary>
    public class LightSpotShadowMapRenderer : LightShadowMapRendererBase
    {
        /// <summary>
        /// The various UP vectors to try.
        /// </summary>
        private static readonly Vector3[] VectorUps = { Vector3.UnitZ, Vector3.UnitY, Vector3.UnitX };

        private PoolListStruct<LightSpotShadowMapShaderData> shaderDataPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightSpotShadowMapRenderer"/> class.
        /// </summary>
        public LightSpotShadowMapRenderer()
        {
            shaderDataPool = new PoolListStruct<LightSpotShadowMapShaderData>(8, CreateLightSpotShadowMapShaderDAta);
        }
        
        public override void Reset()
        {
            shaderDataPool.Clear();
        }

        public override ILightShadowMapShaderGroupData CreateShaderGroupData(string compositionKey, LightShadowType shadowType, int maxLightCount)
        {
            return new LightSpotShadowMapGroupShaderData(compositionKey, shadowType, maxLightCount);
        }
        
        public override void Extract(RenderContext context, ShadowMapRenderer shadowMapRenderer, LightShadowMapTexture lightShadowMap)
        {
            throw new System.NotImplementedException();
        }

        public override void GetCascadeViewParameters(LightShadowMapTexture shadowMapTexture, int cascadeIndex, out Matrix view, out Matrix projection)
        {
            throw new System.NotImplementedException();
        }

        private class LightSpotShadowMapShaderData : ILightShadowMapShaderData
        {
            public Texture Texture;

            public float DepthBias;

            public float OffsetScale;

            public Matrix WorldToShadowCascadeUV;
        }

        private class LightSpotShadowMapGroupShaderData : ILightShadowMapShaderGroupData
        {
            private const string ShaderName = "ShadowMapReceiverSpot";

            private readonly LightShadowType shadowType;

            private readonly Matrix[] worldToShadowCascadeUV;

            private readonly float[] depthBiases;

            private readonly float[] offsetScales;

            private Texture shadowMapTexture;

            private Vector2 shadowMapTextureSize;

            private Vector2 shadowMapTextureTexelSize;

            private readonly ShaderMixinSource shadowShader;

            private readonly ObjectParameterKey<Texture> shadowMapTextureKey;

            private readonly ValueParameterKey<Matrix> worldToShadowCascadeUVsKey;

            private readonly ValueParameterKey<float> depthBiasesKey;

            private readonly ValueParameterKey<float> offsetScalesKey;

            private readonly ValueParameterKey<Vector2> shadowMapTextureSizeKey;

            private readonly ValueParameterKey<Vector2> shadowMapTextureTexelSizeKey;

            /// <summary>
            /// Initializes a new instance of the <see cref="LightSpotShadowMapGroupShaderData" /> class.
            /// </summary>
            /// <param name="compositionKey">The composition key.</param>
            /// <param name="shadowType">Type of the shadow.</param>
            /// <param name="lightCountMax">The light count maximum.</param>
            public LightSpotShadowMapGroupShaderData(string compositionKey, LightShadowType shadowType, int lightCountMax)
            {
                this.shadowType = shadowType;
                worldToShadowCascadeUV = new Matrix[lightCountMax];
                depthBiases = new float[lightCountMax];
                offsetScales = new float[lightCountMax];

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource(ShaderName,lightCountMax, (this.shadowType & LightShadowType.Debug) != 0));
                // TODO: Temporary passing filter here

                switch (shadowType & LightShadowType.FilterMask)
                {
                    case LightShadowType.PCF3x3:
                        mixin.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", 3));
                        break;
                    case LightShadowType.PCF5x5:
                        mixin.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", 5));
                        break;
                    case LightShadowType.PCF7x7:
                        mixin.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", 7));
                        break;
                    default:
                        mixin.Mixins.Add(new ShaderClassSource("ShadowMapFilterDefault"));
                        break;
                }

                shadowShader = mixin;
                shadowMapTextureKey = ShadowMapKeys.Texture.ComposeWith(compositionKey);
                shadowMapTextureSizeKey = ShadowMapKeys.TextureSize.ComposeWith(compositionKey);
                shadowMapTextureTexelSizeKey = ShadowMapKeys.TextureTexelSize.ComposeWith(compositionKey);
                worldToShadowCascadeUVsKey = ShadowMapReceiverBaseKeys.WorldToShadowCascadeUV.ComposeWith(compositionKey);
                depthBiasesKey = ShadowMapReceiverBaseKeys.DepthBiases.ComposeWith(compositionKey);
                offsetScalesKey = ShadowMapReceiverBaseKeys.OffsetScales.ComposeWith(compositionKey);
            }

            public void ApplyShader(ShaderMixinSource mixin)
            {
                mixin.CloneFrom(shadowShader);
            }

            public void SetShadowMapShaderData(int index, ILightShadowMapShaderData shaderData)
            {
                var singleLightData = (LightSpotShadowMapShaderData)shaderData;
                worldToShadowCascadeUV[index] = singleLightData.WorldToShadowCascadeUV;

                depthBiases[index] = singleLightData.DepthBias;
                offsetScales[index] = singleLightData.OffsetScale;

                // TODO: should be setup just once at creation time
                if (index == 0)
                {
                    shadowMapTexture = singleLightData.Texture;
                    if (shadowMapTexture != null)
                    {
                        shadowMapTextureSize = new Vector2(shadowMapTexture.Width, shadowMapTexture.Height);
                        shadowMapTextureTexelSize = 1.0f / shadowMapTextureSize;
                    }
                }
            }

            public void ApplyParameters(NextGenParameterCollection parameters)
            {
                parameters.Set(shadowMapTextureKey, shadowMapTexture);
                parameters.Set(shadowMapTextureSizeKey, shadowMapTextureSize);
                parameters.Set(shadowMapTextureTexelSizeKey, shadowMapTextureTexelSize);
                parameters.Set(worldToShadowCascadeUVsKey, worldToShadowCascadeUV);
                parameters.Set(depthBiasesKey, depthBiases);
                parameters.Set(offsetScalesKey, offsetScales);
            }
        }

        private static LightSpotShadowMapShaderData CreateLightSpotShadowMapShaderDAta()
        {
            return new LightSpotShadowMapShaderData();
        }
    }
}