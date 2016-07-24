using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    [DataContract("UILibraryAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(UIPageAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [Display("UI")]
    [AssetPartReference(typeof(UIElement))]
    public class UILibraryAsset : UIAssetBase
    {
        private const string CurrentVersion = "0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="UILibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkuilib";

        /// <summary>
        /// Gets the dictionary of publicly exposed controls.
        /// </summary>
        [DataMember(20)]
        public Dictionary<string, Guid> PublicUIElements { get; } = new Dictionary<string, Guid>();
    }
}
