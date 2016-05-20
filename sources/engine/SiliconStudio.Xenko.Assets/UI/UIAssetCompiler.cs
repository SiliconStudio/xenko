using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    public class UIAssetCompiler : AssetCompilerBase<UIAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, UIAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourcesExist(result, asset, assetAbsolutePath))
                return;

            var parameters = new UIConvertParameters(asset);
            result.BuildSteps = new AssetBuildStep(AssetItem) { new UIConvertCommand(urlInStorage, parameters) };
        }

        /// <summary>
        /// Command used to convert the texture in the storage
        /// </summary>
        public class UIConvertCommand : AssetCommand<UIConvertParameters>
        {
            public UIConvertCommand(string url, UIConvertParameters parameters)
                : base(url, parameters)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                var rootElement = (UIElement)AssetCloner.Clone(AssetParameters.UIAsset.RootElement, AssetClonerFlags.ReferenceAsNull);
                assetManager.Save(Url, rootElement);

                return Task.FromResult(ResultStatus.Successful);
            }
        }

        [DataContract]
        public class UIConvertParameters
        {
            public UIConvertParameters(UIAsset uiAsset)
            {
                UIAsset = uiAsset;
            }

            [DataMember]
            public UIAsset UIAsset { get; set; }
        }
    }
}
