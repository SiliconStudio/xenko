using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    internal struct RenderNodeFeatureReference
    {
        public readonly RootRenderFeature RootRenderFeature;
        public readonly RenderNodeReference RenderNode;

        public RenderNodeFeatureReference(RootRenderFeature rootRenderFeature, RenderNodeReference renderNode)
        {
            RootRenderFeature = rootRenderFeature;
            RenderNode = renderNode;
        }
    }

    /// <summary>
    /// Describes a specific <see cref="RenderView"/>, <see cref="RootRenderFeature"/> and <see cref="RenderStage"/> combination.
    /// </summary>
    public struct RenderViewFeatureStage
    {
        public RenderStage RenderStage;

        public int RenderNodeStart;
        public int RenderNodeEnd;

        public RenderViewFeatureStage(RenderStage renderStage, int renderNodeStart, int renderNodeEnd)
        {
            RenderStage = renderStage;
            RenderNodeStart = renderNodeStart;
            RenderNodeEnd = renderNodeEnd;
        }
    }
}