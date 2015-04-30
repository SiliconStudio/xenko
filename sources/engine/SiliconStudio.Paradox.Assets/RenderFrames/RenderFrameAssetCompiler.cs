// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Assets.RenderFrames
{
    public class RenderFrameAssetCompiler : AssetCompilerBase<RenderFrameAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, RenderFrameAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep { new RenderFrameCompileCommand(urlInStorage, asset) };
        }

        private class RenderFrameCompileCommand : AssetCommand<RenderFrameAsset>
        {
            public RenderFrameCompileCommand(string url, RenderFrameAsset asset)
                : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();
                assetManager.Save(Url, RenderFrame.NewFake(asset.Descriptor));

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}