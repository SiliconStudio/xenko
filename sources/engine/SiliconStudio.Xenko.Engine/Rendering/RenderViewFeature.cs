// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Threading;

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
        public readonly ConcurrentCollector<RenderNodeReference> RenderNodes = new ConcurrentCollector<RenderNodeReference>();

        /// <summary>
        /// The list of object nodes contained in this view.
        /// </summary>
        public readonly ConcurrentCollector<ViewObjectNodeReference> ViewObjectNodes = new ConcurrentCollector<ViewObjectNodeReference>();

        /// <summary>
        /// List of resource layouts used by this render view.
        /// </summary>
        public readonly ConcurrentCollector<ViewResourceGroupLayout> Layouts = new ConcurrentCollector<ViewResourceGroupLayout>();
    }
}
