// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
                IncludedCollisionGroups = CollisionFilterGroupFlags.AllFilter,
            };
        }
    }
}
