using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    [DataContract("SpriteStudioSheetAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioModelAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".xkss4s;.pdxss4s")] // A description used to display in the asset editor
    [Display("Sprite Studio Sheet")]
    public class SpriteStudioModelAsset : Asset
    {
        /// <summary>
        /// Gets or sets the source file of this asset.
        /// </summary>
        /// <value>The source.</value>
        /// <userdoc>
        /// The source file of this asset.
        /// </userdoc>
        [DataMember(-50)]
        [DefaultValue(null)]
        [SourceFileMember(true)]
        public UFile Source { get; set; } = new UFile("");

        [DataMember(1)]
        [Display(Browsable = false)]
        public List<string> NodeNames { get; set; } = new List<string>();

        [DataMemberIgnore]
        public List<string> BuildTextures { get; } = new List<string>();

        [DataMemberIgnore]
        public override UFile MainSource => Source;
    }
}
