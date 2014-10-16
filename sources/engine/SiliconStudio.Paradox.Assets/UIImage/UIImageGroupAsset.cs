using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.UIImage
{
    /// <summary>
    /// Describes a sprite group asset.
    /// </summary>
    [DataContract("UIImageGroup")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(UIImageGroupCompiler))]
    [AssetFactory(typeof(UIImageGroupFactory))]
    [ThumbnailCompiler(PreviewerCompilerNames.UIImageGroupThumbnailCompilerQualifiedName)]
    [AssetDescription("UI Image Group", "An UI Image group", true)]
    public sealed class UIImageGroupAsset : ImageGroupAsset<UIImageInfo>
    {
        /// <summary>
        /// The default file extension used by the <see cref="UIImageGroupAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxuiimage";
        
        private class UIImageGroupFactory : IAssetFactory
        {
            public Asset New()
            {
                return new UIImageGroupAsset();
            }
        }
    }
}