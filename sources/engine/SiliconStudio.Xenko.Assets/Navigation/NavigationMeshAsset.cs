using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;

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

        [DataMember(2000)]
        public NavigationMeshBuildSettings BuildSettings { get; set; }

        [DataMember(2500)]
        public bool AutoGenerateBoundingBox { get; set; }

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
