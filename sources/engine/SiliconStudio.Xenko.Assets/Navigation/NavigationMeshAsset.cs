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

        [DataMember(1000)]
        public Scene DefaultScene { get; set; }

        [DataMember(1200)]
        public BoundingBox BoundingBox { get; set; }

        [DataMember(1205)]
        public bool AutoGenerateBoundingBox { get; set; }

        [DataMember(2000)]
        public NavigationMeshBuildSettings BuildSettings { get; set; }

        // Settings for agents used with this navigationMesh
        [DataMember(2010)] public List<NavigationAgentSettings> NavigationMeshAgentSettings = new List<NavigationAgentSettings>();

        public override int GetHashCode()
        {
            int hash = BoundingBox.GetHashCode() + AutoGenerateBoundingBox.GetHashCode() + BuildSettings.GetHashCode() + DefaultScene.Name.GetHashCode();
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

                var sceneAsset = (SceneAsset)session.FindAsset(reference.Url)?.Asset;
                List<Entity> sceneEntities = sceneAsset.Hierarchy.Parts.Select(x => x.Entity).ToList();
                foreach (var entity in sceneEntities)
                {
                    StaticColliderComponent collider = entity.Get<StaticColliderComponent>();
                    if (collider != null && collider.IsBlocking && collider.Enabled)
                    {
                        foreach (var inlineColliderShapeDesc in collider.ColliderShapes)
                        {
                            if (inlineColliderShapeDesc.GetType() == typeof(ColliderShapeAssetDesc))
                            {
                                yield return AttachedReferenceManager.GetAttachedReference(((ColliderShapeAssetDesc)inlineColliderShapeDesc).Shape);
                            }
                        }
                    }
                }
            }
        }
    }
}