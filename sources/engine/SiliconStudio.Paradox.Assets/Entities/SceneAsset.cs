// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;

using SharpYaml.Serialization;

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

                // NOT USED - for reference, how to access a node that has a . inside is name
                //foreach (var entity in asset.Hierarchy.Entities)
                //{
                //    var comp = entity.Components as DynamicYamlMapping;
                //    if (comp != null)
                //    {
                //        var lightCompNode = comp.Node.Children.Where(x => x.Key is YamlScalarNode).FirstOrDefault(x => (string)(YamlScalarNode)(x.Key) == "LightComponent.Key").Value as YamlMappingNode;
                //        if (lightCompNode != null)
                //        {
                //            dynamic lightComp = new DynamicYamlMapping(lightCompNode);
                //            var shadow = lightComp.Type.Shadow as DynamicYamlMapping;
                //            if (shadow != null)
                //            {
                //                shadow.Node.Tag = null;
                //            }
                //        }
                //    }
                //}
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