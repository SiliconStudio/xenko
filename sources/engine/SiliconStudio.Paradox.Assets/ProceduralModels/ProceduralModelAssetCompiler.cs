// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Rendering.ProceduralModels;

namespace SiliconStudio.Paradox.Assets.ProceduralModels
{
    internal class ProceduralModelAssetCompiler : AssetCompilerBase<ProceduralModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, ProceduralModelAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep { new GeometricPrimitiveCompileCommand(urlInStorage, asset, context.Package) };
        }

        private class GeometricPrimitiveCompileCommand : AssetCommand<ProceduralModelAsset>
        {
            public GeometricPrimitiveCompileCommand(string url, ProceduralModelAsset asset, Package package)
                : base(url, asset)
            {
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();
                assetManager.Save(Url, new ProceduralModelDescriptor(asset.Type));

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
 
