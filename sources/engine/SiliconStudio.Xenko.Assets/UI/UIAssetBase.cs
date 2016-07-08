using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
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
    /// Base class for assets containing a hierarchy of <see cref="UIElement"/>.
    /// </summary>
    public abstract class UIAssetBase : AssetCompositeHierarchy<UIElementDesign, UIElement>
    {
        [DataMember(10)]
        [NotNull]
        [Display("Design")]
        public UIDesign Design { get; set; } = new UIDesign();
        
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
            public Color AreaBackgroundColor { get; set; } = Color.WhiteSmoke * 0.5f;

            [DataMember]
            public Color AreaBorderColor { get; set; } = Color.WhiteSmoke;

            [DataMember]
            public float AreaBorderThickness { get; set; } = 2.0f;

            [DataMember]
            public Color AdornerBackgroundColor { get; set; } = Color.LimeGreen * 0.2f;

            [DataMember]
            public Color AdornerBorderColor { get; set; } = Color.LimeGreen;

            [DataMember]
            public float AdornerBorderThickness { get; set; } = 2.0f;
        }
    }
}
