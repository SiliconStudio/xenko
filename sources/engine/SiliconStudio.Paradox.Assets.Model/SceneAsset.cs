// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Assets.Model
{
    /// <summary>
    /// A scene asset.
    /// </summary>
    [DataContract("SceneAsset")]
    [AssetDescription(FileSceneExtension)]
    [ObjectFactory(typeof(SceneFactory))]
    //[ThumbnailCompiler(PreviewerCompilerNames.SceneThumbnailCompilerQualifiedName, true)]
    [Display(200, "Scene", "A scene")]
    public class SceneAsset : EntityAsset
    {
        public const string FileSceneExtension = ".pdxscene";

        private class SceneFactory : IObjectFactory
        {
            public object New(Type type)
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
        }

    }
}