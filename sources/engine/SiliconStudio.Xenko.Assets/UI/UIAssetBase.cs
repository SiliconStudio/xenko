// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// Base class for assets containing a hierarchy of <see cref="UIElement"/>.
    /// </summary>
    [AssetPartReference(typeof(UIElement))]
    public abstract class UIAssetBase : AssetCompositeHierarchy<UIElementDesign, UIElement>
    {
        [DataContract("UIDesign")]
        [NonIdentifiable]
        public sealed class UIDesign
        {
            [DataMember]
            [Display(category: "Design")]
            public Vector3 Resolution { get; set; } = new Vector3(UIComponent.DefaultWidth, UIComponent.DefaultHeight, UIComponent.DefaultDepth);
        }

        [DataMember(10)]
        [NotNull]
        public UIDesign Design { get; set; } = new UIDesign();

        /// <inheritdoc/>
        public override UIElement GetParent(UIElement part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.VisualParent;
        }

        /// <inheritdoc/>
        public override int IndexOf(UIElement part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            var parent = GetParent(part);
            return parent?.VisualChildren.IndexOf(x => x == part) ?? Hierarchy.RootPartIds.IndexOf(part.Id);
        }

        /// <inheritdoc/>
        public override UIElement GetChild(UIElement part, int index)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.VisualChildren[index];
        }

        /// <inheritdoc/>
        public override int GetChildCount(UIElement part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.VisualChildren.Count;
        }

        /// <inheritdoc/>
        public override IEnumerable<UIElement> EnumerateChildParts(UIElement part, bool isRecursive)
        {
            var elementChildren = (IUIElementChildren)part;
            var enumerator = isRecursive ? elementChildren.Children.DepthFirst(t => t.Children) : elementChildren.Children;
            return enumerator.NotNull().Cast<UIElement>();
        }

        /// <inheritdoc/>
        protected override void ClearReferences(AssetCompositeHierarchyData<UIElementDesign, UIElement> clonedHierarchy)
        {
            // set to null reference outside of the sub-tree
            var tempAsset = new UILibraryAsset { Hierarchy = clonedHierarchy };
            tempAsset.FixupReferences();
        }
    }
}
