// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.SpriteStudio.Runtime;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    [DataContract("SpriteStudioSheetAsset")] // Name of the Asset serialized in YAML
    [AssetContentType(typeof(SpriteStudioSheet))]
    [AssetDescription(FileExtension)] // A description used to display in the asset editor
    [Display("Sprite Studio Sheet")]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "0.0.0")]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public class SpriteStudioModelAsset : Asset
    {
        public const string FileExtension = ".xkss4s";

        private const string CurrentVersion = "2.0.0.0";

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
