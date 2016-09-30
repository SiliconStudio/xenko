// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    public class DefaultNavigationMeshFactory : AssetFactory<NavigationMeshAsset>
    {
        public override NavigationMeshAsset New()
        {
            // Initialize build settings
            return new NavigationMeshAsset
            {
                AutoGenerateBoundingBox = true,
                BuildSettings = new NavigationMeshBuildSettings
                {
                    BoundingBox = new BoundingBox(new Vector3(-25.0f), new Vector3(25.0f)),
                    CellHeight = 0.05f,
                    CellSize = 0.1f,
                    TileSize = 2,
                },
                NavigationMeshAgentSettings = new List<NavigationAgentSettings>
                {
                    new NavigationAgentSettings
                    {
                        Height = 1.0f,
                        Radius = 0.1f,
                        MaxSlope = 45.0f,
                        MaxClimb = 0.25f,
                    }
                }
            };
        }
    }
}