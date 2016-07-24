// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Assets.Entities;

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
                foreach (var kv in AssetParameters.UILibraryAsset.PublicUIElements)
                {

                    if (!AssetParameters.UILibraryAsset.Hierarchy.RootPartIds.Contains(kv.Value))
                    {
                        // We might want to allow that in the future.
                        commandContext.Logger.Warning($"Only root elements can be exposed publicly. Skipping [{kv.Key}].");
                        continue;
                    }

                    // Copy Key/Value pair
                    UIElementDesign element;
                    if (AssetParameters.UILibraryAsset.Hierarchy.Parts.TryGetValue(kv.Value, out element))
                    {
                        uiLibrary.UIElements.Add(kv.Key, element.UIElement);
                    }
                    else
                    {
                        commandContext.Logger.Error($"Cannot find the element with the id [{kv.Value}] to expose [{kv.Key}].");
                    }
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
