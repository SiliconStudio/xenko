// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{

    /// <summary>
    /// Specifies which <see cref="RenderFrame"/> to use when creating the output of <see cref="SceneGraphicsLayer"/> and 
    /// the size mode defined by <see cref="RenderFrameDescriptor.Mode"/> is <see cref="RenderFrameSizeMode.Relative"/>.
    /// </summary>
    public enum RenderFrameRelativeMode
    {
        /// <summary>
        /// The size of the <see cref="RenderFrame"/> is calculated relatively to the current frame.
        /// </summary>
        Current,

        /// <summary>
        /// The size of the <see cref="RenderFrame"/> is calculated relatively to the master frame.
        /// </summary>
        Master
    }

    /// <summary>
    /// Describes the <see cref="RenderFrame"/> to render to.
    /// </summary>
    [DataContract("GraphicsLayerOutputRenderFrame")]
    [Display("RenderFrame")]
    public sealed class GraphicsLayerOutputRenderFrame : RenderFrameProviderBase, IGraphicsLayerOutput
    {
        private RenderFrame allocatedFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsLayerOutputRenderFrame"/> class.
        /// </summary>
        public GraphicsLayerOutputRenderFrame()
        {
            Descriptor = RenderFrameDescriptor.Default();
            RelativeSizeSource = RenderFrameRelativeMode.Current;
        }

        /// <summary>
        /// Gets or sets the descriptor of the render frame.
        /// </summary>
        /// <value>The descriptor.</value>
        [DataMember(10)]
        public RenderFrameDescriptor Descriptor { get; set; }

        /// <summary>
        /// Gets or sets the relative size source.
        /// </summary>
        /// <value>The relative size source.</value>
        [DataMember(20)]
        [DefaultValue(RenderFrameRelativeMode.Current)]
        public RenderFrameRelativeMode RelativeSizeSource { get; set; }

        public override void Dispose()
        {
            if (allocatedFrame != null)
            {
                allocatedFrame.RenderTarget.Dispose();
                allocatedFrame.DepthStencil.Dispose();
                allocatedFrame = null;
            }
        }

        public override RenderFrame GetRenderFrame(RenderContext context)
        {
            // TODO: Should we use the DrawEffectContext to allocated shared textures?
            if (allocatedFrame != null && allocatedFrame.Descriptor != Descriptor)
            {
                Dispose();
            }

            // Allocate the render frame if necessary
            return allocatedFrame ?? (allocatedFrame = RenderFrame.New(context.GraphicsDevice, Descriptor, context.Tags.Get(RelativeSizeSource == RenderFrameRelativeMode.Current ? RenderFrame.Current : SceneGraphicsLayer.Master)));
        }
    }
}