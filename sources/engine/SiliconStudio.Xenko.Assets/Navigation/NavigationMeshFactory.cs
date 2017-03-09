// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Navigation;
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
                BuildSettings = ObjectFactoryRegistry.NewInstance<NavigationMeshBuildSettings>(),
                BoundingBox = new BoundingBox(new Vector3(-10.0f), new Vector3(10.0f)),
                AutoGenerateBoundingBox = true,
                IncludedCollisionGroups = CollisionFilterGroupFlags.AllFilter,
                NavigationMeshAgentSettings = new List<NavigationAgentSettings>
                {
                    ObjectFactoryRegistry.NewInstance<NavigationAgentSettings>()
                }
            };
        }
    }
}