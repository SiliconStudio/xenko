// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Assets.Input
{
    public class InputActionConfigurationAssetCompiler : AssetCompilerBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            result.BuildSteps = new AssetBuildStep(assetItem)
            {
                new InputActionMappingAssetCompileCommand(targetUrlInStorage, (InputActionConfigurationAsset)assetItem.Asset)
            };
        }

        internal class InputActionMappingAssetCompileCommand : AssetCommand<InputActionConfigurationAsset>
        {
            public InputActionMappingAssetCompileCommand(string url, InputActionConfigurationAsset parameters) : base(url, parameters)
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