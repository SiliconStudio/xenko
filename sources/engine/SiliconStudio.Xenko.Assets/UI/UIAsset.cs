// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// This assets represents a tree of UI elements. 
    /// </summary>
    [DataContract("UIAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(UIAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [Display("UI")]
    public sealed class UIAsset : Asset
    {
        private const string CurrentVersion = "1.7.0-alpha01";

        /// <summary>
        /// The default file extension used by the <see cref="UIAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkui";

        /// <summary>
        /// Gets or sets the root UI element.
        /// </summary>
        /// <userdoc>The root UI element.</userdoc>
        [DataMember]
        [NotNull]
        [Display("Root Element")]
        public UIElement RootElement { get; set; } = new Grid();
    }
}
