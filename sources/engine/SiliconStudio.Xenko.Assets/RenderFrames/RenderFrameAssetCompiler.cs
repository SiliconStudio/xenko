// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.RenderFrames
{
    public class RenderFrameAssetCompiler : AssetCompilerBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (RenderFrameAsset)assetItem.Asset;
            result.BuildSteps = new ListBuildStep { new RenderFrameCompileCommand(targetUrlInStorage, asset, assetItem.Package) };
        }

        private class RenderFrameCompileCommand : AssetCommand<RenderFrameAsset>
        {
            public RenderFrameCompileCommand(string url, RenderFrameAsset parameters, Package package)
                : base(url, parameters, package)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();
                assetManager.Save(Url, RenderFrame.NewFake(Parameters.Descriptor));

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
