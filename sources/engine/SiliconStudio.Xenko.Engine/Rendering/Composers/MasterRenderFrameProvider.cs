// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Output to the Direct (same as the output of the master layer).
    /// </summary>
    [DataContract("MasterRenderFrameProvider")]
    [Display("Master")]
    public sealed class MasterRenderFrameProvider : RenderFrameProviderBase, IGraphicsLayerOutput, ISceneRendererOutput
    {
        /// <summary>
        /// Gets a singleton instance.
        /// </summary>
        public static readonly MasterRenderFrameProvider Instance = new MasterRenderFrameProvider();

        public override RenderFrame GetRenderFrame(RenderDrawContext context)
        {
            return context.RenderContext.Tags.Get(SceneGraphicsLayer.Master);
        }
    }
}