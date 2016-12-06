// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    /// <summary>
    /// Renders point light shadow maps using a cubemap
    /// </summary>
    public class LightPointShadowMapRendererCubeMap : LightShadowMapRendererBase
    {
        // Number of border pixels to add to the cube map in order to allow filtering
        public const int BorderPixels = 8;

        public readonly RenderStage ShadowMapRenderStageCubeMap;
        
        private PoolListStruct<ShaderData> shaderDataPool;

        public LightPointShadowMapRendererCubeMap(ShadowMapRenderer parent) : base(parent)
        {
            ShadowMapRenderStageCubeMap = ShadowMapRenderer.RenderSystem.GetRenderStage("ShadowMapCasterCubeMap");

            shaderDataPool = new PoolListStruct<ShaderData>(4, () => new ShaderData());
        }

        public override void Reset()
        {
            shaderDataPool.Clear();
        }

        public override ILightShadowMapShaderGroupData CreateShaderGroupData(LightShadowType shadowType)
        {
            return new ShaderGroupData(shadowType);
        }

        public override bool CanRenderLight(IDirectLight light)
        {
            var pl = light as LightPoint;
            if (pl != null)
                return ((LightPointShadowMap)pl.Shadow).Type == LightPointShadowMapType.Cubemap;
            return false;
        }

        public override LightShadowMapTexture CreateTexture(LightComponent lightComponent, IDirectLight light, int shadowMapSize)
        {
            var shadowMap = base.CreateTexture(lightComponent, light, shadowMapSize);
            shadowMap.CascadeCount = 6; // 6 faces
            return shadowMap;
        }

        public override void CreateRenderViews(LightShadowMapTexture shadowMapTexture, VisibilityGroup visibilityGroup)
        {
            for (int i = 0; i < 6; i++)
            {
                // Allocate shadow render view
                var shadowRenderView = ShadowMapRenderer.ShadowRenderViews.Add();
                shadowRenderView.RenderView = ShadowMapRenderer.CurrentView;
                shadowRenderView.ShadowMapTexture = shadowMapTexture;
                shadowRenderView.Rectangle = shadowMapTexture.GetRectangle(i);

                var clippingPlanes = GetLightClippingPlanes((LightPoint)shadowMapTexture.Light);
                shadowRenderView.NearClipPlane = clippingPlanes.X;
                shadowRenderView.FarClipPlane = clippingPlanes.Y;

                // Compute view parameters
                // Note: we only need view here since we are doing paraboloid projection in the shaders
                GetViewParameters(shadowMapTexture, i, out shadowRenderView.View, true);

                // Calculate angle of the projection with border pixels taken into account to allow filtering
                float halfMapSize = shadowRenderView.Rectangle.Width/2;
                float halfFov = (float)Math.Atan((halfMapSize + BorderPixels)/ halfMapSize);

                shadowRenderView.Projection = Matrix.PerspectiveFovRH(halfFov*2, 1.0f, shadowRenderView.NearClipPlane, shadowRenderView.FarClipPlane);
                shadowRenderView.ViewProjection = shadowRenderView.View * shadowRenderView.Projection;

                shadowRenderView.VisiblityIgnoreDepthPlanes = false;

                // Add the render view for the current frame
                ShadowMapRenderer.RenderSystem.Views.Add(shadowRenderView);

                // Collect objects in shadow views
                visibilityGroup.Collect(shadowRenderView);
            }
        }

        private Vector2 GetLightClippingPlanes(LightPoint pointLight)
        {
            return new Vector2(0.05f, pointLight.Radius + 2.0f);
        }

        /// <returns>
        /// x = Near; y = 1/(Far-Near)
        /// </returns>
        private Vector2 GetShadowMapDepthParameters(LightShadowMapTexture shadowMapTexture)
        {
            var lightPoint = shadowMapTexture.Light as LightPoint;
            Vector2 clippingPlanes = GetLightClippingPlanes(lightPoint);
            return new Vector2(clippingPlanes.X, 1.0f / (clippingPlanes.Y - clippingPlanes.X));
        }

        private void GetViewParameters(LightShadowMapTexture shadowMapTexture, int index, out Matrix view, bool forCasting)
        {
            Matrix rotation = Matrix.Identity;
            Matrix flipping = Matrix.Identity;
            // Flip Y for rendering shadow maps
            if (forCasting)
            {
                // Render upside down, so reading doesn't need any modification
                flipping.Up = -flipping.Up;
            }
            
            // Apply light position
            view = Matrix.Translation(-shadowMapTexture.LightComponent.Position);

            // Select face based on index
            switch (index)
            {
                case 0: // Front
                    rotation *= Matrix.RotationY(MathUtil.Pi);
                    flipping.Right = -flipping.Right;
                    break;
                case 1: // Back
                    break;
                case 2: // Right
                    rotation *= Matrix.RotationY(MathUtil.PiOverTwo);
                    break;
                case 3: // Left
                    rotation *= Matrix.RotationY(-MathUtil.PiOverTwo);
                    flipping.Right = -flipping.Right;
                    break;
                case 4: // Up
                    rotation *= Matrix.RotationX(-MathUtil.PiOverTwo);
                    break;
                case 5: // Down
                    rotation *= Matrix.RotationX(MathUtil.PiOverTwo);
                    flipping.Up = -flipping.Up;
                    break;
            }

            view *= rotation * flipping;
        }

        public override void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, LightShadowMapTexture shadowMapTexture)
        {
        }

        public override void Collect(RenderContext context, LightShadowMapTexture lightShadowMap)
        {
            var visibilityGroup = context.Tags.Get(SceneInstance.CurrentVisibilityGroup);

            var shaderData = shaderDataPool.Add();
            lightShadowMap.ShaderData = shaderData;
            shaderData.Texture = lightShadowMap.Atlas.Texture;
            shaderData.DepthBias = lightShadowMap.Light.Shadow.BiasParameters.DepthBias;
            shaderData.Position = lightShadowMap.LightComponent.Position;

            Vector2 atlasSize = new Vector2(lightShadowMap.Atlas.Width, lightShadowMap.Atlas.Height);

            Rectangle frontRectangle = lightShadowMap.GetRectangle(0);
            
            int borderPixels2 = BorderPixels * 2;

            // Coordinates have 1 border pixel so that the shadow receivers don't accidentally sample outside of the texture area
            shaderData.FaceSize = new Vector2(frontRectangle.Width - borderPixels2, frontRectangle.Height - borderPixels2) / atlasSize;

            for (int i = 0; i < 6; i++)
            {
                Rectangle faceRectangle = lightShadowMap.GetRectangle(i);
                shaderData.FaceOffsets[i] = new Vector2(faceRectangle.Left + BorderPixels, faceRectangle.Top + BorderPixels) /atlasSize;
            }

            var clippingPlanes = GetLightClippingPlanes((LightPoint)lightShadowMap.Light);
            shaderData.LightDepthParameters = CameraKeys.ZProjectionACalculate(clippingPlanes.X, clippingPlanes.Y);
        }
        
        private class ShaderData : ILightShadowMapShaderData
        {
            public Texture Texture;

            /// <summary>
            /// Offset to every one of the faces of the cubemap in the atlas
            /// </summary>
            public readonly Vector2[] FaceOffsets = new Vector2[6];

            /// <summary>
            /// Size of a single face of the shadow map
            /// </summary>
            public Vector2 FaceSize;

            /// <summary>
            /// Calculated by <see cref="CameraKeys.ZProjectionACalculate"/> to reconstruct linear depth from the depth buffer
            /// </summary>
            public Vector2 LightDepthParameters;

            /// <summary>
            /// Position of the light
            /// </summary>
            public Vector3 Position;

            public float DepthBias;
        }

        private class ShaderGroupData : ILightShadowMapShaderGroupData
        {
            private const string ShaderName = "ShadowMapReceiverPointCubeMap";

            private ShaderMixinSource shadowShader;
            private LightShadowType shadowType;

            private Texture shadowMapTexture;
            private Vector2 shadowMapTextureSize;
            private Vector2 shadowMapTextureTexelSize;
            
            private Vector2[] lightFaceOffsets;
            private Vector2[] lightFaceSize;
            private Vector2[] lightDepthParameters;
            private Vector4[] lightPosition;
            private float[] depthBiases;
            private ValueParameterKey<Vector2> lightFaceOffsetsKey;
            private ValueParameterKey<Vector2> lightFaceSizeKey;
            private ValueParameterKey<Vector2> lightDepthParametersKey;
            private ValueParameterKey<Vector4> lightPositionKey;

            private ObjectParameterKey<Texture> shadowMapTextureKey;
            private ValueParameterKey<Vector2> shadowMapTextureSizeKey;
            private ValueParameterKey<Vector2> shadowMapTextureTexelSizeKey;
            private ValueParameterKey<float> depthBiasesKey;

            public ShaderGroupData(LightShadowType shadowType)
            {
                this.shadowType = shadowType;
            }

            public void ApplyShader(ShaderMixinSource mixin)
            {
                mixin.CloneFrom(shadowShader);
            }

            public void UpdateLayout(string compositionName)
            {
                shadowMapTextureKey = ShadowMapKeys.Texture.ComposeWith(compositionName);
                shadowMapTextureSizeKey = ShadowMapKeys.TextureSize.ComposeWith(compositionName);
                shadowMapTextureTexelSizeKey = ShadowMapKeys.TextureTexelSize.ComposeWith(compositionName);
                lightPositionKey = ShadowMapReceiverPointCubeMapKeys.LightPosition.ComposeWith(compositionName);
                lightFaceOffsetsKey = ShadowMapReceiverPointCubeMapKeys.LightFaceOffsets.ComposeWith(compositionName);
                lightFaceSizeKey = ShadowMapReceiverPointCubeMapKeys.LightFaceSize.ComposeWith(compositionName);
                lightDepthParametersKey = ShadowMapReceiverPointCubeMapKeys.LightDepthParameters.ComposeWith(compositionName);
                depthBiasesKey = ShadowMapReceiverPointCubeMapKeys.DepthBiases.ComposeWith(compositionName);
            }

            public void UpdateLightCount(int lightLastCount, int lightCurrentCount)
            {
                shadowShader = new ShaderMixinSource();
                shadowShader.Mixins.Add(new ShaderClassSource(ShaderName, lightCurrentCount));

                switch (shadowType & LightShadowType.FilterMask)
                {
                    case LightShadowType.PCF3x3:
                        shadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerDraw.Lighting", 3));
                        break;
                    case LightShadowType.PCF5x5:
                        shadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerDraw.Lighting", 5));
                        break;
                    case LightShadowType.PCF7x7:
                        shadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerDraw.Lighting", 7));
                        break;
                    default:
                        shadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterDefault", "PerDraw.Lighting"));
                        break;
                }

                Array.Resize(ref lightFaceOffsets, lightCurrentCount*6);
                Array.Resize(ref lightFaceSize, lightCurrentCount);
                Array.Resize(ref lightDepthParameters, lightCurrentCount);
                Array.Resize(ref lightPosition, lightCurrentCount);
                Array.Resize(ref depthBiases, lightCurrentCount);
            }

            public void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, FastListStruct<LightDynamicEntry> currentLights)
            {
            }

            public void ApplyDrawParameters(RenderDrawContext context, ParameterCollection parameters, FastListStruct<LightDynamicEntry> currentLights, ref BoundingBoxExt boundingBox)
            {
                for (int lightIndex = 0; lightIndex < currentLights.Count; ++lightIndex)
                {
                    var lightEntry = currentLights[lightIndex];
                    var shaderData = (ShaderData)lightEntry.ShadowMapTexture.ShaderData;
                    for(int i = 0; i < 6; i++)
                        lightFaceOffsets[lightIndex*6+i] = shaderData.FaceOffsets[i];
                    lightPosition[lightIndex] = new  Vector4(shaderData.Position, 1);
                    lightFaceSize[lightIndex] = shaderData.FaceSize;
                    depthBiases[lightIndex] = shaderData.DepthBias;
                    lightDepthParameters[lightIndex] = shaderData.LightDepthParameters;

                    // TODO: should be setup just once at creation time
                    if (lightIndex == 0)
                    {
                        shadowMapTexture = shaderData.Texture;
                        if (shadowMapTexture != null)
                        {
                            shadowMapTextureSize = new Vector2(shadowMapTexture.Width, shadowMapTexture.Height);
                            shadowMapTextureTexelSize = 1.0f / shadowMapTextureSize;
                        }
                    }
                }

                parameters.Set(shadowMapTextureKey, shadowMapTexture);
                parameters.Set(shadowMapTextureSizeKey, shadowMapTextureSize);
                parameters.Set(shadowMapTextureTexelSizeKey, shadowMapTextureTexelSize);

                parameters.Set(lightPositionKey, lightPosition);
                parameters.Set(lightFaceOffsetsKey, lightFaceOffsets);
                parameters.Set(lightFaceSizeKey, lightFaceSize);
                parameters.Set(lightDepthParametersKey, lightDepthParameters);

                parameters.Set(depthBiasesKey, depthBiases);
            }
        }
    }
}