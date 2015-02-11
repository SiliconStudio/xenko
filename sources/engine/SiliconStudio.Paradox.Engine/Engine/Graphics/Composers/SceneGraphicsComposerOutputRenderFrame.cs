// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Describes the <see cref="RenderFrame"/> to render to.
    /// </summary>
    [DataContract("SceneGraphicsComposerOutputRenderFrame")]
    [Display("RenderFrame")]
    public sealed class SceneGraphicsComposerOutputRenderFrame : ISceneGraphicsComposerOutput
    {
        private RenderFrame allocatedFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsComposerOutputRenderFrame"/> class.
        /// </summary>
        public SceneGraphicsComposerOutputRenderFrame()
        {
            Descriptor = new RenderFrameDescriptor();
        }

        /// <summary>
        /// Gets or sets the descriptor of the render frame.
        /// </summary>
        /// <value>The descriptor.</value>
        [DataMember(10)]
        public RenderFrameDescriptor Descriptor { get; private set; }

        public void Dispose()
        {
            if (allocatedFrame != null)
            {
                allocatedFrame.RenderTarget.Dispose();
                allocatedFrame.DepthStencil.Dispose();
                allocatedFrame = null;
            }
        }

        public RenderFrame GetRenderFrame(RenderContext context)
        {
            // TODO: Should we use the DrawEffectContext to allocated shared textures?
            if (allocatedFrame != null && allocatedFrame.Descriptor != Descriptor)
            {
                Dispose();
            }

            // Allocate the render frame if necessary
            return allocatedFrame ?? (allocatedFrame = RenderFrame.New(context.GraphicsDevice, Descriptor, context.Tags.Get(RenderFrame.Current)));
        }
    }
}