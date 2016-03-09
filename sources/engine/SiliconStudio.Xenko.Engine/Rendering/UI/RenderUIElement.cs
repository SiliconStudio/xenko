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

        public UIElement LastOveredElement;

        public UIElement LastTouchedElement;

        public Vector3 LastIntersectionPoint;

        public Matrix LastRootMatrix;
    }
}