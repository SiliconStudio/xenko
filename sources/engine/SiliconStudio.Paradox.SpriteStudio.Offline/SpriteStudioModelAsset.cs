using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    [DataContract("SpriteStudioSheetAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioModelAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".pdxss4s", false)] // A description used to display in the asset editor
    [ObjectFactory(typeof(SpriteStudioSheetAssetFactory))]
    [Display("Sprite Studio Sheet")]
    public class SpriteStudioModelAsset : AssetImportTracked
    {
        [DataMember(1)]
        [Browsable(false)]
        public List<string> NodeNames { get; set; } = new List<string>();

        [DataMemberIgnore]
        public List<string> BuildTextures { get; } = new List<string>();

        private class SpriteStudioSheetAssetFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new SpriteStudioModelAsset();
            }
        }
    }
}