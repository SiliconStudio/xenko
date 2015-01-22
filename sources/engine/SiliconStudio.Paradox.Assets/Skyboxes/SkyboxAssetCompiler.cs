// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Paradox.Assets.Skyboxes
{
    internal class SkyboxAssetCompiler : AssetCompilerBase<SkyboxAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SkyboxAsset asset, AssetCompilerResult result)
        {
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new ListBuildStep { new MaterialCompileCommand(urlInStorage, asset) };
        }

        private class MaterialCompileCommand : AssetCommand<SkyboxAsset>
        {
            public MaterialCompileCommand(string url, SkyboxAsset asset)
                : base(url, asset)
            {
            }

            public override System.Collections.Generic.IEnumerable<ObjectUrl> GetInputFiles()
            {
                if (asset.Model != null)
                {
                    foreach (var contentReference in asset.Model.GetDependencies())
                    {
                        yield return new ObjectUrl(UrlType.Internal, contentReference.Location);
                    }
                }

                foreach (var inputFile in base.GetInputFiles())
                    yield return inputFile;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {

                // TODO Convert SkyboxAsset to Skybox and save to Skybox object

                // TODO Add system to prefilter


                //var materialData = new Material { Parameters = materialContext.Parameters};
                
                //var assetManager = new AssetManager();
                //assetManager.Save(assetUrl, materialData);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
 
