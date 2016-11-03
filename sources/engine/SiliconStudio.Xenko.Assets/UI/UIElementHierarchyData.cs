using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// Associate an <see cref="UIElement"/> with design-time data.
    /// </summary>
    [DataContract("UIElementDesign")]
    public class UIElementDesign : IAssetPartDesign<UIElement>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UIElementDesign"/>.
        /// </summary>
        public UIElementDesign()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UIElementDesign"/>.
        /// </summary>
        /// <param name="uiElement">The UI Element</param>
        public UIElementDesign(UIElement uiElement)
        {
            UIElement = uiElement;
        }

        /// <summary>
        /// Gets or sets the entity
        /// </summary>
        [DataMember(10)]
        public UIElement UIElement { get; set; }

        /// <inheritdoc/>
        [DataMember(20)]
        [DefaultValue(null)]
        public BasePart Base { get; set; }

        /// <inheritdoc/>
        UIElement IAssetPartDesign<UIElement>.Part => UIElement;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"UIElementDesign [{UIElement.GetType().Name}, {UIElement.Name}]";
        }
    }
}
