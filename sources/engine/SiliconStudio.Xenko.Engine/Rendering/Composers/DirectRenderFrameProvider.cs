// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Renders directly to a custom <see cref="RenderFrame"/>.
    /// </summary>
    [DataContract("DirectRenderFrameProvider")]
    [Display("Direct")]
    public sealed class DirectRenderFrameProvider : RenderFrameProviderBase, IGraphicsLayerOutput, IImageEffectRendererInput, ISceneRendererOutput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectRenderFrameProvider"/> class.
        /// </summary>
        /// <param name="renderFrame">The render frame.</param>
        public DirectRenderFrameProvider(RenderFrame renderFrame) // Provide only this constructor so that this class doesn't appear in the editor
        {
            RenderFrame = renderFrame;
        }

        /// <summary>
        /// Gets or sets the render frame.
        /// </summary>
        /// <value>The render frame.</value>
        /// <userdoc>The render frame to use.</userdoc>
        public RenderFrame RenderFrame { get; set; }

        public override RenderFrame GetRenderFrame(RenderContext context)
        {
            return RenderFrame;
        }
    }
}