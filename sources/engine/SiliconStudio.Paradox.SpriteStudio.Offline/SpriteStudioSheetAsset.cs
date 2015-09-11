using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.SpriteStudio.Runtime;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    [DataContract("SpriteStudioSheetAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioSheetAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".pdxss4s", false)] // A description used to display in the asset editor
    [ObjectFactory(typeof(SpriteStudioSheetAssetFactory))]
    [Display("Sprite Studio Sheet")]
    public class SpriteStudioSheetAsset : AssetImportTracked
    {
        public SpriteStudioSheetAsset()
        {
            Nodes = new List<SpriteStudioNode>();
            Textures = new List<string>();
        }

        [DataMember(2)]
        public List<SpriteStudioNode> Nodes { get; private set; }

        public List<string> Textures { get; private set; }

        [DataMemberIgnore]
        public List<string> BuilTextures { get; } = new List<string>();

        private class SpriteStudioSheetAssetFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new SpriteStudioSheetAsset();
            }
        }
    }
}