// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    public class DefaultNavigationMeshFactory : AssetFactory<NavigationMeshAsset>
    {
        public override NavigationMeshAsset New()
        {
            NavigationMeshAsset newAsset = new NavigationMeshAsset();

            newAsset.AutoGenerateBoundingBox = true;

            // Initialize build settings
            NavigationMeshBuildSettings defaultBuildSettings = new NavigationMeshBuildSettings();
            defaultBuildSettings.NavigationMeshAgentSettings.Height = 1.0f;
            defaultBuildSettings.NavigationMeshAgentSettings.Radius = 0.1f;
            defaultBuildSettings.NavigationMeshAgentSettings.MaxSlope = 45.0f;
            defaultBuildSettings.NavigationMeshAgentSettings.MaxClimb = 0.25f;
            defaultBuildSettings.BoundingBox = new BoundingBox(new Vector3(-25.0f), new Vector3(25.0f));
            defaultBuildSettings.CellHeight = 0.05f;
            defaultBuildSettings.CellSize = 0.1f;
            newAsset.BuildSettings = defaultBuildSettings;

            return newAsset;
        }
    }

}
