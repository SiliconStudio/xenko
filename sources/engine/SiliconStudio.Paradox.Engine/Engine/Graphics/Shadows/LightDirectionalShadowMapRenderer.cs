// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Shadows
{
    /// <summary>
    /// Renders a shadow map from a directional light.
    /// </summary>
    public class LightDirectionalShadowMapRenderer : ILightShadowMapRenderer
    {
        /// <summary>
        /// The various UP vectors to try.
        /// </summary>
        private static readonly Vector3[] VectorUps = { Vector3.UnitZ, Vector3.UnitY, Vector3.UnitX };

        /// <summary>
        /// Base points for frustum corners.
        /// </summary>
        private static readonly Vector3[] FrustumBasePoints =
        {
            new Vector3(-1.0f,-1.0f, 0.0f), new Vector3(1.0f,-1.0f, 0.0f), new Vector3(-1.0f,1.0f, 0.0f), new Vector3(1.0f,1.0f, 0.0f),
            new Vector3(-1.0f,-1.0f, 1.0f), new Vector3(1.0f,-1.0f, 1.0f), new Vector3(-1.0f,1.0f, 1.0f), new Vector3(1.0f,1.0f, 1.0f),
        };

        private readonly float[] cascadeSplitRatios;
        private readonly Vector3[] cascadeFrustumCornersWS;
        private readonly Vector3[] frustumCorners;

        private PoolListStruct<LightDirectionalShadowMapShaderData> shaderDataPoolCascade1;
        private PoolListStruct<LightDirectionalShadowMapShaderData> shaderDataPoolCascade2;
        private PoolListStruct<LightDirectionalShadowMapShaderData> shaderDataPoolCascade4;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightDirectionalShadowMapRenderer"/> class.
        /// </summary>
        public LightDirectionalShadowMapRenderer()
        {
            cascadeSplitRatios = new float[4];
            cascadeFrustumCornersWS = new Vector3[8];
            frustumCorners = new Vector3[8];
            shaderDataPoolCascade1 = new PoolListStruct<LightDirectionalShadowMapShaderData>(4, CreateLightDirectionalShadowMapShaderDataCascade1);
            shaderDataPoolCascade2 = new PoolListStruct<LightDirectionalShadowMapShaderData>(4, CreateLightDirectionalShadowMapShaderDataCascade2);
            shaderDataPoolCascade4 = new PoolListStruct<LightDirectionalShadowMapShaderData>(4, CreateLightDirectionalShadowMapShaderDataCascade4);
        }
        
        public void Reset()
        {
            shaderDataPoolCascade1.Clear();
            shaderDataPoolCascade2.Clear();
            shaderDataPoolCascade4.Clear();
        }

        public ILightShadowMapShaderGroupData CreateShaderGroupData(string compositionKey, LightShadowType shadowType, int maxLightCount)
        {
            return new LightDirectionalShadowMapGroupShaderData(compositionKey, shadowType, maxLightCount);
        }

        public void Render(RenderContext context, ShadowMapRenderer shadowMapRenderer, LightShadowMapTexture lightShadowMap)
        {
            // Update the frustum infos
            UpdateFrustum(shadowMapRenderer.Camera);

            // Computes the cascade splits
            ComputeCascadeSplits(shadowMapRenderer, ref lightShadowMap);
            var direction = lightShadowMap.LightComponent.Direction;

            // Fake value
            // It will be setup by next loop
            Vector3 side = Vector3.UnitX;
            Vector3 upDirection = Vector3.UnitX;

            // Select best Up vector
            // TODO: User preference?
            foreach (var vectorUp in VectorUps)
            {
                if (Vector3.Dot(direction, vectorUp) < (1.0 - 0.0001))
                {
                    side = Vector3.Normalize(Vector3.Cross(vectorUp, direction));
                    upDirection = Vector3.Normalize(Vector3.Cross(direction, side));
                    break;
                }
            }

            int cascadeCount = lightShadowMap.CascadeCount;

            // Get new shader data from pool
            LightDirectionalShadowMapShaderData shaderData;
            if (cascadeCount == 1)
            {
                shaderData = shaderDataPoolCascade1.Add();
            }
            else if (cascadeCount == 2)
            {
                shaderData = shaderDataPoolCascade2.Add();
            }
            else
            {
                shaderData = shaderDataPoolCascade4.Add();
            }
            lightShadowMap.ShaderData = shaderData;
            shaderData.Texture = lightShadowMap.Atlas.Texture;

            var shadow = lightShadowMap.Shadow;
            // TODO: Min and Max distance can be auto-computed from readback from Z buffer
            var camera = shadowMapRenderer.Camera;
            var shadowCamera = shadowMapRenderer.ShadowCamera;

            // Push a new graphics state
            var graphicsDevice = context.GraphicsDevice;
            graphicsDevice.PushState();

            float splitMaxRatio = shadow.MinDistance;
            for (int cascadeLevel = 0; cascadeLevel < cascadeCount; ++cascadeLevel)
            {
                // Calculate frustum corners for this cascade
                var splitMinRatio = splitMaxRatio;
                splitMaxRatio = cascadeSplitRatios[cascadeLevel];
                for (int j = 0; j < 4; j++)
                {
                    var frustumRange = frustumCorners[j + 4] - frustumCorners[j];
                    cascadeFrustumCornersWS[j] = frustumCorners[j] + frustumRange * splitMinRatio;
                    cascadeFrustumCornersWS[j + 4] = frustumCorners[j] + frustumRange * splitMaxRatio;
                }
                var cascadeBoundWS = BoundingBox.FromPoints(cascadeFrustumCornersWS);

                Vector3 cascadeMinBoundLS;
                Vector3 cascadeMaxBoundLS;

                var target = cascadeBoundWS.Center;

                if (shadow.StabilizationMode == LightShadowMapStabilizationMode.ViewSnapping)
                {
                    // Make sure we are using the same direction when stabilizing
                    //upDirection = Vector3.UnitY;

                    // Compute bounding box center & radius
                    // Note: boundingBox is computed in view space so the computation of the radius is only correct when the view matrix does not do any kind of scale/shear transformation
                    var radius = (cascadeBoundWS.Maximum - cascadeBoundWS.Minimum).Length() * 0.5f;

                    cascadeMaxBoundLS = new Vector3(radius, radius, radius);
                    cascadeMinBoundLS = -cascadeMaxBoundLS;

                    // Snap camera to texel units (so that shadow doesn't jitter when light doesn't change direction but camera is moving)
                    // Technique from ShaderX7 - Practical Cascaded Shadows Maps -  p310-311 
                    var shadowMapHalfSize = lightShadowMap.Size * 0.5f;
                    float x = (float)Math.Ceiling(Vector3.Dot(target, upDirection) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                    float y = (float)Math.Ceiling(Vector3.Dot(target, side) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                    float z = Vector3.Dot(target, direction);
                    //target = up * x + side * y + direction * R32G32B32_Float.Dot(target, direction);
                    target = upDirection * x + side * y + direction * z;
                }
                else
                {
                    upDirection = Vector3.UnitY;
                    // Computes the bouding box of the frustum cascade in light space
                    var lightViewMatrix = Matrix.LookAtLH(cascadeBoundWS.Center, cascadeBoundWS.Center + direction, upDirection);
                    cascadeMinBoundLS = new Vector3(float.MaxValue);
                    cascadeMaxBoundLS = new Vector3(-float.MaxValue);
                    for (int i = 0; i < cascadeFrustumCornersWS.Length; i++)
                    {
                        Vector3 cornerViewSpace;
                        Vector3.TransformCoordinate(ref cascadeFrustumCornersWS[i], ref lightViewMatrix, out cornerViewSpace);

                        cascadeMinBoundLS = Vector3.Min(cascadeMinBoundLS, cornerViewSpace);
                        cascadeMaxBoundLS = Vector3.Max(cascadeMaxBoundLS, cornerViewSpace);
                    }

                    // TODO: Adjust orthoSize by taking into account filtering size
                }

                // Update the shadow camera
                shadowCamera.ViewMatrix = Matrix.LookAtLH(target + direction * cascadeMinBoundLS.Z, target, upDirection); // View;;
                shadowCamera.ProjectionMatrix = Matrix.OrthoOffCenterLH(cascadeMinBoundLS.X, cascadeMaxBoundLS.X, cascadeMinBoundLS.Y, cascadeMaxBoundLS.Y, 0.0f, cascadeMaxBoundLS.Z - cascadeMinBoundLS.Z); // Projection
                shadowCamera.Update();

                // Stabilize the Shadow matrix on the projection
                if (shadow.StabilizationMode == LightShadowMapStabilizationMode.ProjectionSnapping)
                {
                    var shadowPixelPosition = shadowCamera.ViewProjectionMatrix.TranslationVector * lightShadowMap.Size * 0.5f;
                    shadowPixelPosition.Z = 0;
                    var shadowPixelPositionRounded = new Vector3((float)Math.Round(shadowPixelPosition.X), (float)Math.Round(shadowPixelPosition.Y), 0.0f);

                    var shadowPixelOffset = new Vector4(shadowPixelPositionRounded - shadowPixelPosition, 0.0f);
                    shadowPixelOffset *= 2.0f / lightShadowMap.Size;
                    shadowCamera.ProjectionMatrix.Row4 += shadowPixelOffset;
                    shadowCamera.Update();
                }

                // Cascade splits in light space using depth: Store depth on first CascaderCasterMatrix in last column of each row
                shaderData.CascadeSplits[cascadeLevel] = MathUtil.Lerp(camera.NearClipPlane, camera.FarClipPlane, cascadeSplitRatios[cascadeLevel]);

                var shadowMapRectangle = lightShadowMap.GetRectangle(cascadeLevel);

                var cascadeTextureCoords = new Vector4((float)shadowMapRectangle.Left / lightShadowMap.Atlas.Width,
                    (float)shadowMapRectangle.Top / lightShadowMap.Atlas.Height,
                    (float)shadowMapRectangle.Right / lightShadowMap.Atlas.Width,
                    (float)shadowMapRectangle.Bottom / lightShadowMap.Atlas.Height);

                //// Add border (avoid using edges due to bilinear filtering and blur)
                //var borderSizeU = VsmBlurSize / lightShadowMap.Atlas.Width;
                //var borderSizeV = VsmBlurSize / lightShadowMap.Atlas.Height;
                //cascadeTextureCoords.X += borderSizeU;
                //cascadeTextureCoords.Y += borderSizeV;
                //cascadeTextureCoords.Z -= borderSizeU;
                //cascadeTextureCoords.W -= borderSizeV;

                float leftX = (float)lightShadowMap.Size / lightShadowMap.Atlas.Width * 0.5f;
                float leftY = (float)lightShadowMap.Size / lightShadowMap.Atlas.Height * 0.5f;
                float centerX = 0.5f * (cascadeTextureCoords.X + cascadeTextureCoords.Z);
                float centerY = 0.5f * (cascadeTextureCoords.Y + cascadeTextureCoords.W);

                // Compute receiver view proj matrix
                Matrix adjustmentMatrix = Matrix.Scaling(leftX, -leftY, 1.0f) * Matrix.Translation(centerX, centerY, 0.0f);
                // Calculate View Proj matrix from World space to Cascade space
                Matrix.Multiply(ref shadowCamera.ViewProjectionMatrix, ref adjustmentMatrix, out shaderData.WorldToShadowCascadeUV[cascadeLevel]);

                // Render to the atlas
                lightShadowMap.Atlas.RenderFrame.Activate(context);
                graphicsDevice.SetViewport(new Viewport(shadowMapRectangle.X, shadowMapRectangle.Y, shadowMapRectangle.Width, shadowMapRectangle.Height));

                // Render the scene for this cascade
                shadowMapRenderer.RenderCasters(context, lightShadowMap.LightComponent.CullingMask);
                //// Copy texture coords with border
                //cascades[cascadeLevel].CascadeLevels.CascadeTextureCoordsBorder = cascadeTextureCoords;
            }

            graphicsDevice.PopState();
        }

        private void UpdateFrustum(CameraComponent camera)
        {
            // Compute frustum-dependent variables (common for all shadow maps)
            Matrix projectionToWorld;
            Matrix.Invert(ref camera.ViewProjectionMatrix, out projectionToWorld);

            // Transform Frustum corners in World Space (8 points) - algorithm is valid only if the view matrix does not do any kind of scale/shear transformation
            for (int i = 0; i < 8; ++i)
            {
                Vector3.TransformCoordinate(ref FrustumBasePoints[i], ref projectionToWorld, out frustumCorners[i]);
            }
        }

        private void ComputeCascadeSplits(ShadowMapRenderer shadowContext, ref LightShadowMapTexture lightShadowMap)
        {
            var shadow = lightShadowMap.Shadow;

            // TODO: Min and Max distance can be auto-computed from readback from Z buffer
            var minDistance = shadow.MinDistance;
            var maxDistance = shadow.MaxDistance;

            if (shadow.SplitMode == LightShadowMapSplitMode.Logarithmic || shadow.SplitMode == LightShadowMapSplitMode.PSSM)
            {
                var nearClip = shadowContext.Camera.NearClipPlane;
                var farClip = shadowContext.Camera.FarClipPlane;
                var rangeClip = farClip - nearClip;

                var minZ = nearClip + minDistance * rangeClip;
                var maxZ = nearClip + maxDistance * rangeClip;

                var range = maxZ - minZ;
                var ratio = maxZ / minZ;
                var logRatio = shadow.SplitMode == LightShadowMapSplitMode.Logarithmic ? 1.0f : 0.0f;

                for (int cascadeLevel = 0; cascadeLevel < lightShadowMap.CascadeCount; ++cascadeLevel)
                {
                    // Compute cascade split (between znear and zfar)
                    float distrib = (float)(cascadeLevel + 1) / lightShadowMap.CascadeCount;
                    float logZ = (float)(minZ * Math.Pow(ratio, distrib));
                    float uniformZ = minZ + range * distrib;
                    float distance = MathUtil.Lerp(uniformZ, logZ, logRatio);
                    cascadeSplitRatios[cascadeLevel] = (distance - nearClip) / rangeClip;  // Normalize cascade splits to [0,1]
                }
            }
            else
            {
                if (lightShadowMap.CascadeCount == 1)
                {
                    cascadeSplitRatios[0] = minDistance + shadow.SplitDistance1 * maxDistance;
                }
                else if (lightShadowMap.CascadeCount == 2)
                {
                    cascadeSplitRatios[0] = minDistance + shadow.SplitDistance1 * maxDistance;
                    cascadeSplitRatios[1] = minDistance + shadow.SplitDistance3 * maxDistance;
                }
                else if (lightShadowMap.CascadeCount == 4)
                {
                    cascadeSplitRatios[0] = minDistance + shadow.SplitDistance0 * maxDistance;
                    cascadeSplitRatios[1] = minDistance + shadow.SplitDistance1 * maxDistance;
                    cascadeSplitRatios[2] = minDistance + shadow.SplitDistance2 * maxDistance;
                    cascadeSplitRatios[3] = minDistance + shadow.SplitDistance3 * maxDistance;
                }
            }
        }

        private class LightDirectionalShadowMapShaderData : ILightShadowMapShaderData
        {
            public LightDirectionalShadowMapShaderData(int cascadeCount)
            {
                CascadeSplits = new float[cascadeCount];
                WorldToShadowCascadeUV = new Matrix[cascadeCount];
            }

            public Texture Texture;

            public readonly float[] CascadeSplits;

            public readonly Matrix[] WorldToShadowCascadeUV;
        }

        private class LightDirectionalShadowMapGroupShaderData : ILightShadowMapShaderGroupData
        {
            private const string ShaderName = "ShadowMapCascade";

            private readonly LightShadowType shadowType;

            private readonly int cascadeCount;

            private readonly float[] cascadeSplits;

            private readonly Matrix[] worldToShadowCascadeUV;

            private readonly ShaderMixinSource shadowShader;

            private readonly ParameterKey<Texture> shadowMapTextureKey;

            private readonly ParameterKey<float[]> cascadeSplitsKey;

            private readonly ParameterKey<Matrix[]> worldToShadowCascadeUVsKey;

            private Texture shadowMapTexture;

            /// <summary>
            /// Initializes a new instance of the <see cref="LightDirectionalShadowMapGroupShaderData" /> class.
            /// </summary>
            /// <param name="compositionKey">The composition key.</param>
            /// <param name="shadowType">Type of the shadow.</param>
            /// <param name="lightCountMax">The light count maximum.</param>
            public LightDirectionalShadowMapGroupShaderData(string compositionKey, LightShadowType shadowType, int lightCountMax)
            {
                this.shadowType = shadowType;
                this.cascadeCount = 1 << ((int)(shadowType & LightShadowType.CascadeMask) - 1);
                cascadeSplits = new float[cascadeCount * lightCountMax];
                worldToShadowCascadeUV = new Matrix[cascadeCount * lightCountMax];

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource(ShaderName, cascadeCount, lightCountMax, (this.shadowType & LightShadowType.BlendCascade) != 0, (this.shadowType & LightShadowType.Debug) != 0));
                // TODO: Temporary passing filter here
                mixin.Mixins.Add(new ShaderClassSource("ShadowMapFilterDefault"));

                shadowShader = mixin;
                shadowMapTextureKey = ShadowMapKeys.Texture.ComposeWith(compositionKey);
                cascadeSplitsKey = ShadowMapCascadeKeys.CascadeDepthSplits.ComposeWith(compositionKey);
                worldToShadowCascadeUVsKey = ShadowMapCascadeKeys.WorldToShadowCascadeUV.ComposeWith(compositionKey);
            }

            public void ApplyShader(ShaderMixinSource mixin)
            {
                mixin.CloneFrom(shadowShader);
            }

            public void SetShadowMapShaderData(int index, ILightShadowMapShaderData shaderData)
            {
                var singleLightData = (LightDirectionalShadowMapShaderData)shaderData;
                var splits = singleLightData.CascadeSplits;
                var matrices = singleLightData.WorldToShadowCascadeUV;
                int splitIndex = index * cascadeCount;
                for (int i = 0; i < splits.Length; i++)
                {
                    cascadeSplits[splitIndex + i] = splits[i];
                    worldToShadowCascadeUV[splitIndex + i] = matrices[i];
                }

                // TODO: should be setup just once at creation time
                if (index == 0)
                {
                    shadowMapTexture = singleLightData.Texture;
                }
            }

            public void ApplyParameters(ParameterCollection parameters)
            {
                parameters.Set(shadowMapTextureKey, shadowMapTexture);
                parameters.Set(cascadeSplitsKey, cascadeSplits);
                parameters.Set(worldToShadowCascadeUVsKey, worldToShadowCascadeUV);
            }
        }

        private static LightDirectionalShadowMapShaderData CreateLightDirectionalShadowMapShaderDataCascade1()
        {
            return new LightDirectionalShadowMapShaderData(1);
        }

        private static LightDirectionalShadowMapShaderData CreateLightDirectionalShadowMapShaderDataCascade2()
        {
            return new LightDirectionalShadowMapShaderData(2);
        }

        private static LightDirectionalShadowMapShaderData CreateLightDirectionalShadowMapShaderDataCascade4()
        {
            return new LightDirectionalShadowMapShaderData(4);
        }
    }
}