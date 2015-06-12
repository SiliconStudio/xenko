
using System.Collections.Generic;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Sprite
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    internal class SpriteGroupCompiler : ImageGroupCompiler<SpriteGroupAsset, SpriteInfo>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SpriteGroupAsset asset, AssetCompilerResult result)
        {
            var imageToTextureIndex = CompileGroup(context, urlInStorage, assetAbsolutePath, asset, result);

            if (!result.HasErrors)
                result.BuildSteps.Add(new SpriteGroupCommand(urlInStorage, new ImageGroupParameters<SpriteGroupAsset>(asset, context.Platform), imageToTextureIndex));
        }

        internal class SpriteGroupCommand : ImageGroupCommand<SpriteGroupAsset, SpriteInfo, SpriteGroup, Graphics.Sprite>
        {
            public SpriteGroupCommand(string url, ImageGroupParameters<SpriteGroupAsset> asset, Dictionary<SpriteInfo, int> imageToTextureIndex)
                : base(url, asset, imageToTextureIndex)
            {
            }
        
            protected override void SetImageSpecificFields(SpriteInfo imageInfo, Graphics.Sprite newImage)
            {
                base.SetImageSpecificFields(imageInfo, newImage);
        
                newImage.Center = imageInfo.Center + (imageInfo.CenterFromMiddle ? +new Vector2(imageInfo.TextureRegion.Width, imageInfo.TextureRegion.Height) / 2 : Vector2.Zero);
            }
        }
    }
}