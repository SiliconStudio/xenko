using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Associate an <see cref="Engine.Entity"/> with design-time data.
    /// </summary>
    [DataContract("UIElementDesign")]
    [NonIdentifiable]
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
        /// Gets or sets the unique identifier of the base UIElement.
        /// </summary>
        [DataMember(20)]
        [DefaultValue(null)]
        public Guid? BaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the part group. If null, the entity doesn't belong to a part group.
        /// </summary>
        [DataMember(30)]
        [DefaultValue(null)]
        public Guid? BasePartInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the entity
        /// </summary>
        [DataMember(40)]
        public UIElement UIElement { get; set; }

        /// <inheritdoc/>
        UIElement IAssetPartDesign<UIElement>.Part => UIElement;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"UIElementDesign [{UIElement.GetType().Name}, {UIElement.Name}]";
        }
    }
}
