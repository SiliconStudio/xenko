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
    public class UIPageAssetCompiler : AssetCompilerBase<UIPageAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, UIPageAsset asset, AssetCompilerResult result)
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

                var uiPage = new Engine.UIPage { RootElement = AssetParameters.UIPageAsset.RootElement };
                assetManager.Save(Url, uiPage);

                return Task.FromResult(ResultStatus.Successful);
            }
        }

        [DataContract]
        public class UIConvertParameters
        {
            public UIConvertParameters(UIPageAsset uiPageAsset)
            {
                UIPageAsset = uiPageAsset;
            }

            [DataMember]
            public UIPageAsset UIPageAsset { get; set; }
        }
    }
}
