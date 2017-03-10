// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation
{
    [DataContract]
    [Display("Navigation Settings")]
    [ObjectFactory(typeof(NavigationSettingsFactory))]
    public class NavigationSettings : Configuration
    {
        /// <summary>
        /// If set to <c>true</c>, navigation meshes will be built at runtime. This allows for scene streaming and dynamic obstacles
        /// </summary>
        public bool EnableDynamicNavigationMesh { get; set; }

        /// <summary>
        /// Collision filter that indicates which colliders are used in navmesh generation
        /// </summary>
        [DataMember(1500)]
        public CollisionFilterGroupFlags IncludedCollisionGroups { get; set; }

        /// <summary>
        /// Build settings used by Recast
        /// </summary>
        [DataMember(2000)]
        public NavigationMeshBuildSettings BuildSettings { get; set; }
        
        /// <summary>
        /// Settings for agents used with the dynamic navigation mesh
        /// Every entry corresponds with a layer, which is used by <see cref="NavigationComponent.NavigationMeshLayer"/> to select one from this list
        /// </summary>
        [DataMember(2010)]
        public List<NavigationAgentSettings> NavigationMeshAgentSettings = new List<NavigationAgentSettings>();
    }

    public class NavigationSettingsFactory : IObjectFactory
    {
        public object New(Type type)
        {
            // Initialize build settings
            return new NavigationSettings
            {
                EnableDynamicNavigationMesh = false,
                BuildSettings = ObjectFactoryRegistry.NewInstance<NavigationMeshBuildSettings>(),
                IncludedCollisionGroups = CollisionFilterGroupFlags.AllFilter,
                NavigationMeshAgentSettings = new List<NavigationAgentSettings>
                {
                    ObjectFactoryRegistry.NewInstance<NavigationAgentSettings>()
                }
            };
        }
    }
}