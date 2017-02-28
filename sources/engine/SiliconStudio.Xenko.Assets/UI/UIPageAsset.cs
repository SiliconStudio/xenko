// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
    [AssetCompiler(typeof(UIPageAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [Display("UI Page")]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "1.9.0-beta01", typeof(BasePartsRemovalComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta01", "1.10.0-beta01", typeof(FixPartReferenceUpgrader))]
    public sealed class UIPageAsset : UIAssetBase
    {
        private const string CurrentVersion = "1.10.0-beta01";

        /// <summary>
        /// The default file extension used by the <see cref="UIPageAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkuipage";
    }
}
