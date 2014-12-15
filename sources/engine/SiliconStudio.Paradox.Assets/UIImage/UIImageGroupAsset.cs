using System;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Assets.UIImage
{
    /// <summary>
    /// Describes a sprite group asset.
    /// </summary>
    [DataContract("UIImageGroup")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(UIImageGroupCompiler))]
    [ObjectFactory(typeof(UIImageGroupFactory))]
    [ThumbnailCompiler(PreviewerCompilerNames.UIImageGroupThumbnailCompilerQualifiedName, true)]
    [Display("UI Image Group", "An UI Image group")]
    public sealed class UIImageGroupAsset : ImageGroupAsset<UIImageInfo>
    {
        /// <summary>
        /// The default file extension used by the <see cref="UIImageGroupAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxuiimage";
        
        private class UIImageGroupFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new UIImageGroupAsset();
            }
        }
    }
}