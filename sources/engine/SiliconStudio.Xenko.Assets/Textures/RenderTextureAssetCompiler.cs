using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.RenderTextures;

namespace SiliconStudio.Xenko.Assets.Textures
{
    public class RenderTextureAssetCompiler : AssetCompilerBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (RenderTextureAsset)assetItem.Asset;
            var colorSpace = context.GetColorSpace();

            result.BuildSteps = new AssetBuildStep(assetItem) { new RenderTextureConvertCommand(targetUrlInStorage, new RenderTextureParameters(asset, colorSpace) ) };
        }

        /// <summary>
        /// Command used to convert the texture in the storage
        /// </summary>
        private class RenderTextureConvertCommand : AssetCommand<RenderTextureParameters>
        {
            public RenderTextureConvertCommand(string url, RenderTextureParameters parameters)
                : base(url, parameters)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();
                assetManager.Save(Url, new RenderTextureDescriptor
                {
                    Width = Parameters.Asset.Width,
                    Height = Parameters.Asset.Height,
                    Format = Parameters.Asset.Format,
                    ColorSpace = Parameters.Asset.IsSRgb(Parameters.ColorSpace) ? ColorSpace.Linear : ColorSpace.Gamma,
                });

                return Task.FromResult(ResultStatus.Successful);
            }
        }

        [DataContract]
        public struct RenderTextureParameters
        {
            public RenderTextureAsset Asset;
            public ColorSpace ColorSpace;

            public RenderTextureParameters(RenderTextureAsset asset, ColorSpace colorSpace)
            {
                Asset = asset;
                ColorSpace = colorSpace;
            }
        }
    }
}