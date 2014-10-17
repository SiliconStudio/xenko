// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Paradox.Assets.Physics
{
    internal class ColliderShapeAssetCompiler : AssetCompilerBase<ColliderShapeAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, ColliderShapeAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep { new ColliderShapeCombineCommand(urlInStorage, asset) };
        }

        private class ColliderShapeCombineCommand : AssetCommand<ColliderShapeAsset>
        {
            public ColliderShapeCombineCommand(string url, ColliderShapeAsset asset)
                : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();
                assetManager.Save(Url, asset.Data);
                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
