// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    [DataContract("NavigationMeshAsset")]
    [AssetDescription(FileExtension)]
    [Display("Navigation Mesh Asset")]
    [AssetCompiler(typeof(NavigationMeshAssetCompiler))]
    public class NavigationMeshAsset : Asset, IAssetCompileTimeDependencies
    {
        public const string FileExtension = ".xknavmesh";

        /// <summary>
        /// Scene that is used for building the navigation mesh
        /// </summary>
        [DataMember(1000)]
        public Scene DefaultScene { get; set; }

        /// <summary>
        /// The bounding box used for the navigation mesh, ignored if <see cref="AutoGenerateBoundingBox"/> is set to true
        /// </summary>
        [DataMember(1200)]
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Toggles automatic generation of bounding boxed, might not work well with infinite planes.
        /// If this is enabled, the <see cref="BoundingBox"/> property is ignored
        /// </summary>
        [DataMember(1205)]
        public bool AutoGenerateBoundingBox { get; set; }
        
        /// <summary>
        /// Collision filter that indicates which colliders are used in navmesh generation
        /// </summary>
        [DataMember(1500)]
        public CollisionFilterGroupFlags AllowedCollisionGroups { get; set; }

        /// <summary>
        /// Build settings used by Recast
        /// </summary>
        [DataMember(2000)]
        public NavigationMeshBuildSettings BuildSettings { get; set; }

        /// <summary>
        /// Settings for agents used with this navigationMesh
        /// Every entry corresponds with a layer, which is used by <see cref="NavigationComponent.NavigationMeshLayer"/> to select one from this list at runtime
        /// </summary>
        [DataMember(2010)] public List<NavigationAgentSettings> NavigationMeshAgentSettings = new List<NavigationAgentSettings>();

        public override int GetHashCode()
        {
            int hash = BoundingBox.GetHashCode() + AutoGenerateBoundingBox.GetHashCode() + BuildSettings.GetHashCode() + DefaultScene.Name.GetHashCode();
            hash += 379*AllowedCollisionGroups.GetHashCode();
            if (NavigationMeshAgentSettings != null)
            {
                foreach (var agentSetting in NavigationMeshAgentSettings)
                {
                    hash += agentSetting.GetHashCode();
                }
            }
            return hash;
        }

        public IEnumerable<IReference> EnumerateCompileTimeDependencies(PackageSession session)
        {
            if (DefaultScene != null)
            {
                var reference = AttachedReferenceManager.GetAttachedReference(DefaultScene);
                yield return new AssetReference<SceneAsset>(reference.Id, reference.Url);
            }
        }
    }
}