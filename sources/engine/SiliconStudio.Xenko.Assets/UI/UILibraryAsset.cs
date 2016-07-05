// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// This assets represents a tree of UI elements. 
    /// </summary>
    [DataContract("UILibraryAsset")]
    [AssetDescription(FileExtension)]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [Display("UI Library")]
    public class UILibraryAsset : Asset
    {
        private const string CurrentVersion = "0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="UILibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkuilib";

        public UILibraryAsset()
        {
            UIElements = new Dictionary<string, UIElement>();
        }

        /// <summary>
        /// Gets the UI elements.
        /// </summary>
        [DataMember]
        public Dictionary<string, UIElement> UIElements { get; }
    }
}
