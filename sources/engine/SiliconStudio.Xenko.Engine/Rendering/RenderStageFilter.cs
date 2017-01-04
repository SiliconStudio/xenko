using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines a way to filter RenderObject.
    /// </summary>
    [DataContract("RenderStageFilter")]
    public abstract class RenderStageFilter
    {
        public abstract bool IsVisible(RenderObject renderObject, RenderView renderView, RenderViewStage renderViewStage);
    }
}