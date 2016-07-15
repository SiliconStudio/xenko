// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Light renderer for clustered shading.
    /// </summary>
    /// <remarks>
    /// Due to the fact that it handles both Point and Spot with a single logic, it doesn't fit perfectly the current logic of one "direct light groups" per renderer.
    /// </remarks>
    public class LightClusteredPointGroupRenderer : LightGroupRendererBase
    {
        private PointLightShaderGroupData pointGroup;
        private PointSpotShaderGroupData spotGroup;

        private Texture lightClusters;
        private Buffer lightIndicesBuffer;
        private Buffer pointLightsBuffer;
        private Buffer spotLightsBuffer;

        public LightGroupRendererBase SpotRenderer { get; }

        public LightClusteredPointGroupRenderer()
        {
            SpotRenderer = new LightClusteredSpotGroupRenderer(this);
        }

        public override void Initialize(RenderContext context)
        {
            base.Initialize(context);

            pointGroup = new PointLightShaderGroupData(context, this);
            spotGroup = new PointSpotShaderGroupData(context, pointGroup);
        }

        public override void Unload()
        {
            // Dispose GPU resources
            lightClusters?.Dispose();
            lightClusters = null;

            lightIndicesBuffer?.Dispose();
            lightIndicesBuffer = null;

            pointLightsBuffer?.Dispose();
            pointLightsBuffer = null;

            spotLightsBuffer?.Dispose();
            spotLightsBuffer = null;

            base.Unload();
        }

        public override void Reset()
        {
            base.Reset();

            pointGroup.Reset();
            spotGroup.Reset();
        }

        public override void SetViews(FastList<RenderView> views)
        {
            base.SetViews(views);

            pointGroup.SetViews(views);
            spotGroup.SetViews(views);
        }

        public override void ProcessLights(ProcessLightsParameters parameters)
        {
            pointGroup.AddView(parameters.ViewIndex, parameters.View, parameters.LightEnd - parameters.LightStart);

            for (int index = parameters.LightStart; index < parameters.LightEnd; index++)
            {
                pointGroup.AddLight(parameters.LightCollection[index], null);
            }
        }

        public override void UpdateShaderPermutationEntry(ForwardLightingRenderFeature.LightShaderPermutationEntry shaderEntry)
        {
            shaderEntry.DirectLightGroups.Add(pointGroup);
            shaderEntry.DirectLightGroups.Add(spotGroup);
        }

        class PointLightShaderGroupData : LightShaderGroupDynamic
        {
            private readonly LightClusteredPointGroupRenderer pointGroupRenderer;

            public int ClusterSize = 64; // Size in pixel of each cluster
            public int ClusterSlices = 8; // Number of ranges

            // Artifically increase range of first slice to not waste too much slices in very short area
            public float SpecialNearPlane = 2.0f;

            private float clusterDepthScale;
            private float clusterDepthBias;

            private FastListStruct<PointLightData> pointLights = new FastListStruct<PointLightData>(8);
            private FastListStruct<SpotLightData> spotLights = new FastListStruct<SpotLightData>(8);
            private FastListStruct<int> lightIndices = new FastListStruct<int>(8);
            private FastListStruct<LightClusterLinkedNode> lightNodes = new FastListStruct<LightClusterLinkedNode>(8);
            private FastListStruct<Int2> clusterInfos = new FastListStruct<Int2>(8);
            private Int2[] lightClustersValues;
            private RenderView[] renderViews;

            private Plane[] zPlanes;

            public PointLightShaderGroupData(RenderContext renderContext, LightClusteredPointGroupRenderer pointGroupRenderer)
                : base(renderContext, null)
            {
                this.pointGroupRenderer = pointGroupRenderer;
                ShaderSource = new ShaderClassSource("LightClusteredPointGroup", ClusterSize);
            }

            protected override void UpdateLightCount()
            {
                base.UpdateLightCount();

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("LightClusteredPointGroup", ClusterSize));
                ShadowGroup?.ApplyShader(mixin);

                ShaderSource = mixin;
            }

            /// <inheritdoc/>
            protected override int ComputeLightCount(int lightCount)
            {
                // Fake numbers (we allow as many lights as we want in practice)
                return 1;
            }

            public override void Reset()
            {
                base.Reset();

                if (renderViews != null)
                    Array.Clear(renderViews, 0, renderViews.Length);
            }

            /// <inheritdoc/>
            public override void SetViews(FastList<RenderView> views)
            {
                base.SetViews(views);

                Array.Resize(ref renderViews, views.Count);
                for (int i = 0; i < views.Count; ++i)
                    renderViews[i] = views[i];
            }

            /// <inheritdoc/>
            public override int AddView(int viewIndex, RenderView renderView, int lightCount)
            {
                base.AddView(viewIndex, renderView, lightCount);

                // We allow more lights than LightCurrentCount (they will be culled)
                return lightCount;
            }

            public override unsafe void ApplyViewParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters)
            {
                // Note: no need to fill CurrentLights since we have no shadow maps
                base.ApplyViewParameters(context, viewIndex, parameters);

                var renderView = renderViews[viewIndex];

                var viewSize = renderView.ViewSize;

                // No screen size set?
                if (viewSize.X == 0 || viewSize.Y == 0)
                    return;

                var clusterCountX = ((int)viewSize.X + ClusterSize - 1) / ClusterSize;
                var clusterCountY = ((int)viewSize.Y + ClusterSize - 1) / ClusterSize;

                // TODO: Additional culling on x/y (to remove corner clusters)
                // Prepare planes for culling
                //var viewProjection = renderView.ViewProjection;
                //Array.Resize(ref zPlanes, ClusterSlices + 1);
                //for (int z = 0; z <= ClusterSlices; ++z)
                //{
                //    var zFactor = (float)z / (float)ClusterSlices;
                //
                //    // Build planes between nearplane and -farplane (see BoundingFrustum code)
                //    zPlanes[z] = new Plane(
                //        viewProjection.M13 - zFactor * viewProjection.M14,
                //        viewProjection.M23 - zFactor * viewProjection.M24,
                //        viewProjection.M33 - zFactor * viewProjection.M34,
                //        viewProjection.M43 - zFactor * viewProjection.M44);
                //
                //    zPlanes[z].Normalize();
                //}

                if (pointGroupRenderer.lightClusters == null || lightClustersValues.Length != clusterCountX * clusterCountY * ClusterSlices)
                {
                    // First time?
                    pointGroupRenderer.lightClusters?.Dispose();
                    pointGroupRenderer.lightClusters = Texture.New3D(context.GraphicsDevice, clusterCountX, clusterCountY, 8, PixelFormat.R32G32_UInt);
                    lightClustersValues = new Int2[clusterCountX * clusterCountY * ClusterSlices];
                }

                // Initialize cluster with no light (-1)
                for (int i = 0; i < clusterCountX * clusterCountY * ClusterSlices; ++i)
                {
                    lightNodes.Add(new LightClusterLinkedNode(LightType.Point, -1, -1));
                }

                // List of clusters moved by this light
                var movedClusters = new Dictionary<LightClusterLinkedNode, int>();

                // Try to use SpecialNearPlane to not waste too much slices in very small depth
                // Make sure we don't go to more than 10% of max depth
                var nearPlane = Math.Max(Math.Min(SpecialNearPlane, renderView.FarClipPlane * 0.1f), renderView.NearClipPlane);

                //var sliceBias = ((renderView.NearClipPlane * renderView.Projection.M33) + renderView.Projection.M43) / (renderView.NearClipPlane * renderView.Projection.M34);
                // Compute scale and bias so that near_plane..special_near fits in slice 0, then grow exponentionally
                //   log2(specialNear * scale + bias) == 1.0
                //   log2(far * scale + bias) == ClusterSlices
                // as a result:
                clusterDepthScale = (float)(Math.Pow(2.0f, ClusterSlices) - 2.0f) / (renderView.FarClipPlane - nearPlane);
                clusterDepthBias = 2.0f - clusterDepthScale * nearPlane;

                //---------------- SPOT LIGHTS -------------------
                var lightRange = pointGroupRenderer.spotGroup.LightRanges[viewIndex];
                for (int i = lightRange.Start; i < lightRange.End; ++i)
                {
                    var light = pointGroupRenderer.spotGroup.Lights[i].Light;
                    var spotLight = (LightSpot)light.Type;

                    // Create spot light data
                    var spotLightData = new SpotLightData
                    {
                        PositionWS = light.Position,
                        DirectionWS = light.Direction,
                        AngleOffsetAndInvSquareRadius = new Vector3(spotLight.LightAngleScale, spotLight.LightAngleOffset, spotLight.InvSquareRange),
                        Color = light.Color,
                    };

                    // Fill list of spot lights
                    spotLights.Add(spotLightData);

                    movedClusters.Clear();

                    var radius = (float)Math.Sqrt(1.0f / spotLightData.AngleOffsetAndInvSquareRadius.Z);

                    Vector3 positionVS;
                    Vector3.TransformCoordinate(ref spotLightData.PositionWS, ref renderView.View, out positionVS);

                    // TODO: culling (first do it on PointLight, then backport it to SpotLight and improve for SpotLight case)
                    // Find x/y ranges
                    Vector2 clipMin, clipMax;
                    ComputeClipRegion(positionVS, radius, ref renderView.Projection, out clipMin, out clipMax);

                    var tileStartX = MathUtil.Clamp((int)((clipMin.X * 0.5f + 0.5f) * viewSize.X / ClusterSize), 0, clusterCountX);
                    var tileEndX = MathUtil.Clamp((int)((clipMax.X * 0.5f + 0.5f) * viewSize.X / ClusterSize) + 1, 0, clusterCountX);
                    var tileStartY = MathUtil.Clamp((int)((-clipMax.Y * 0.5f + 0.5f) * viewSize.Y / ClusterSize), 0, clusterCountY);
                    var tileEndY = MathUtil.Clamp((int)((-clipMin.Y * 0.5f + 0.5f) * viewSize.Y / ClusterSize) + 1, 0, clusterCountY);

                    // Find z range (project using Projection matrix)
                    var startZ = -positionVS.Z - radius;
                    var endZ = -positionVS.Z + radius;

                    var tileStartZ = MathUtil.Clamp((int)Math.Log(startZ * clusterDepthScale + clusterDepthBias, 2.0f), 0, ClusterSlices);
                    var tileEndZ = MathUtil.Clamp((int)Math.Log(endZ * clusterDepthScale + clusterDepthBias, 2.0f) + 1, 0, ClusterSlices);

                    for (int z = tileStartZ; z < tileEndZ; ++z)
                    {
                        for (int y = tileStartY; y < tileEndY; ++y)
                        {
                            for (int x = tileStartX; x < tileEndX; ++x)
                            {
                                AddLightToCluster(movedClusters, LightType.Spot, i - lightRange.Start, x + (y + z * clusterCountY) * clusterCountX);
                            }
                        }
                    }
                }

                //---------------- POINT LIGHTS -------------------
                lightRange = LightRanges[viewIndex];
                for (int i = lightRange.Start; i < lightRange.End; ++i)
                {
                    var light = Lights[i].Light;
                    var pointLight = (LightPoint)light.Type;

                    // Create point light data
                    var pointLightData = new PointLightData
                    {
                        PositionWS = light.Position,
                        InvSquareRadius = pointLight.InvSquareRadius,
                        Color = light.Color,
                    };

                    // Fill list of point lights
                    pointLights.Add(pointLightData);

                    movedClusters.Clear();

                    var radius = (float)Math.Sqrt(1.0f / pointLightData.InvSquareRadius);

                    Vector3 positionVS;
                    Vector3.TransformCoordinate(ref pointLightData.PositionWS, ref renderView.View, out positionVS);

                    //Vector3 positionScreen;
                    //Vector3.TransformCoordinate(ref pointLightData.PositionWS, ref renderView.ViewProjection, out positionScreen);

                    // Find x/y ranges
                    Vector2 clipMin, clipMax;
                    ComputeClipRegion(positionVS, radius, ref renderView.Projection, out clipMin, out clipMax);

                    var tileStartX = MathUtil.Clamp((int)((clipMin.X * 0.5f + 0.5f) * viewSize.X / ClusterSize), 0, clusterCountX);
                    var tileEndX = MathUtil.Clamp((int)((clipMax.X * 0.5f + 0.5f) * viewSize.X / ClusterSize) + 1, 0, clusterCountX);
                    var tileStartY = MathUtil.Clamp((int)((-clipMax.Y * 0.5f + 0.5f) * viewSize.Y / ClusterSize), 0, clusterCountY);
                    var tileEndY = MathUtil.Clamp((int)((-clipMin.Y * 0.5f + 0.5f) * viewSize.Y / ClusterSize) + 1, 0, clusterCountY);

                    // Find z range (project using Projection matrix)
                    var startZ = -positionVS.Z - radius;
                    var endZ = -positionVS.Z + radius;

                    //var centerZ = (int)(positionVS.Z * ClusterDepthScale + ClusterDepthBias);
                    var tileStartZ = MathUtil.Clamp((int)Math.Log(startZ * clusterDepthScale + clusterDepthBias, 2.0f), 0, ClusterSlices);
                    var tileEndZ = MathUtil.Clamp((int)Math.Log(endZ * clusterDepthScale + clusterDepthBias, 2.0f) + 1, 0, ClusterSlices);

                    for (int z = tileStartZ; z < tileEndZ; ++z)
                    {
                        // TODO: Additional culling on x/y (to remove corner clusters)
                        // See "Practical Clustered Shading" for details
                        //if (z != centerZ)
                        //{
                        //    var plane = z < centerZ ? zPlanes[z + 1] : -zPlanes[z];
                        //    
                        //    positionScreen = Plane.DotCoordinate(ref plane, ref positionScreen, out )
                        //}

                        for (int y = tileStartY; y < tileEndY; ++y)
                        {
                            for (int x = tileStartX; x < tileEndX; ++x)
                            {
                                AddLightToCluster(movedClusters, LightType.Point, i - lightRange.Start, x + (y + z * clusterCountY) * clusterCountX);
                            }
                        }
                    }
                }

                // Finish clusters by making their last element unique and building clusterInfos
                movedClusters.Clear();
                for (int i = 0; i < clusterCountX * clusterCountY * ClusterSlices; ++i)
                {
                    FinishCluster(movedClusters, i);
                }

                // Prepare light clusters
                for (int i = 0; i < clusterCountX * clusterCountY * ClusterSlices; ++i)
                {
                    var clusterId = lightNodes[i].NextNode;
                    lightClustersValues[i] = clusterId != -1 ? clusterInfos[clusterId] : new Int2(0, 0);
                }

                // Upload data to texture
                using (context.LockCommandList())
                {
                    fixed (Int2* dataPtr = lightClustersValues)
                        context.CommandList.UpdateSubresource(pointGroupRenderer.lightClusters, 0, new DataBox((IntPtr)dataPtr, sizeof(Int2) * clusterCountX, sizeof(Int2) * clusterCountX * clusterCountY));

                    // PointLights: Ensure size and update
                    if (pointLights.Count > 0)
                    {
                        if (pointGroupRenderer.pointLightsBuffer == null || pointGroupRenderer.pointLightsBuffer.SizeInBytes < pointLights.Count * sizeof(PointLightData))
                        {
                            pointGroupRenderer.pointLightsBuffer?.Dispose();
                            pointGroupRenderer.pointLightsBuffer = Buffer.New(context.GraphicsDevice, MathUtil.NextPowerOfTwo(pointLights.Count * sizeof(PointLightData)), 0, BufferFlags.ShaderResource, PixelFormat.R32G32B32A32_Float);
                        }
                        fixed (PointLightData* pointLightsPtr = pointLights.Items)
                            context.CommandList.UpdateSubresource(pointGroupRenderer.pointLightsBuffer, 0, new DataBox((IntPtr)pointLightsPtr, 0, 0), new ResourceRegion(0, 0, 0, pointLights.Count * sizeof(PointLightData), 1, 1));
                    }

                    // SpotLights: Ensure size and update
                    if (spotLights.Count > 0)
                    {
                        if (pointGroupRenderer.spotLightsBuffer == null || pointGroupRenderer.spotLightsBuffer.SizeInBytes < spotLights.Count * sizeof(SpotLightData))
                        {
                            pointGroupRenderer.spotLightsBuffer?.Dispose();
                            pointGroupRenderer.spotLightsBuffer = Buffer.New(context.GraphicsDevice, MathUtil.NextPowerOfTwo(spotLights.Count * sizeof(SpotLightData)), 0, BufferFlags.ShaderResource, PixelFormat.R32G32B32A32_Float);
                        }
                        fixed (SpotLightData* spotLightsPtr = spotLights.Items)
                            context.CommandList.UpdateSubresource(pointGroupRenderer.spotLightsBuffer, 0, new DataBox((IntPtr)spotLightsPtr, 0, 0), new ResourceRegion(0, 0, 0, spotLights.Count * sizeof(SpotLightData), 1, 1));
                    }

                    // LightIndices: Ensure size and update
                    if (lightIndices.Count > 0)
                    {
                        if (pointGroupRenderer.lightIndicesBuffer == null || pointGroupRenderer.lightIndicesBuffer.SizeInBytes < lightIndices.Count*sizeof(int))
                        {
                            pointGroupRenderer.lightIndicesBuffer?.Dispose();
                            pointGroupRenderer.lightIndicesBuffer = Buffer.New(context.GraphicsDevice, MathUtil.NextPowerOfTwo(lightIndices.Count*sizeof(int)), 0, BufferFlags.ShaderResource, PixelFormat.R32_UInt);
                        }
                        fixed (int* lightIndicesPtr = lightIndices.Items)
                            context.CommandList.UpdateSubresource(pointGroupRenderer.lightIndicesBuffer, 0, new DataBox((IntPtr)lightIndicesPtr, 0, 0), new ResourceRegion(0, 0, 0, lightIndices.Count*sizeof(int), 1, 1));
                    }
                }

                // Clear data
                pointLights.Clear();
                spotLights.Clear();
                lightIndices.Clear();
                lightNodes.Clear();
                clusterInfos.Clear();

                // Set resources
                parameters.Set(LightClusteredPointGroupKeys.PointLights, pointGroupRenderer.pointLightsBuffer);
                parameters.Set(LightClusteredSpotGroupKeys.SpotLights, pointGroupRenderer.spotLightsBuffer);
                parameters.Set(LightClusteredKeys.LightIndices, pointGroupRenderer.lightIndicesBuffer);
                parameters.Set(LightClusteredKeys.LightClusters, pointGroupRenderer.lightClusters);

                parameters.Set(LightClusteredKeys.ClusterDepthScale, clusterDepthScale);
                parameters.Set(LightClusteredKeys.ClusterDepthBias, clusterDepthBias);
            }

            private void FinishCluster(Dictionary<LightClusterLinkedNode, int> movedClusters, int clusterIndex)
            {
                var clusterId = -1;
                if (lightNodes.Items[clusterIndex].LightIndex != -1)
                {
                    var movedCluster = lightNodes.Items[clusterIndex];

                    // Try to check if same linked-list doesn't already exist
                    if (!movedClusters.TryGetValue(movedCluster, out clusterId))
                    {
                        // First time, let's add it
                        clusterId = movedClusters.Count;
                        movedClusters.Add(movedCluster, clusterId);

                        int lightIndex = lightNodes.Count;
                        lightNodes.Add(movedCluster);

                        // Build light indices
                        int pointLightCounter = 0;
                        int spotLightCounter = 0;

                        while (lightIndex != -1)
                        {
                            movedCluster = lightNodes[lightIndex];
                            lightIndices.Add(movedCluster.LightIndex);
                            switch (movedCluster.LightType)
                            {
                                case LightType.Point:
                                    pointLightCounter++;
                                    break;
                                case LightType.Spot:
                                    spotLightCounter++;
                                    break;
                            }
                            lightIndex = movedCluster.NextNode;
                        }

                        // Add new light cluster range
                        // Stored in the format:
                        //   x          = start_index
                        //   y & 0xFFFF = point_light_count
                        //   y >> 16    =  spot_light_count
                        clusterInfos.Add(new Int2(lightIndices.Count - pointLightCounter - spotLightCounter, pointLightCounter | (spotLightCounter << 16)));
                    }
                }

                // Last pass: store cluster id (instead of next node)
                lightNodes.Items[clusterIndex] = new LightClusterLinkedNode(LightType.Point, -1, clusterId);
            }

            private void AddLightToCluster(Dictionary<LightClusterLinkedNode, int> movedClusters, LightType lightType, int lightIndex, int clusterIndex)
            {
                var nextNode = -1;
                if (lightNodes.Items[clusterIndex].LightIndex != -1)
                {
                    var movedCluster = lightNodes.Items[clusterIndex];
                    
                    // Try to check if same linked-list doesn't already exist
                    if (!movedClusters.TryGetValue(movedCluster, out nextNode))
                    {
                        // First time, let's add it
                        nextNode = lightNodes.Count;
                        movedClusters.Add(movedCluster, nextNode);
                        lightNodes.Add(movedCluster);
                    }
                }

                // Replace new linked-list head
                lightNodes.Items[clusterIndex] = new LightClusterLinkedNode(lightType, lightIndex, nextNode);
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

            private static void ComputeClipRegion(Vector3 lightPosView, float lightRadius, ref Matrix projection, out Vector2 clipMin, out Vector2 clipMax)
            {
                clipMin = new Vector2(-1.0f, -1.0f);
                clipMax = new Vector2(1.0f, 1.0f);

                UpdateClipRegion(lightPosView.X, -lightPosView.Z, lightRadius, projection.M11, ref clipMin.X, ref clipMax.X);
                UpdateClipRegion(lightPosView.Y, -lightPosView.Z, lightRadius, projection.M22, ref clipMin.Y, ref clipMax.Y);
            }

            // Single linked list of lights (stored in an array)
            struct LightClusterLinkedNode : IEquatable<LightClusterLinkedNode>
            {
                public readonly LightType LightType;
                public readonly int LightIndex;
                public readonly int NextNode;

                public LightClusterLinkedNode(LightType lightType, int lightIndex, int nextNode)
                {
                    LightType = lightType;
                    LightIndex = lightIndex;
                    NextNode = nextNode;
                }

                public bool Equals(LightClusterLinkedNode other)
                {
                    return LightType == other.LightType && LightIndex == other.LightIndex && NextNode == other.NextNode;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    return obj is LightClusterLinkedNode && Equals((LightClusterLinkedNode)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        var hashCode = (int)LightType;
                        hashCode = (hashCode*397) ^ LightIndex;
                        hashCode = (hashCode*397) ^ NextNode;
                        return hashCode;
                    }
                }
            }
        }

        class LightClusteredSpotGroupRenderer : LightGroupRendererBase
        {
            private readonly LightClusteredPointGroupRenderer pointGroupRenderer;

            public LightClusteredSpotGroupRenderer(LightClusteredPointGroupRenderer pointGroupRenderer)
            {
                this.pointGroupRenderer = pointGroupRenderer;
            }

            public override void ProcessLights(ProcessLightsParameters parameters)
            {
                pointGroupRenderer.spotGroup.AddView(parameters.ViewIndex, parameters.View, parameters.LightEnd - parameters.LightStart);

                for (int index = parameters.LightStart; index < parameters.LightEnd; index++)
                {
                    pointGroupRenderer.spotGroup.AddLight(parameters.LightCollection[index], null);
                }
            }

            public override void UpdateShaderPermutationEntry(ForwardLightingRenderFeature.LightShaderPermutationEntry shaderEntry)
            {
            }
        }

        class PointSpotShaderGroupData : LightShaderGroupDynamic
        {
            public PointSpotShaderGroupData(RenderContext renderContext, PointLightShaderGroupData pointLightGroup)
                : base(renderContext, null)
            {
                ShaderSource = new ShaderClassSource("LightClusteredSpotGroup", pointLightGroup.ClusterSize);
            }

            // Makes LightRanges and Lights public
            public new LightRange[] LightRanges => base.LightRanges;

            public new FastListStruct<LightDynamicEntry> Lights => base.Lights;
        }

        enum LightType
        {
            Point,
            Spot,
        }
    }
}
