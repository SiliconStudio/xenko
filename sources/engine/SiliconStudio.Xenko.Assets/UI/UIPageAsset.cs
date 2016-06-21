// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Assets.Entities;
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
    [Display("UI")]
    public sealed class UIPageAsset : AssetCompositeHierarchy<UIElementDesign, UIElement>
    {
        private const string CurrentVersion = "0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="UIPageAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkui";

        [DataMember]
        [NotNull]
        [Display("Design")]
        public UIDesign Design { get; set; } = new UIDesign();

        [DataContract("UIDesign")]
        [NonIdentifiable]
        public sealed class UIDesign
        {
            [DataMember]
            public float Depth { get; set; } = UIComponent.DefaultDepth;

            [DataMember]
            public float Height { get; set; } = UIComponent.DefaultHeight;

            [DataMember]
            public float Width { get; set; } = UIComponent.DefaultWidth;

            [DataMember]
            public Color AreaBackgroundColor { get; set; } = Color.WhiteSmoke*0.5f;

            [DataMember]
            public Color AreaBorderColor { get; set; } = Color.WhiteSmoke;

            [DataMember]
            public float AreaBorderThickness { get; set; } = 2.0f;

            [DataMember]
            public Color AdornerBackgroundColor { get; set; } = Color.LimeGreen*0.2f;

            [DataMember]
            public Color AdornerBorderColor { get; set; } = Color.LimeGreen;

            [DataMember]
            public float AdornerBorderThickness { get; set; } = 2.0f;
        }

        public override UIElement GetParent(UIElement element)
        {
            return element.Parent;
        }

        public override IEnumerable<UIElement> EnumerateChildParts(UIElement part, bool isRecursive)
        {
            var elementChildren = (IUIElementChildren)part;
            var enumerator = isRecursive ? elementChildren.Children.DepthFirst(t => t.Children) : elementChildren.Children;
            return enumerator.Cast<UIElement>();
        }
    }
}
