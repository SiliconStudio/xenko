using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    [DataContract("SpriteStudioSheetAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioModelAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".xkss4s;.pdxss4s")] // A description used to display in the asset editor
    [Display("Sprite Studio Sheet")]
    public class SpriteStudioModelAsset : AssetImportTracked
    {
        [DataMember(1)]
        [Display(Browsable = false)]
        public List<string> NodeNames { get; set; } = new List<string>();

        [DataMemberIgnore]
        public List<string> BuildTextures { get; } = new List<string>();
    }
}
