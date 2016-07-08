using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// Base class for assets containing a hierarchy of <see cref="UIElement"/>.
    /// </summary>
    public abstract class UIAssetBase : AssetCompositeHierarchy<UIElementDesign, UIElement>
    {
        /// <inheritdoc/>
        public override UIElement GetParent(UIElement part)
        {
            return part.Parent;
        }

        /// <inheritdoc/>
        public override IEnumerable<UIElement> EnumerateChildParts(UIElement part, bool isRecursive)
        {
            var elementChildren = (IUIElementChildren)part;
            var enumerator = isRecursive ? elementChildren.Children.DepthFirst(t => t.Children) : elementChildren.Children;
            return enumerator.NotNull().Cast<UIElement>();
        }
    }
}
