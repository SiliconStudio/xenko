using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Rendering.UI
{
    [DefaultPipelinePlugin(typeof(UIPipelinePlugin))]
    public class RenderUIElement : RenderObject
    {
        public RenderUIElement(UIComponent uiComponent, TransformComponent transformComponent)
        {
            UIComponent = uiComponent;
            TransformComponent = transformComponent;
        }

        public readonly UIComponent UIComponent;

        public readonly TransformComponent TransformComponent;

        /// <summary>
        /// Last registered position of teh mouse
        /// </summary>
        public Vector2 LastMousePosition;

        /// <summary>
        /// Last element over which the mouse cursor was registered
        /// </summary>
        public UIElement LastMouseOverElement;

        /// <summary>
        /// Last element which received a touch/click event
        /// </summary>
        public UIElement LastTouchedElement;

        public Vector3 LastIntersectionPoint;

        public Matrix LastRootMatrix;
    }
}
