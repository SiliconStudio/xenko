// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    /// <summary>
    /// Default factory for navigation meshes
    /// </summary>
    public class DefaultNavigationMeshFactory : AssetFactory<NavigationMeshAsset>
    {
        public override NavigationMeshAsset New()
        {
            // Initialize build settings
            return new NavigationMeshAsset
            {
                BuildSettings = new NavigationMeshBuildSettings
                {
                    CellHeight = 0.2f,
                    CellSize = 0.3f,
                    TileSize = 32,
                    MinRegionArea = 2,
                    RegionMergeArea = 20,
                    MaxEdgeLen = 12.0f,
                    MaxEdgeError = 1.3f,
                    DetailSamplingDistance = 6.0f,
                    MaxDetailSamplingError = 1.0f,
                },
                BoundingBox = new BoundingBox(new Vector3(-10.0f), new Vector3(10.0f)),
                AutoGenerateBoundingBox = true,
                IncludedCollisionGroups = CollisionFilterGroupFlags.AllFilter,
                NavigationMeshAgentSettings = new List<NavigationAgentSettings>
                {
                    new NavigationAgentSettings
                    {
                        Height = 1.0f,
                        Radius = 0.5f,
                        MaxSlope = new AngleSingle(45.0f, AngleType.Degree),
                        MaxClimb = 0.25f,
                    }
                }
            };
        }
    }
}