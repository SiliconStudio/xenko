// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.RenderFrames
{
    public class RenderFrameAssetCompiler : AssetCompilerBase<RenderFrameAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, RenderFrameAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep { new RenderFrameCompileCommand(urlInStorage, asset) };
        }

        private class RenderFrameCompileCommand : AssetCommand<RenderFrameAsset>
        {
            public RenderFrameCompileCommand(string url, RenderFrameAsset assetParameters)
                : base(url, assetParameters)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();
                assetManager.Save(Url, RenderFrame.NewFake(AssetParameters.Descriptor));

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}