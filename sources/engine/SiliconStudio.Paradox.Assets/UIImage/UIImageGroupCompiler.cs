
using System.Collections.Generic;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.UI;

namespace SiliconStudio.Paradox.Assets.UIImage
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    internal class UIImageGroupCompiler : ImageGroupCompiler<UIImageGroupAsset, UIImageInfo>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, UIImageGroupAsset asset, AssetCompilerResult result)
        {
            var imageToTextureIndex = CompileGroup(context, urlInStorage, assetAbsolutePath, asset, result);

            if(!result.HasErrors)
                result.BuildSteps.Add(new UIImageGroupCommand(urlInStorage, new ImageGroupParameters<UIImageGroupAsset>(asset, context.Platform), imageToTextureIndex));
        }

        internal class UIImageGroupCommand : ImageGroupCommand<UIImageGroupAsset, UIImageInfo, UIImageGroup, UI.UIImage>
        {
            public UIImageGroupCommand(string url, ImageGroupParameters<UIImageGroupAsset> asset, Dictionary<UIImageInfo, int> imageToTextureIndex)
                : base(url, asset, imageToTextureIndex)
            {
            }
        
            protected override void SetImageSpecificFields(UIImageInfo imageInfo, UI.UIImage newImage)
            {
                base.SetImageSpecificFields(imageInfo, newImage);
        
                newImage.Borders = imageInfo.Borders;
            }
        }
    }
}