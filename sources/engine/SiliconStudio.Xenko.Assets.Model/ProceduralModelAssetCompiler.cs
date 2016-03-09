// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Rendering.ProceduralModels;

namespace SiliconStudio.Xenko.Assets.Model
{
    internal class ProceduralModelAssetCompiler : AssetCompilerBase<ProceduralModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, ProceduralModelAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep { new GeometricPrimitiveCompileCommand(urlInStorage, asset) };
        }

        private class GeometricPrimitiveCompileCommand : AssetCommand<ProceduralModelAsset>
        {
            public GeometricPrimitiveCompileCommand(string url, ProceduralModelAsset assetParameters)
                : base(url, assetParameters)
            {
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();
                assetManager.Save(Url, new ProceduralModelDescriptor(AssetParameters.Type));

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
 
