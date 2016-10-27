// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Input
{
    public class InputMappingAssetCompiler : AssetCompilerBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            result.BuildSteps.Add(new InputMappingCommand(targetUrlInStorage, (InputMappingAsset)assetItem.Asset));
        }

        private class InputMappingCommand : AssetCommand<InputMappingAsset>
        {
            private string targetUrl;
            private InputMappingAsset asset;

            public InputMappingCommand(string url, InputMappingAsset parameters) : base(url, parameters)
            {
                targetUrl = url;
                asset = parameters;
            }
            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                ContentManager contentManager = new ContentManager();
                //InputMapping generatedInputMapping = new InputMapping();
                //generatedInputMapping.Bindings = new List<InputBinding>();
                //
                //foreach (var b in asset.Bindings)
                //{
                //    var ib = new InputBinding();
                //    ib.DefaultMappings = new List<IVirtualButtonDesc>();
                //    foreach (var m in b.DefaultMappings)
                //    {
                //        ib.DefaultMappings.Add(m);
                //    }
                //    generatedInputMapping.Bindings.Add(ib);
                //}
                //
                //contentManager.Save(targetUrl, generatedInputMapping);
                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}