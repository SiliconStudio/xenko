// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Lights;

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
            CascadeCasterMatrix = new Matrix[4];
            CascadeToUVMatrix = new Matrix[4];
            CascadeSplitRatios = new float[4];
            CascadeSplits = new float[4];
            CascadeOffsets = new Vector3[4];
            CascadeScales = new Vector3[4];
            CascadeRectangleUVs = new Vector4[4];
            CascadeFrustumCorners = new Vector3[8];
        }

        public readonly Matrix[] CascadeCasterMatrix;

        public readonly Matrix[] CascadeToUVMatrix;

        public readonly float[] CascadeSplitRatios;

        public readonly float[] CascadeSplits;

        public readonly Vector3[] CascadeOffsets;

        public readonly Vector3[] CascadeScales;

        public readonly Vector4[] CascadeRectangleUVs;

        private Vector3[] CascadeFrustumCorners;

        private void ComputeCascadeSplits(ShadowMapCasterContext shadowContext, ref LightShadowMapTexture lightShadowMap)
        {
            var shadow = lightShadowMap.Shadow;

            // TODO: Min and Max distance can be auto-computed from readback from Z buffer
            var minDistance = shadow.MinDistance;
            var maxDistance = shadow.MinDistance;

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
                CascadeSplitRatios[0] = minDistance + shadow.SplitDistance0 * maxDistance;
                CascadeSplitRatios[1] = minDistance + shadow.SplitDistance1 * maxDistance;
                CascadeSplitRatios[2] = minDistance + shadow.SplitDistance2 * maxDistance;
                CascadeSplitRatios[3] = minDistance + shadow.SplitDistance3 * maxDistance;
            }
        }

        public void Render(ShadowMapCasterContext shadowContext, ref LightShadowMapTexture lightShadowMap)
        {
            // Computes the cascade splits
            ComputeCascadeSplits(shadowContext, ref lightShadowMap);
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
            var minDistance = shadow.MinDistance;
            var maxDistance = shadow.MinDistance;
            var camera = shadowContext.Camera;

            for (int cascadeLevel = 0; cascadeLevel < lightShadowMap.CascadeCount; ++cascadeLevel)
            {
                // Compute caster view and projection matrices
                var shadowMapView = Matrix.Zero;
                var shadowMapProjection = Matrix.Zero;

                float max = minDistance;

                // Calculate frustum corners for this cascade
                for (int j = 0; j < 4; j++)
                {
                    var min = max;
                    max = CascadeSplitRatios[cascadeLevel];
                    CascadeFrustumCorners[j * 2 + 0] = shadowContext.FrustumCorner[j] + shadowContext.FrustumDirection[j] * min;
                    CascadeFrustumCorners[j * 2 + 1] = shadowContext.FrustumCorner[j] + shadowContext.FrustumDirection[j] * max;
                }
                var cascadeBounds = BoundingBox.FromPoints(CascadeFrustumCorners);

                var orthoMin = Vector3.Zero;
                var orthoMax = Vector3.Zero;

                var target = cascadeBounds.Center;

                if (shadow.Stabilized)
                {
                    // Compute bounding box center & radius
                    // Note: boundingBox is computed in view space so the computation of the radius is only correct when the view matrix does not do any kind of scale/shear transformation
                    var radius = (cascadeBounds.Maximum - cascadeBounds.Minimum).Length() * 0.5f;

                    orthoMax = new Vector3(radius, radius, radius);
                    orthoMin = -orthoMax;

                    // Make sure we are using the same direction when stabilizing
                    upDirection = shadowContext.Camera.ViewMatrix.Right;

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
                    var lightViewMatrix = Matrix.LookAtLH(cascadeBounds.Center - direction, cascadeBounds.Center, upDirection);
                    orthoMin = new Vector3(float.MaxValue);
                    orthoMax = new Vector3(-float.MaxValue);
                    for (int i = 0; i < CascadeFrustumCorners.Length; i++)
                    {
                        Vector3 cornerViewSpace;
                        Vector3.TransformCoordinate(ref CascadeFrustumCorners[i], ref lightViewMatrix, out cornerViewSpace);

                        orthoMin = Vector3.Min(orthoMin, cornerViewSpace);
                        orthoMax = Vector3.Min(orthoMax, cornerViewSpace);
                    }

                    // TODO: Adjust orthoSize by taking into account filtering size
                }

                // Compute caster view and projection matrices
                shadowMapView = Matrix.LookAtRH(target - direction * orthoMin.Z, target, upDirection); // View;
                shadowMapProjection = Matrix.OrthoOffCenterRH(orthoMin.X, orthoMax.X, orthoMin.Y, orthoMax.Y, 0.0f, orthoMax.Z - orthoMin.Z); // Projection

                // Calculate View Proj matrix from World space to Cascade space
                Matrix.Multiply(ref shadowMapView, ref shadowMapProjection, out CascadeCasterMatrix[cascadeLevel]);

                // Cascade splits in light space using depth
                CascadeSplits[cascadeLevel] = camera.NearClipPlane + CascadeSplitRatios[cascadeLevel] * (camera.FarClipPlane - camera.NearClipPlane);

                // Cascade offsets
                Matrix lightSpaceToWorld;
                Matrix.Invert(ref shadowMapView, out lightSpaceToWorld);
                CascadeOffsets[cascadeLevel] = lightSpaceToWorld.TranslationVector;

                var shadowMapRectangle = lightShadowMap.GetRectangle(cascadeLevel);

                var cascadeTextureCoords = new Vector4((float)shadowMapRectangle.Left / lightShadowMap.Atlas.Width,
                    (float)shadowMapRectangle.Top / lightShadowMap.Atlas.Height,
                    (float)shadowMapRectangle.Right / lightShadowMap.Atlas.Width,
                    (float)shadowMapRectangle.Bottom / lightShadowMap.Atlas.Height);

                // Copy texture coords without border
                CascadeRectangleUVs[cascadeLevel] = cascadeTextureCoords;

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
                Matrix adjustmentMatrix = Matrix.Scaling(leftX, -leftY, 0.5f) * Matrix.Translation(centerX, centerY, 0.5f);
                Matrix.Multiply(ref CascadeCasterMatrix[cascadeLevel], ref adjustmentMatrix, out CascadeToUVMatrix[cascadeLevel]);

                //// Copy texture coords with border
                //cascades[cascadeLevel].CascadeLevels.CascadeTextureCoordsBorder = cascadeTextureCoords;
            }
        }
    }
}