// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
    internal class ProceduralModelAssetCompiler : AssetCompilerBase
    {
        public ProceduralModelAssetCompiler()
        {
            CompileTimeDependencyTypes.Add(typeof(MaterialAsset), BuildDependencyType.CompileAsset);
        }

        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (ProceduralModelAsset)assetItem.Asset;
            result.BuildSteps = new ListBuildStep { new GeometricPrimitiveCompileCommand(targetUrlInStorage, asset, assetItem.Package) };
        }

        private class GeometricPrimitiveCompileCommand : AssetCommand<ProceduralModelAsset>
        {
            public GeometricPrimitiveCompileCommand(string url, ProceduralModelAsset parameters, Package package)
                : base(url, parameters, package)
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
 
