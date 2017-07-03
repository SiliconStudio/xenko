// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Materials;
using SiliconStudio.Xenko.Rendering.ProceduralModels;

namespace SiliconStudio.Xenko.Assets.Models
{
    [AssetCompiler(typeof(ProceduralModelAsset), typeof(AssetCompilationContext))]
    internal class ProceduralModelAssetCompiler : AssetCompilerBase
    {
        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetItem assetItem)
        {
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(MaterialAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileAsset);
        }

        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (ProceduralModelAsset)assetItem.Asset;
            result.BuildSteps = new AssetBuildStep(assetItem) { new GeometricPrimitiveCompileCommand(targetUrlInStorage, asset, assetItem.Package) };
        }

        private class GeometricPrimitiveCompileCommand : AssetCommand<ProceduralModelAsset>
        {
            public GeometricPrimitiveCompileCommand(string url, ProceduralModelAsset parameters, IAssetFinder assetFinder)
                : base(url, parameters, assetFinder)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();
                assetManager.Save(Url, new ProceduralModelDescriptor(Parameters.Type));

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
 
