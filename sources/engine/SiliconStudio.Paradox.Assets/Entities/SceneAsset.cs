// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.Engine;

using IObjectFactory = SiliconStudio.Core.Reflection.IObjectFactory;

namespace SiliconStudio.Paradox.Assets.Entities
{
    /// <summary>
    /// A scene asset.
    /// </summary>
    [DataContract("SceneAsset")]
    [AssetDescription(FileSceneExtension)]
    [ObjectFactory(typeof(SceneFactory))]
    //[ThumbnailCompiler(PreviewerCompilerNames.SceneThumbnailCompilerQualifiedName, true)]
    [AssetFormatVersion(1)]
    [AssetUpgrader(0, 1, typeof(Upgrader))]
    [Display(200, "Scene", "A scene")]
    public class SceneAsset : EntityAsset
    {
        public const string FileSceneExtension = ".pdxscene";

        public static SceneAsset Create()
        {
            // Create a new root entity, and make sure transformation component is created
            var rootEntity = new Scene { Name = "Root" };
            rootEntity.GetOrCreate(TransformComponent.Key);

            return new SceneAsset
            {
                Hierarchy =
                {
                    Entities = { rootEntity },
                    RootEntity = rootEntity.Id,
                }
            };
        }

        class Upgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(int currentVersion, int targetVersion, ILogger log, dynamic asset)
            {
                if (asset.Source != null)
                    asset.Source = DynamicYamlEmpty.Default;
                if (asset.SourceHash != null)
                    asset.SourceHash = DynamicYamlEmpty.Default;
            }
        }

        private class SceneFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return Create();
            }
        }

    }
}