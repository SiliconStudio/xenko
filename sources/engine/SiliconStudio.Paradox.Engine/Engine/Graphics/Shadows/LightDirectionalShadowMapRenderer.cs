// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Graphics;

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

        public LightDirectionalShadowMapRenderer()
        {
            WorldToShadowCascadeUV = new Matrix[4];
            CascadeSplitRatios = new float[4];
            CascadeSplits = new Vector4();
            CascadeFrustumCorners = new Vector3[8];
        }

        private readonly Matrix[] WorldToShadowCascadeUV;

        private readonly float[] CascadeSplitRatios;

        private Vector4 CascadeSplits;

        private readonly Vector3[] CascadeFrustumCorners;

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
                    CascadeSplitRatios[cascadeLevel] = (distance - nearClip) / rangeClip;  // Normalize cascade splits to [0,1]
                }
            }
            else
            {
                if (lightShadowMap.CascadeCount == 1)
                {
                    CascadeSplitRatios[0] = minDistance + shadow.SplitDistance1 * maxDistance;
                }
                else if (lightShadowMap.CascadeCount == 2)
                {
                    CascadeSplitRatios[0] = minDistance + shadow.SplitDistance1 * maxDistance;
                    CascadeSplitRatios[1] = minDistance + shadow.SplitDistance3 * maxDistance;
                }
                else if (lightShadowMap.CascadeCount == 4)
                {
                    CascadeSplitRatios[0] = minDistance + shadow.SplitDistance0 * maxDistance;
                    CascadeSplitRatios[1] = minDistance + shadow.SplitDistance1 * maxDistance;
                    CascadeSplitRatios[2] = minDistance + shadow.SplitDistance2 * maxDistance;
                    CascadeSplitRatios[3] = minDistance + shadow.SplitDistance3 * maxDistance;
                }
            }
        }

        public void Render(RenderContext context, ShadowMapRenderer shadowMapRenderer, ref LightShadowMapTexture lightShadowMap)
        {
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

            var shadow = lightShadowMap.Shadow;
            // TODO: Min and Max distance can be auto-computed from readback from Z buffer
            var camera = shadowMapRenderer.Camera;
            var shadowCamera = shadowMapRenderer.ShadowCamera;

            // Push a new graphics state
            var graphicsDevice = context.GraphicsDevice;
            graphicsDevice.PushState();

            float splitMaxRatio = shadow.MinDistance;
            for (int cascadeLevel = 0; cascadeLevel < lightShadowMap.CascadeCount; ++cascadeLevel)
            {
                // Compute caster view and projection matrices
                var shadowMapView = Matrix.Zero;
                var shadowMapProjection = Matrix.Zero;

                // Calculate frustum corners for this cascade
                var splitMinRatio = splitMaxRatio;
                splitMaxRatio = CascadeSplitRatios[cascadeLevel];
                for (int j = 0; j < 4; j++)
                {
                    var frustumRange = shadowMapRenderer.FrustumCorner[j + 4] - shadowMapRenderer.FrustumCorner[j];
                    CascadeFrustumCorners[j] = shadowMapRenderer.FrustumCorner[j] + frustumRange * splitMinRatio;
                    CascadeFrustumCorners[j + 4] = shadowMapRenderer.FrustumCorner[j] + frustumRange * splitMaxRatio;
                }
                var cascadeBoundWS = BoundingBox.FromPoints(CascadeFrustumCorners);

                var cascadeMinBoundLS = Vector3.Zero;
                var cascadeMaxBoundLS = Vector3.Zero;

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
                    for (int i = 0; i < CascadeFrustumCorners.Length; i++)
                    {
                        Vector3 cornerViewSpace;
                        Vector3.TransformCoordinate(ref CascadeFrustumCorners[i], ref lightViewMatrix, out cornerViewSpace);

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
                CascadeSplits[cascadeLevel] = camera.NearClipPlane + CascadeSplitRatios[cascadeLevel] * (camera.FarClipPlane - camera.NearClipPlane);

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
                Matrix.Multiply(ref cascadeShadowMatrix, ref adjustmentMatrix, out WorldToShadowCascadeUV[cascadeLevel]);

                // Render to the atlas
                lightShadowMap.Atlas.RenderFrame.Activate(context);
                graphicsDevice.SetViewport(new Viewport(shadowMapRectangle.X, shadowMapRectangle.Y, shadowMapRectangle.Width, shadowMapRectangle.Height));

                // Render the scene for this cascade
                shadowMapRenderer.RenderCasters(context);

                //// Copy texture coords with border
                //cascades[cascadeLevel].CascadeLevels.CascadeTextureCoordsBorder = cascadeTextureCoords;
            }

            graphicsDevice.PopState();
        }
    }
}