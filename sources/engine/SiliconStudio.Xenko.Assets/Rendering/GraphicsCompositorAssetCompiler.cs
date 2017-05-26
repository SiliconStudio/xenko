// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    [AssetCompiler(typeof(GraphicsCompositorAsset), typeof(AssetCompilationContext))]
    public class GraphicsCompositorAssetCompiler : AssetCompilerBase
    {
        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(RenderTextureAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileAsset);
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(TextureAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileAsset);
        }

        public override IEnumerable<Type> GetRuntimeTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            yield return typeof(RendererCoreBase);
        }

        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (GraphicsCompositorAsset)assetItem.Asset;
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
