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
        private readonly Vector3[] cascadeFrustumCorners;
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
            cascadeFrustumCorners = new Vector3[8];
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
            var cascadeCount = 1 << ((int)(shadowType & LightShadowType.CascadeMask) - 1);
            return new LightDirectionalShadowMapGroupShaderData(compositionKey, cascadeCount, maxLightCount, (shadowType & LightShadowType.Debug) != 0);
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
                // Compute caster view and projection matrices
                var shadowMapView = Matrix.Zero;
                var shadowMapProjection = Matrix.Zero;

                // Calculate frustum corners for this cascade
                var splitMinRatio = splitMaxRatio;
                splitMaxRatio = cascadeSplitRatios[cascadeLevel];
                for (int j = 0; j < 4; j++)
                {
                    var frustumRange = frustumCorners[j + 4] - frustumCorners[j];
                    cascadeFrustumCorners[j] = frustumCorners[j] + frustumRange * splitMinRatio;
                    cascadeFrustumCorners[j + 4] = frustumCorners[j] + frustumRange * splitMaxRatio;
                }
                var cascadeBoundWS = BoundingBox.FromPoints(cascadeFrustumCorners);

                Vector3 cascadeMinBoundLS;
                Vector3 cascadeMaxBoundLS;

                var target = cascadeBoundWS.Center;

                if (shadow.Stabilized)
                {
                    // Compute bounding box center & radius
                    // Note: boundingBox is computed in view space so the computation of the radius is only correct when the view matrix does not do any kind of scale/shear transformation
                    var radius = (cascadeBoundWS.Maximum - cascadeBoundWS.Minimum).Length() * 0.5f;

                    cascadeMaxBoundLS = new Vector3(radius, radius, radius);
                    cascadeMinBoundLS = -cascadeMaxBoundLS;

                    // Make sure we are using the same direction when stabilizing
                    upDirection = shadowMapRenderer.Camera.ViewMatrix.Right;

                    // Snap camera to texel units (so that shadow doesn't jitter when light doesn't change direction but camera is moving)
                    var shadowMapHalfSize = lightShadowMap.Size * 0.5f;
                    float x = (float)Math.Ceiling(Vector3.Dot(target, upDirection) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                    float y = (float)Math.Ceiling(Vector3.Dot(target, side) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                    float z = Vector3.Dot(target, direction);
                    //target = up * x + side * y + direction * R32G32B32_Float.Dot(target, direction);
                    target = upDirection * x + side * y + direction * z;
                }
                else
                {
                    // Computes the bouding box of the frustum cascade in light space
                    var lightViewMatrix = Matrix.LookAtLH(cascadeBoundWS.Center, cascadeBoundWS.Center + direction, upDirection);
                    cascadeMinBoundLS = new Vector3(float.MaxValue);
                    cascadeMaxBoundLS = new Vector3(-float.MaxValue);
                    for (int i = 0; i < cascadeFrustumCorners.Length; i++)
                    {
                        Vector3 cornerViewSpace;
                        Vector3.TransformCoordinate(ref cascadeFrustumCorners[i], ref lightViewMatrix, out cornerViewSpace);

                        cascadeMinBoundLS = Vector3.Min(cascadeMinBoundLS, cornerViewSpace);
                        cascadeMaxBoundLS = Vector3.Max(cascadeMaxBoundLS, cornerViewSpace);
                    }

                    // TODO: Adjust orthoSize by taking into account filtering size
                }

                // Compute caster view and projection matrices
                shadowMapView = Matrix.LookAtLH(target + direction * cascadeMinBoundLS.Z, target, upDirection); // View;
                shadowMapProjection = Matrix.OrthoOffCenterLH(cascadeMinBoundLS.X, cascadeMaxBoundLS.X, cascadeMinBoundLS.Y, cascadeMaxBoundLS.Y, 0.0f, cascadeMaxBoundLS.Z - cascadeMinBoundLS.Z); // Projection

                // Update the shadow camera
                shadowCamera.ViewMatrix = shadowMapView;
                shadowCamera.ProjectionMatrix = shadowMapProjection;
                shadowCamera.Update();

                // Calculate View Proj matrix from World space to Cascade space
                var cascadeShadowMatrix = shadowCamera.ViewProjectionMatrix;

                // Cascade splits in light space using depth: Store depth on first CascaderCasterMatrix in last column of each row
                shaderData.CascadeSplits[cascadeLevel] = camera.NearClipPlane + cascadeSplitRatios[cascadeLevel] * (camera.FarClipPlane - camera.NearClipPlane);

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
                Matrix.Multiply(ref cascadeShadowMatrix, ref adjustmentMatrix, out shaderData.WorldToShadowCascadeUV[cascadeLevel]);

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

            public readonly float[] CascadeSplits;

            public readonly Matrix[] WorldToShadowCascadeUV;
        }

        private class LightDirectionalShadowMapGroupShaderData : ILightShadowMapShaderGroupData
        {
            private const string ShaderName = "ShadowMapCascade";

            private readonly int cascadeCount;

            private readonly bool isDebug;

            private readonly float[] cascadeSplits;

            private readonly Matrix[] worldToShadowCascadeUV;

            private readonly ShaderClassSource shadowShader;

            private readonly ParameterKey<float[]> cascadeSplitsKey;

            private readonly ParameterKey<Matrix[]> worldToShadowCascadeUVsKey;

            /// <summary>
            /// Initializes a new instance of the <see cref="LightDirectionalShadowMapGroupShaderData" /> class.
            /// </summary>
            /// <param name="compositionKey">The composition key.</param>
            /// <param name="indexInComposition">The index in composition.</param>
            /// <param name="cascadeCount">The cascade count.</param>
            /// <param name="lightCountMax">The light count maximum.</param>
            /// <param name="isDebug">if set to <c>true</c> [is debug].</param>
            public LightDirectionalShadowMapGroupShaderData(string compositionKey, int cascadeCount, int lightCountMax, bool isDebug)
            {
                this.cascadeCount = cascadeCount;
                this.isDebug = isDebug;
                cascadeSplits = new float[cascadeCount * lightCountMax];
                worldToShadowCascadeUV = new Matrix[cascadeCount * lightCountMax];
                shadowShader = new ShaderClassSource(ShaderName, cascadeCount, lightCountMax, isDebug);
                cascadeSplitsKey = ShadowMapCascadeKeys.CascadeDepthSplits.ComposeWith(compositionKey);
                worldToShadowCascadeUVsKey = ShadowMapCascadeKeys.WorldToShadowCascadeUV.ComposeWith(compositionKey);
            }

            public void ApplyShader(ShaderMixinSource mixin)
            {
                mixin.Mixins.Add(shadowShader);
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
            }

            public void ApplyParameters(ParameterCollection parameters)
            {
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