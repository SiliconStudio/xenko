// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// This assets represents a tree of UI elements. 
    /// </summary>
    [DataContract("UIAsset")]
    [AssetDescription(FileExtension)]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [ObjectFactory(typeof(UIFactory))]
    [Display("UI")]
    public sealed class UIAsset : Asset
    {
        private const string CurrentVersion = "1.7.0-alpha01";

        /// <summary>
        /// The default file extension used by the <see cref="UIAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkui";

        public UIAsset()
        {
            SetDefaults();
        }

        /// <summary>
        /// Gets or sets the fonts used by the UI.
        /// </summary>
        /// <userdoc>The fonts used by the UI.</userdoc>
        [DataMember(10)]
        public List<Graphics.SpriteFont> Fonts { get; set; } = new List<Graphics.SpriteFont>();

        /// <summary>
        /// Gets or sets the root UI element.
        /// </summary>
        /// <userdoc>The root UI element.</userdoc>
        [DataMember(100)]
        [NotNull]
        [Display("Root Element")]
        public UIElement RootElement { get; set; }

        /// <summary>
        /// Gets or sets the sprites used by the UI.
        /// </summary>
        /// <userdoc>The sprites used by the UI.</userdoc>
        [DataMember(20)]
        public Graphics.SpriteSheet Sprites { get; set; }

        public override void SetDefaults()
        {
            RootElement = new Grid();
        }

        private class UIFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new UIAsset();
            }
        }
    }
}
