// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    public class GraphicsCompositorAssetCompiler : AssetCompilerBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (GraphicsCompositorAsset)assetItem.Asset;
            // TODO: We should ignore game settings stored in dependencies
            result.BuildSteps = new AssetBuildStep(assetItem)
            {
                new GraphicsCompositorCompileCommand(targetUrlInStorage, asset),
            };
        }

        internal class GraphicsCompositorCompileCommand : AssetCommand<GraphicsCompositorAsset>
        {
            public GraphicsCompositorCompileCommand(string url, GraphicsCompositorAsset asset) : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var graphicsCompositor = new GraphicsCompositor();

                graphicsCompositor.RenderStages.AddRange(Parameters.RenderStages);
                graphicsCompositor.RenderFeatures.AddRange(Parameters.RenderFeatures);
                graphicsCompositor.Code = Parameters.Code;

                var assetManager = new ContentManager();
                assetManager.Save(Url, graphicsCompositor);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}