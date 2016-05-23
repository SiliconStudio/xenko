using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Stage-specific data for a <see cref="RenderView"/>.
    /// </summary>
    /// Mostly useful to store list of <see cref="RenderNode"/> prefiltered by a <see cref="Rendering.RenderStage"/> and a <see cref="RenderView"/>.
    public class RenderViewStage
    {
        public readonly RenderStage RenderStage;

        /// <summary>
        /// List of render nodes. It might cover multiple RenderStage and RootRenderFeature. RenderStages contains RenderStage range information.
        /// Used mostly for sorting and rendering.
        /// </summary>
        public readonly List<RenderNodeFeatureReference> RenderNodes = new List<RenderNodeFeatureReference>();

        /// <summary>
        /// Sorted list of render nodes, that should be used during actual drawing.
        /// </summary>
        public RenderNodeFeatureReference[] SortedRenderNodes;

        public RenderViewStage(RenderStage renderStage)
        {
            RenderStage = renderStage;
        }

        public static implicit operator RenderViewStage(RenderStage renderStage)
        {
            return new RenderViewStage(renderStage);
        }

        public override string ToString()
        {
            return $"{RenderStage}: {RenderNodes.Count} node(s)";
        }
    }
}