using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    using SiliconStudio.Xenko.Native;

    [DataContract("NavmeshAsset")]
    [AssetDescription(FileExtension)]
    [Display("Navmesh Asset")]
    [AssetCompiler(typeof(NavmeshAssetCompiler))]
    public class NavmeshAsset : Asset, IAssetCompileTimeDependencies
    {
        public const string FileExtension = ".xknavmesh";

        [DataMember(1000)]
        public Scene DefaultScene { get; set; }

        [DataMember(2000)]
        public NavmeshBuildSettings BuildSettings { get; set; }

        [DataMember(2500)]
        public bool AutoGenerateBoundingBox { get; set; } = true;
        public NavmeshAsset()
        {
            // Initialize build settings
            NavmeshBuildSettings defaultBuildSettings = new NavmeshBuildSettings();
            defaultBuildSettings.AgentSettings.Height = 1.0f;
            defaultBuildSettings.AgentSettings.Radius = 0.1f;
            defaultBuildSettings.AgentSettings.MaxSlope = 45.0f;
            defaultBuildSettings.AgentSettings.MaxClimb = 0.25f;
            defaultBuildSettings.BoundingBox = new BoundingBox(new Vector3(-25.0f), new Vector3(25.0f));
            defaultBuildSettings.CellHeight = 0.05f;
            defaultBuildSettings.CellSize = 0.1f;
            BuildSettings = defaultBuildSettings;
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
