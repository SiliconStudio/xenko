using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Engine
{
    public class UIElementTransformLink : TransformLink
    {
        private readonly UIComponent parentUIComponent;
        private UIElement rootElement;
        private readonly bool forceRecursive;
        private string elementName;
        private UIElement followedElement;

        public UIElementTransformLink(UIComponent parentUIComponent, string elementName, bool forceRecursive)
        {
            this.parentUIComponent = parentUIComponent;
            this.elementName = elementName;
            this.forceRecursive = forceRecursive;
        }

        public TransformTRS Transform;

        private UIElement FindElementByName(string name, UIElement element)
        {
            if (element == null || name == null)
                return null;

            if (name.Equals(element.Name))
                return element;

            foreach (var child in element.VisualChildren)
            {
                var childElement = FindElementByName(name, child);
                if (childElement != null)
                    return childElement;
            }

            return null;
        }

        /// <inheritdoc/>
        public override void ComputeMatrix(bool recursive, out Matrix matrix)
        {
            // If model is not in the parent, we might want to force recursive update (since parentModelComponent might not be updated yet)
            if (forceRecursive || recursive)
            {
                parentUIComponent.Entity.Transform.UpdateWorldMatrix();
            }

            if (parentUIComponent.RootElement != rootElement)
            {
                rootElement = parentUIComponent.RootElement;
                followedElement = FindElementByName(elementName, rootElement);
            }

            // Updated? (rare slow path)
            if (followedElement != null)
            {
                // TODO Or local matrix? Check later
                matrix = followedElement.WorldMatrix;
                return;
            }

            // Fallback to TransformComponent
            matrix = parentUIComponent.Entity.Transform.WorldMatrix;
        }

        public bool NeedsRecreate(Entity parentEntity, string targetNodeName)
        {
            return parentUIComponent.Entity != parentEntity
                || !object.ReferenceEquals(elementName, targetNodeName); // note: supposed to use same string instance so no need to compare content
        }
    }
}
