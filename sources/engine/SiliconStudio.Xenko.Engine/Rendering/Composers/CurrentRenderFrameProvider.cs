// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Output to the Direct (same as the output of the master layer).
    /// </summary>
    [DataContract("CurrentRenderFrameProvider")]
    [Display("Current")]
    public sealed class CurrentRenderFrameProvider : RenderFrameProviderBase, IGraphicsLayerOutput, IImageEffectRendererInput, ISceneRendererOutput
    {
        public static readonly CurrentRenderFrameProvider Instance = new CurrentRenderFrameProvider();

        public override RenderFrame GetRenderFrame(RenderDrawContext context)
        {
            return context.RenderContext.Tags.Get(RenderFrame.Current);
        }
    }
}