// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Describes the <see cref="RenderFrame"/> to render to.
    /// </summary>
    [DataContract("GraphicsComposerOutputRenderFrame")]
    [Display("RenderFrame")]
    public class GraphicsComposerOutputRenderFrame : IGraphicsComposerOutput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsComposerOutputRenderFrame"/> class.
        /// </summary>
        public GraphicsComposerOutputRenderFrame()
        {
            Descriptor = new RenderFrameDescriptor();
        }

        /// <summary>
        /// Gets or sets the descriptor of the render frame.
        /// </summary>
        /// <value>The descriptor.</value>
        [DataMember(10)]
        public RenderFrameDescriptor Descriptor { get; private set; }
    }
}