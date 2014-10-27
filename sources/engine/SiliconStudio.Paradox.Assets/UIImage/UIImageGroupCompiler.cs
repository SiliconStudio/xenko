using System.Collections.Generic;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.UI.Data;

namespace SiliconStudio.Paradox.Assets.UIImage
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    internal class UIImageGroupCompiler : ImageGroupCompiler<UIImageGroupAsset, UIImageInfo>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, UIImageGroupAsset asset, AssetCompilerResult result)
        {
            base.Compile(context, urlInStorage, assetAbsolutePath, asset, result);

            if(!result.HasErrors)
                result.BuildSteps.Add(new UIImageGroupCommand(urlInStorage, 
                    new ImageGroupParameters<UIImageGroupAsset>(asset, context.Platform, context.GetGraphicsPlatform(), context.GetGraphicsProfile(), context.GetTextureQuality()),
                    SpriteToTextureIndex, SeparateAlphaTexture));
        }

        internal class UIImageGroupCommand : ImageGroupCommand<UIImageGroupAsset, UIImageInfo, UIImageGroupData, UIImageData>
        {
            public UIImageGroupCommand(string url, ImageGroupParameters<UIImageGroupAsset> asset, Dictionary<UIImageInfo, int> imageToTextureIndex, bool separateAlpha)
                : base(url, asset, imageToTextureIndex, separateAlpha)
            {
            }

            protected override void SetImageSpecificFields(UIImageInfo imageInfo, UIImageData newImage)
            {
                base.SetImageSpecificFields(imageInfo, newImage);

                newImage.Borders = imageInfo.Borders;
            }
        }
    }
}