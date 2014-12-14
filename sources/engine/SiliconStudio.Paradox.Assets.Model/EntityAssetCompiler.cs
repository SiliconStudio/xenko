// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Paradox.Assets.Model
{
    public class EntityAssetCompiler : AssetCompilerBase<EntityAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, EntityAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep { new EntityCombineCommand(urlInStorage, asset) };
        }

        private class EntityCombineCommand : AssetCommand<EntityAsset>
        {
            public EntityCombineCommand(string url, EntityAsset asset) : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();

                var rootEntity = asset.Hierarchy.Entities[asset.Hierarchy.RootEntity];
                assetManager.Save(Url, rootEntity);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}