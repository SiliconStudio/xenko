// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Assets.Input
{
    [AssetCompiler(typeof(InputActionConfigurationAsset), typeof(AssetCompilationContext))]
    public class InputActionConfigurationAssetCompiler : AssetCompilerBase
    {
        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (InputActionConfigurationAsset)assetItem.Asset;
            result.BuildSteps = new AssetBuildStep(assetItem)
            {
                new InputActionMappingAssetCompileCommand(targetUrlInStorage, assetItem, asset)
            };
        }

        internal class InputActionMappingAssetCompileCommand : AssetCommand<InputActionConfigurationAsset>
        {
            public InputActionMappingAssetCompileCommand(string url, AssetItem assetItem, InputActionConfigurationAsset value) : base(url, value, assetItem.Package)
            {
            }
            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var contentManager = new ContentManager();
                
                // The runtime object is basically just a copy of the asset
                var runtimeObject = new InputActionConfiguration
                {
                    Actions = Parameters.Actions,
                };
                contentManager.Save(Url, runtimeObject);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}