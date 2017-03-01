// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class SceneAssetCompiler : EntityHierarchyCompilerBase<SceneAsset>
    {
        protected override AssetCommand<SceneAsset> Create(string url, SceneAsset assetParameters, Package package)
        {
            return new SceneCommand(url, assetParameters, package);
        }

        private class SceneCommand : AssetCommand<SceneAsset>
        {
            public SceneCommand(string url, SceneAsset parameters, Package package) : base(url, parameters, package)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                var scene = new Scene { Parent = Parameters.Parent };
                foreach (var rootEntity in Parameters.Hierarchy.RootPartIds)
                {
                    scene.Entities.Add(Parameters.Hierarchy.Parts[rootEntity].Entity);
                }
                assetManager.Save(Url, scene);

                return Task.FromResult(ResultStatus.Successful);
            }

            public override string ToString()
            {
                return $"Scene command for asset '{Url}'.";
            }
        }
    }
}
