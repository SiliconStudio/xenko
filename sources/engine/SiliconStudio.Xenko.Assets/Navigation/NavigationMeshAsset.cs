// Copyright (c) 2016-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Navigation;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    [DataContract("NavigationMeshAsset")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(NavigationMesh))]
    [Display("Navigation Mesh")]
    public class NavigationMeshAsset : Asset
    {
        public const string FileExtension = ".xknavmesh";

        /// <summary>
        /// Scene that is used for building the navigation mesh
        /// </summary>
        [DataMember(10)]
        public Scene Scene { get; set; }
        
        /// <summary>
        /// Collision filter that indicates which colliders are used in navmesh generation
        /// </summary>
        [DataMember(20)]
        public CollisionFilterGroupFlags IncludedCollisionGroups { get; set; }

        /// <summary>
        /// Build settings used by Recast
        /// </summary>
        [DataMember(30)]
        public NavigationMeshBuildSettings BuildSettings { get; set; }

        /// <summary>
        /// Groups that this navigation mesh should be built for
        /// </summary>
        [DataMember(40)]
        public List<Guid> SelectedGroups { get; private set; } = new List<Guid>();

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SelectedGroups?.ComputeHash() ?? 0;
                hashCode = (hashCode*397) ^ (int)IncludedCollisionGroups;
                hashCode = (hashCode*397) ^ BuildSettings.GetHashCode();
                if (Scene != null)
                    hashCode = (hashCode*397) ^ Scene.Name.GetHashCode();
                return hashCode;
            }
        }
    }
}
