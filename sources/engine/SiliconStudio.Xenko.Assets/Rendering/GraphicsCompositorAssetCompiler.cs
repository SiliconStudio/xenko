// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;

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
                new GraphicsCompositorCompileCommand(targetUrlInStorage, asset, assetItem.Package),
            };
        }

        internal class GraphicsCompositorCompileCommand : AssetCommand<GraphicsCompositorAsset>
        {
            public GraphicsCompositorCompileCommand(string url, GraphicsCompositorAsset asset, Package assetItemPackage) : base(url, asset, assetItemPackage)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var graphicsCompositor = new GraphicsCompositor();

                foreach (var cameraSlot in Parameters.Cameras)
                    graphicsCompositor.Cameras.Add(cameraSlot);
                foreach (var renderStage in Parameters.RenderStages)
                    graphicsCompositor.RenderStages.Add(renderStage);
                foreach (var renderFeature in Parameters.RenderFeatures)
                    graphicsCompositor.RenderFeatures.Add(renderFeature);
                graphicsCompositor.Game = Parameters.Game;
                graphicsCompositor.SingleView = Parameters.SingleView;
                graphicsCompositor.Editor = Parameters.Editor;

                var assetManager = new ContentManager();
                assetManager.Save(Url, graphicsCompositor);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}