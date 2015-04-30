// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Composers;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// A link to a shared <see cref="RenderFrame"/>.
    /// </summary>
    [DataContract("SharedRenderFrameProvider")]
    [Display("Shared RenderFrame")]
    public sealed class SharedRenderFrameProvider : RenderFrameProviderBase, IGraphicsLayerOutput, IImageEffectRendererInput, ISceneRendererOutput
    {
        /// <summary>
        /// Gets or sets the shared RenderFrame.
        /// </summary>
        /// <value>The shared RenderFrame.</value>
        [DataMember(10)]
        [DefaultValue(null)]
        public RenderFrame RenderFrame { get; set; }

        public override RenderFrame GetRenderFrame(RenderContext context)
        {
            return RenderFrame;
        }
    }
}