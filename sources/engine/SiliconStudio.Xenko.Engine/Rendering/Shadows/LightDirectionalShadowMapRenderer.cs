// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    /// <summary>
    /// Renders a shadow map from a directional light.
    /// </summary>
    public class LightDirectionalShadowMapRenderer : LightShadowMapRendererBase
    {
        /// <summary>
        /// The various UP vectors to try.
        /// </summary>
        private static readonly Vector3[] VectorUps = { Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ };

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
        private readonly Vector3[] cascadeFrustumCornersVS;
        private readonly Vector3[] frustumCornersWS;
        private readonly Vector3[] frustumCornersVS;

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
            cascadeFrustumCornersVS = new Vector3[8];
            frustumCornersWS = new Vector3[8];
            frustumCornersVS = new Vector3[8];
            shaderDataPoolCascade1 = new PoolListStruct<LightDirectionalShadowMapShaderData>(4, CreateLightDirectionalShadowMapShaderDataCascade1);
            shaderDataPoolCascade2 = new PoolListStruct<LightDirectionalShadowMapShaderData>(4, CreateLightDirectionalShadowMapShaderDataCascade2);
            shaderDataPoolCascade4 = new PoolListStruct<LightDirectionalShadowMapShaderData>(4, CreateLightDirectionalShadowMapShaderDataCascade4);
        }
        
        public override void Reset()
        {
            shaderDataPoolCascade1.Clear();
            shaderDataPoolCascade2.Clear();
            shaderDataPoolCascade4.Clear();
        }

        public override LightShadowType GetShadowType(LightShadowMap shadowMapArg)
        {
            var shadowMap = (LightDirectionalShadowMap)shadowMapArg;

            var shadowType = base.GetShadowType(shadowMapArg);

            if (shadowMap.DepthRange.IsAutomatic)
            {
                shadowType |= LightShadowType.DepthRangeAuto;
            }
            else if (shadowMap.DepthRange.IsBlendingCascades)
            {
                shadowType |= LightShadowType.BlendCascade;
            }

            return shadowType;
        }

        public override ILightShadowMapShaderGroupData CreateShaderGroupData(LightShadowType shadowType)
        {
            return new LightDirectionalShadowMapGroupShaderData(shadowType);
        }

        public override void Collect(RenderContext context, ShadowMapRenderer shadowMapRenderer, LightShadowMapTexture lightShadowMap)
        {
            var shadow = (LightDirectionalShadowMap)lightShadowMap.Shadow;
            // TODO: Min and Max distance can be auto-computed from readback from Z buffer
            var shadowRenderView = shadowMapRenderer.CurrentView;

            var viewToWorld = shadowRenderView.View;
            viewToWorld.Invert();

            // Update the frustum infos
            UpdateFrustum(shadowRenderView);

            // Computes the cascade splits
            var minMaxDistance = ComputeCascadeSplits(context, shadowMapRenderer, ref lightShadowMap);
            var direction = lightShadowMap.LightComponent.Direction;

            // Fake value
            // It will be setup by next loop
            Vector3 side = Vector3.UnitX;
            Vector3 upDirection = Vector3.UnitX;

            // Select best Up vector
            // TODO: User preference?
            foreach (var vectorUp in VectorUps)
            {
                if (Math.Abs(Vector3.Dot(direction, vectorUp)) < (1.0 - 0.0001))
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
            shaderData.DepthBias = shadow.BiasParameters.DepthBias;
            shaderData.OffsetScale = shadow.BiasParameters.NormalOffsetScale;

            float splitMaxRatio = (minMaxDistance.X - shadowRenderView.NearClipPlane) / (shadowRenderView.FarClipPlane - shadowRenderView.NearClipPlane);
            for (int cascadeLevel = 0; cascadeLevel < cascadeCount; ++cascadeLevel)
            {
                // Calculate frustum corners for this cascade
                var splitMinRatio = splitMaxRatio;
                splitMaxRatio = cascadeSplitRatios[cascadeLevel];
                for (int j = 0; j < 4; j++)
                {
                    // Calculate frustum in WS and VS
                    var frustumRange = frustumCornersWS[j + 4] - frustumCornersWS[j];
                    cascadeFrustumCornersWS[j] = frustumCornersWS[j] + frustumRange * splitMinRatio;
                    cascadeFrustumCornersWS[j + 4] = frustumCornersWS[j] + frustumRange * splitMaxRatio;

                    frustumRange = frustumCornersVS[j + 4] - frustumCornersVS[j];
                    cascadeFrustumCornersVS[j] = frustumCornersVS[j] + frustumRange * splitMinRatio;
                    cascadeFrustumCornersVS[j + 4] = frustumCornersVS[j] + frustumRange * splitMaxRatio;
                }

                Vector3 cascadeMinBoundLS;
                Vector3 cascadeMaxBoundLS;
                Vector3 target;

                if (!shadow.DepthRange.IsAutomatic && (shadow.StabilizationMode == LightShadowMapStabilizationMode.ViewSnapping || shadow.StabilizationMode == LightShadowMapStabilizationMode.ProjectionSnapping))
                {
                    // Make sure we are using the same direction when stabilizing
                    var boundingVS = BoundingSphere.FromPoints(cascadeFrustumCornersVS);

                    // Compute bounding box center & radius
                    target = Vector3.TransformCoordinate(boundingVS.Center, viewToWorld);
                    var radius = boundingVS.Radius;

                    //if (shadow.AutoComputeMinMax)
                    //{
                    //    var snapRadius = (float)Math.Ceiling(radius / snapRadiusValue) * snapRadiusValue;
                    //    Debug.WriteLine("Radius: {0} SnapRadius: {1} (snap: {2})", radius, snapRadius, snapRadiusValue);
                    //    radius = snapRadius;
                    //}

                    cascadeMaxBoundLS = new Vector3(radius, radius, radius);
                    cascadeMinBoundLS = -cascadeMaxBoundLS;

                    if (shadow.StabilizationMode == LightShadowMapStabilizationMode.ViewSnapping)
                    {
                        // Snap camera to texel units (so that shadow doesn't jitter when light doesn't change direction but camera is moving)
                        // Technique from ShaderX7 - Practical Cascaded Shadows Maps -  p310-311 
                        var shadowMapHalfSize = lightShadowMap.Size * 0.5f;
                        float x = (float)Math.Ceiling(Vector3.Dot(target, upDirection) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                        float y = (float)Math.Ceiling(Vector3.Dot(target, side) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                        float z = Vector3.Dot(target, direction);

                        //target = up * x + side * y + direction * R32G32B32_Float.Dot(target, direction);
                        target = upDirection * x + side * y + direction * z;
                    }
                }
                else
                {
                    var cascadeBoundWS = BoundingBox.FromPoints(cascadeFrustumCornersWS);
                    target = cascadeBoundWS.Center;

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
                var viewMatrix = Matrix.LookAtLH(target + direction * cascadeMinBoundLS.Z, target, upDirection); // View;;
                var projectionMatrix = Matrix.OrthoOffCenterLH(cascadeMinBoundLS.X, cascadeMaxBoundLS.X, cascadeMinBoundLS.Y, cascadeMaxBoundLS.Y, 0.0f, cascadeMaxBoundLS.Z - cascadeMinBoundLS.Z); // Projection
                Matrix viewProjectionMatrix;
                Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjectionMatrix);

                // Stabilize the Shadow matrix on the projection
                if (shadow.StabilizationMode == LightShadowMapStabilizationMode.ProjectionSnapping)
                {
                    var shadowPixelPosition = viewProjectionMatrix.TranslationVector * lightShadowMap.Size * 0.5f;
                    shadowPixelPosition.Z = 0;
                    var shadowPixelPositionRounded = new Vector3((float)Math.Round(shadowPixelPosition.X), (float)Math.Round(shadowPixelPosition.Y), 0.0f);

                    var shadowPixelOffset = new Vector4(shadowPixelPositionRounded - shadowPixelPosition, 0.0f);
                    shadowPixelOffset *= 2.0f / lightShadowMap.Size;
                    projectionMatrix.Row4 += shadowPixelOffset;
                    Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjectionMatrix);
                }

                shaderData.ViewMatrix[cascadeLevel] = viewMatrix;
                shaderData.ProjectionMatrix[cascadeLevel] = projectionMatrix;

                // Cascade splits in light space using depth: Store depth on first CascaderCasterMatrix in last column of each row
                shaderData.CascadeSplits[cascadeLevel] = MathUtil.Lerp(shadowRenderView.NearClipPlane, shadowRenderView.FarClipPlane, cascadeSplitRatios[cascadeLevel]);

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
                Matrix.Multiply(ref viewProjectionMatrix, ref adjustmentMatrix, out shaderData.WorldToShadowCascadeUV[cascadeLevel]);
            }
        }

        public override void GetCascadeViewParameters(LightShadowMapTexture shadowMapTexture, int cascadeIndex, out Matrix view, out Matrix projection)
        {
            var shaderData = (LightDirectionalShadowMapShaderData)shadowMapTexture.ShaderData;
            view = shaderData.ViewMatrix[cascadeIndex];
            projection = shaderData.ProjectionMatrix[cascadeIndex];
        }

        private void UpdateFrustum(RenderView renderView)
        {
            var projectionToView = renderView.Projection;
            projectionToView.Invert();

            // Compute frustum-dependent variables (common for all shadow maps)
            var projectionToWorld = renderView.ViewProjection;
            projectionToWorld.Invert();

            // Transform Frustum corners in World Space (8 points) - algorithm is valid only if the view matrix does not do any kind of scale/shear transformation
            for (int i = 0; i < 8; ++i)
            {
                Vector3.TransformCoordinate(ref FrustumBasePoints[i], ref projectionToWorld, out frustumCornersWS[i]);
                Vector3.TransformCoordinate(ref FrustumBasePoints[i], ref projectionToView, out frustumCornersVS[i]);
            }
        }

        private static float ToLinearDepth(float depthZ, ref Matrix projectionMatrix)
        {
            // as projection matrix is RH we calculate it like this
            var denominator = (depthZ + projectionMatrix.M33);
            return projectionMatrix.M43 / denominator;
        }

        private Vector2 ComputeCascadeSplits(RenderContext context, ShadowMapRenderer shadowContext, ref LightShadowMapTexture lightShadowMap)
        {
            var shadow = (LightDirectionalShadowMap)lightShadowMap.Shadow;
            var shadowRenderView = shadowContext.CurrentView;

            var cameraNear = shadowRenderView.NearClipPlane;
            var cameraFar = shadowRenderView.FarClipPlane;
            var cameraRange = cameraFar - cameraNear;

            var minDistance = cameraNear + LightDirectionalShadowMap.DepthRangeParameters.DefaultMinDistance;
            var maxDistance = cameraNear + LightDirectionalShadowMap.DepthRangeParameters.DefaultMaxDistance;

            if (shadow.DepthRange.IsAutomatic)
            {
                //var depthReadBack = DepthReadback.GetDepthReadback(context);
                //if (depthReadBack.IsResultAvailable)
                //{
                //    var depthMinMax = depthReadBack.DepthMinMax;
                    
                //    minDistance = ToLinearDepth(depthMinMax.X, ref camera.ProjectionMatrix);
                //    // Reserve 1/3 of the guard distance for the min distance
                //    minDistance = Math.Max(cameraNear, minDistance - shadow.DepthRange.GuardDistance / 3);

                //    // Reserve 2/3 of the guard distance for the max distance
                //    var guardMaxDistance = minDistance + shadow.DepthRange.GuardDistance * 2 / 3;
                //    maxDistance = ToLinearDepth(depthMinMax.Y, ref camera.ProjectionMatrix);
                //    maxDistance = Math.Max(maxDistance, guardMaxDistance);
                //}

                // Reserve 1/3 of the guard distance for the min distance
                minDistance = Math.Max(cameraNear, shadowContext.CurrentView.MinimumDistance - shadow.DepthRange.GuardDistance / 3);

                // Reserve 2/3 of the guard distance for the max distance
                var guardMaxDistance = minDistance + shadow.DepthRange.GuardDistance * 2 / 3;
                maxDistance = Math.Max(shadowContext.CurrentView.MaximumDistance, guardMaxDistance);
            }
            else
            {
                minDistance = cameraNear + shadow.DepthRange.ManualMinDistance;
                maxDistance = cameraNear + shadow.DepthRange.ManualMaxDistance;
            }

            var manualPartitionMode = shadow.PartitionMode as LightDirectionalShadowMap.PartitionManual;
            var logarithmicPartitionMode = shadow.PartitionMode as LightDirectionalShadowMap.PartitionLogarithmic;
            if (logarithmicPartitionMode != null)
            {
                var minZ = minDistance;
                var maxZ = maxDistance;

                var range = maxZ - minZ;
                var ratio = maxZ / minZ;
                var logRatio = MathUtil.Clamp(1.0f - logarithmicPartitionMode.PSSMFactor, 0.0f, 1.0f);

                for (int cascadeLevel = 0; cascadeLevel < lightShadowMap.CascadeCount; ++cascadeLevel)
                {
                    // Compute cascade split (between znear and zfar)
                    float distrib = (float)(cascadeLevel + 1) / lightShadowMap.CascadeCount;
                    float logZ = (float)(minZ * Math.Pow(ratio, distrib));
                    float uniformZ = minZ + range * distrib;
                    float distance = MathUtil.Lerp(uniformZ, logZ, logRatio);
                    cascadeSplitRatios[cascadeLevel] = distance;
                }
            }
            else if (manualPartitionMode != null)
            {
                if (lightShadowMap.CascadeCount == 1)
                {
                    cascadeSplitRatios[0] = minDistance + manualPartitionMode.SplitDistance1 * maxDistance;
                }
                else if (lightShadowMap.CascadeCount == 2)
                {
                    cascadeSplitRatios[0] = minDistance + manualPartitionMode.SplitDistance1 * maxDistance;
                    cascadeSplitRatios[1] = minDistance + manualPartitionMode.SplitDistance3 * maxDistance;
                }
                else if (lightShadowMap.CascadeCount == 4)
                {
                    cascadeSplitRatios[0] = minDistance + manualPartitionMode.SplitDistance0 * maxDistance;
                    cascadeSplitRatios[1] = minDistance + manualPartitionMode.SplitDistance1 * maxDistance;
                    cascadeSplitRatios[2] = minDistance + manualPartitionMode.SplitDistance2 * maxDistance;
                    cascadeSplitRatios[3] = minDistance + manualPartitionMode.SplitDistance3 * maxDistance;
                }
            }

            // Convert distance splits to ratios cascade in the range [0, 1]
            for (int i = 0; i < cascadeSplitRatios.Length; i++)
            {
                cascadeSplitRatios[i] = (cascadeSplitRatios[i] - cameraNear) / cameraRange;
            }

            return new Vector2(minDistance, maxDistance);
        }

        private class DepthReadback : RendererBase
        {
            public static DepthReadback GetDepthReadback(RenderContext context)
            {
                var sceneCameraRenderer = context.Tags.Get(SceneCameraRenderer.Current);
                DepthReadback depthReadBack;
                for (int i = 0; i < sceneCameraRenderer.PostRenderers.Count; i++)
                {
                    depthReadBack = sceneCameraRenderer.PostRenderers[i] as DepthReadback;
                    if (depthReadBack != null)
                    {
                        return depthReadBack;
                    }
                }

                depthReadBack = new DepthReadback();
                sceneCameraRenderer.PostRenderers.Add(depthReadBack);
                return depthReadBack;
            }

            private DepthMinMax minMax;

            protected override void InitializeCore()
            {
                base.InitializeCore();

                minMax = ToLoadAndUnload(new DepthMinMax());
            }

            public bool IsResultAvailable { get; private set; }

            public Vector2 DepthMinMax { get; private set; }

            protected override void DrawCore(RenderDrawContext context)
            {
                try
                {
                    context.PushRenderTargets();
                    minMax.SetInput(context.CommandList.DepthStencilBuffer);
                    ((RendererBase)minMax).Draw(context);

                    IsResultAvailable = minMax.IsResultAvailable;
                    if (IsResultAvailable)
                    {
                        DepthMinMax = minMax.Result;
                    }
                }
                finally 
                {
                    context.PopRenderTargets();
                }
            }
        }

        private class LightDirectionalShadowMapShaderData : ILightShadowMapShaderData
        {
            public LightDirectionalShadowMapShaderData(int cascadeCount)
            {
                CascadeSplits = new float[cascadeCount];
                WorldToShadowCascadeUV = new Matrix[cascadeCount];
                ViewMatrix = new Matrix[cascadeCount];
                ProjectionMatrix = new Matrix[cascadeCount];
            }

            public Texture Texture;

            public readonly float[] CascadeSplits;

            public float DepthBias;

            public float OffsetScale;

            public readonly Matrix[] WorldToShadowCascadeUV;

            public readonly Matrix[] ViewMatrix;

            public readonly Matrix[] ProjectionMatrix;
        }

        private class LightDirectionalShadowMapGroupShaderData : ILightShadowMapShaderGroupData
        {
            private const string ShaderName = "ShadowMapReceiverDirectional";

            private readonly LightShadowType shadowType;

            private readonly int cascadeCount;

            private float[] cascadeSplits;

            private Matrix[] worldToShadowCascadeUV;

            private float[] depthBiases;

            private float[] offsetScales;

            private Texture shadowMapTexture;

            private Vector2 shadowMapTextureSize;

            private Vector2 shadowMapTextureTexelSize;

            private ShaderMixinSource shadowShader;

            private ObjectParameterKey<Texture> shadowMapTextureKey;

            private ValueParameterKey<float> cascadeSplitsKey;

            private ValueParameterKey<Matrix> worldToShadowCascadeUVsKey;

            private ValueParameterKey<float> depthBiasesKey;

            private ValueParameterKey<float> offsetScalesKey;

            private ValueParameterKey<Vector2> shadowMapTextureSizeKey;

            private ValueParameterKey<Vector2> shadowMapTextureTexelSizeKey;

            /// <summary>
            /// Initializes a new instance of the <see cref="LightDirectionalShadowMapGroupShaderData" /> class.
            /// </summary>
            /// <param name="shadowType">Type of the shadow.</param>
            /// <param name="lightCountMax">The light count maximum.</param>
            public LightDirectionalShadowMapGroupShaderData(LightShadowType shadowType)
            {
                this.shadowType = shadowType;
                this.cascadeCount = 1 << ((int)(shadowType & LightShadowType.CascadeMask) - 1);
            }

            public void UpdateLayout(string compositionKey)
            {
                shadowMapTextureKey = ShadowMapKeys.Texture.ComposeWith(compositionKey);
                shadowMapTextureSizeKey = ShadowMapKeys.TextureSize.ComposeWith(compositionKey);
                shadowMapTextureTexelSizeKey = ShadowMapKeys.TextureTexelSize.ComposeWith(compositionKey);
                cascadeSplitsKey = ShadowMapReceiverDirectionalKeys.CascadeDepthSplits.ComposeWith(compositionKey);
                worldToShadowCascadeUVsKey = ShadowMapReceiverBaseKeys.WorldToShadowCascadeUV.ComposeWith(compositionKey);
                depthBiasesKey = ShadowMapReceiverBaseKeys.DepthBiases.ComposeWith(compositionKey);
                offsetScalesKey = ShadowMapReceiverBaseKeys.OffsetScales.ComposeWith(compositionKey);
            }

            public void UpdateLightCount(int lightLastCount, int lightCurrentCount)
            {
                shadowShader = new ShaderMixinSource();
                var isDepthRangeAuto = (this.shadowType & LightShadowType.DepthRangeAuto) != 0;
                shadowShader.Mixins.Add(new ShaderClassSource(ShaderName, cascadeCount, lightCurrentCount, (this.shadowType & LightShadowType.BlendCascade) != 0 && !isDepthRangeAuto, isDepthRangeAuto, (this.shadowType & LightShadowType.Debug) != 0));
                // TODO: Temporary passing filter here

                switch (shadowType & LightShadowType.FilterMask)
                {
                    case LightShadowType.PCF3x3:
                        shadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerView.Lighting", 3));
                        break;
                    case LightShadowType.PCF5x5:
                        shadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerView.Lighting", 5));
                        break;
                    case LightShadowType.PCF7x7:
                        shadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerView.Lighting", 7));
                        break;
                    default:
                        shadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterDefault", "PerView.Lighting"));
                        break;
                }

                Array.Resize(ref cascadeSplits, cascadeCount * lightCurrentCount);
                Array.Resize(ref worldToShadowCascadeUV, cascadeCount * lightCurrentCount);
                Array.Resize(ref depthBiases, lightCurrentCount);
                Array.Resize(ref offsetScales, lightCurrentCount);
            }

            public void ApplyShader(ShaderMixinSource mixin)
            {
                mixin.CloneFrom(shadowShader);
            }

            public void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, FastListStruct<LightDynamicEntry> currentLights)
            {
                for (int lightIndex = 0; lightIndex < currentLights.Count; ++lightIndex)
                {
                    var lightEntry = currentLights[lightIndex];

                    var singleLightData = (LightDirectionalShadowMapShaderData)lightEntry.ShadowMapTexture.ShaderData;
                    var splits = singleLightData.CascadeSplits;
                    var matrices = singleLightData.WorldToShadowCascadeUV;
                    int splitIndex = lightIndex * cascadeCount;
                    for (int i = 0; i < splits.Length; i++)
                    {
                        cascadeSplits[splitIndex + i] = splits[i];
                        worldToShadowCascadeUV[splitIndex + i] = matrices[i];
                    }

                    depthBiases[lightIndex] = singleLightData.DepthBias;
                    offsetScales[lightIndex] = singleLightData.OffsetScale;

                    // TODO: should be setup just once at creation time
                    if (lightIndex == 0)
                    {
                        shadowMapTexture = singleLightData.Texture;
                        if (shadowMapTexture != null)
                        {
                            shadowMapTextureSize = new Vector2(shadowMapTexture.Width, shadowMapTexture.Height);
                            shadowMapTextureTexelSize = 1.0f/shadowMapTextureSize;
                        }
                    }
                }

                parameters.Set(shadowMapTextureKey, shadowMapTexture);
                parameters.Set(shadowMapTextureSizeKey, shadowMapTextureSize);
                parameters.Set(shadowMapTextureTexelSizeKey, shadowMapTextureTexelSize);
                parameters.Set(cascadeSplitsKey, cascadeSplits);
                parameters.Set(worldToShadowCascadeUVsKey, worldToShadowCascadeUV);
                parameters.Set(depthBiasesKey, depthBiases);
                parameters.Set(offsetScalesKey, offsetScales);
            }

            public void ApplyDrawParameters(RenderDrawContext context, ParameterCollection parameters, FastListStruct<LightDynamicEntry> currentLights, ref BoundingBoxExt boundingBox)
            {
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