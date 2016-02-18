using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Describes a specific <see cref="RenderView"/> and <see cref="RootRenderFeature"/> combination.
    /// </summary>
    public class RenderViewFeature
    {
        public RootRenderFeature RootFeature;

        /// <summary>
        /// List of render nodes. It might cover multiple RenderStage, RenderStages contains range information.
        /// </summary>
        public List<RenderNodeReference> RenderNodes = new List<RenderNodeReference>();

        /// <summary>
        /// The list of object nodes contained in this view.
        /// </summary>
        public List<ViewObjectNodeReference> ViewObjectNodes = new List<ViewObjectNodeReference>();

        /// <summary>
        /// List of resource layouts used by this render view.
        /// </summary>
        public List<ViewResourceGroupLayout> Layouts = new List<ViewResourceGroupLayout>();
    }
}