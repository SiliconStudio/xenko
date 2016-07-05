// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Xenko.Assets.UI
{
    public class UILibraryAssetCompiler : AssetCompilerBase<UILibraryAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, UILibraryAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourcesExist(result, asset, assetAbsolutePath))
                return;

            var parameters = new UILibraryConvertParameters(asset);
            result.BuildSteps = new AssetBuildStep(AssetItem) { new UIConvertCommand(urlInStorage, parameters) };
        }

        /// <summary>
        /// Command used to convert the texture in the storage
        /// </summary>
        public class UIConvertCommand : AssetCommand<UILibraryConvertParameters>
        {
            public UIConvertCommand(string url, UILibraryConvertParameters parameters)
                : base(url, parameters)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                var uiLibrary = new Engine.UILibrary();
                foreach (var kv in AssetParameters.UILibraryAsset.UIElements)
                {
                    // Copy Key/Value pair
                    uiLibrary.UIElements.Add(kv.Key, kv.Value);
                }

                assetManager.Save(Url, uiLibrary);
                return Task.FromResult(ResultStatus.Successful);
            }
        }

        [DataContract]
        public class UILibraryConvertParameters
        {
            public UILibraryConvertParameters(UILibraryAsset uiLibraryAsset)
            {
                UILibraryAsset = uiLibraryAsset;
            }

            [DataMember]
            public UILibraryAsset UILibraryAsset { get; set; }
        }
    }
}
