// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// This assets represents a tree of UI elements. 
    /// </summary>
    [DataContract("UIPageAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(UIPageAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [Display("UI Page")]
    [AssetPartReference(typeof(UIElement))]
    public sealed class UIPageAsset : UIAssetBase
    {
        private const string CurrentVersion = "0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="UIPageAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkuipage";
    }
}
