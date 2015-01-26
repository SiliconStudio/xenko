// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets;
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
            result.BuildSteps = new ListBuildStep { new SkyboxCompileCommand(urlInStorage, asset, context.Package) };
        }

        private class SkyboxCompileCommand : AssetCommand<SkyboxAsset>
        {
            private readonly Package package;

            public SkyboxCompileCommand(string url, SkyboxAsset asset, Package package)
                : base(url, asset)
            {
                this.package = package;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                // TODO Convert SkyboxAsset to Skybox and save to Skybox object
                // TODO Add system to prefilter

                var context = new SkyboxGeneratorContext(package);
                var result = SkyboxGenerator.Compile(asset, context);

                if (result.HasErrors)
                {
                    result.CopyTo(commandContext.Logger);
                    return Task.FromResult(ResultStatus.Failed);
                }

                var assetManager = new AssetManager();
                assetManager.Save(Url, result.Skybox);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
 
