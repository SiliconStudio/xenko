// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Threading;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Stage-specific data for a <see cref="RenderView"/>.
    /// </summary>
    /// Mostly useful to store list of <see cref="RenderNode"/> prefiltered by a <see cref="Rendering.RenderStage"/> and a <see cref="RenderView"/>.
    public struct RenderViewStage
    {
        public readonly int Index;

        /// <summary>
        /// Invalid slot.
        /// </summary>
        public static readonly RenderViewStage Invalid = new RenderViewStage(-1);

        public RenderViewStage(int index)
        {
            Index = index;
            RenderNodes = null;
            SortedRenderNodes = null;
        }

        public RenderViewStage(RenderStage renderStage)
        {
            Index = renderStage.Index;
            RenderNodes = null;
            SortedRenderNodes = null;
        }

        /// <summary>
        /// List of render nodes. It might cover multiple RenderStage and RootRenderFeature. RenderStages contains RenderStage range information.
        /// Used mostly for sorting and rendering.
        /// </summary>
        public ConcurrentCollector<RenderNodeFeatureReference> RenderNodes;

        /// <summary>
        /// Sorted list of render nodes, that should be used during actual drawing.
        /// </summary>
        public FastList<RenderNodeFeatureReference> SortedRenderNodes;

        public static implicit operator RenderViewStage(RenderStage renderStage)
        {
            return new RenderViewStage(renderStage);
        }
    }
}
