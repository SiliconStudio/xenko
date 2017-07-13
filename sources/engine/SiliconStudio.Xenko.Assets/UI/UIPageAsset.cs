// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// This assets represents a tree of UI elements. 
    /// </summary>
    [DataContract("UIPageAsset")]
    [AssetDescription(FileExtension, AllowArchetype = false)]
    [AssetContentType(typeof(UIPage))]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "1.9.0-beta01")]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta01", "1.10.0-beta01", typeof(FixPartReferenceUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.10.0-beta01", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    [AssetUpgrader(XenkoConfig.PackageName, "2.0.0.0", "2.1.0.1", typeof(RootPartIdsToRootPartsUpgrader))]
    [Display("UI page")]
    public sealed class UIPageAsset : UIAssetBase
    {
        private const string CurrentVersion = "2.1.0.1";

        /// <summary>
        /// The default file extension used by the <see cref="UIPageAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkuipage";
    }
}
