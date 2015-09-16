using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    [DataContract("SpriteStudioSheetAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioSheetAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".pdxss4s", false)] // A description used to display in the asset editor
    [ObjectFactory(typeof(SpriteStudioSheetAssetFactory))]
    [ThumbnailCompiler("SiliconStudio.Paradox.GameStudio.Plugin.ThumbnailCompilers.SpriteStudioSheetThumbnailCompiler, SiliconStudio.Paradox.GameStudio.Plugin", true)] // TODO: Obsolete
    [Display("Sprite Studio Sheet")]
    public class SpriteStudioSheetAsset : AssetImportTracked
    {
        public SpriteStudioSheetAsset()
        {
        }

        [DataMemberIgnore]
        public List<string> BuildTextures { get; } = new List<string>();

        private class SpriteStudioSheetAssetFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new SpriteStudioSheetAsset();
            }
        }
    }
}