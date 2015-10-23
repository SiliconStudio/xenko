// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Describes the <see cref="RenderFrame"/> to render to.
    /// </summary>
    [DataContract("LocalRenderFrameProvider")]
    [Display("RenderFrame")]
    public sealed class LocalRenderFrameProvider : RenderFrameProviderBase, IGraphicsLayerOutput
    {
        private RenderFrame currentFrame;
        private ColorSpace colorSpace;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRenderFrameProvider"/> class.
        /// </summary>
        public LocalRenderFrameProvider()
        {
            Descriptor = RenderFrameDescriptor.Default();
            RelativeSizeSource = RenderFrameRelativeMode.Current;
        }

        /// <summary>
        /// Gets or sets the descriptor of the render frame.
        /// </summary>
        /// <value>The descriptor.</value>
        [DataMember(10)]
        public RenderFrameDescriptor Descriptor;

        /// <summary>
        /// Gets or sets the relative size source.
        /// </summary>
        /// <value>The relative size source.</value>
        [DataMember(20)]
        [DefaultValue(RenderFrameRelativeMode.Current)]
        public RenderFrameRelativeMode RelativeSizeSource { get; set; }

        public override void Dispose()
        {
            if (currentFrame != null)
            {
                currentFrame.Dispose();
                currentFrame = null;
            }
        }

        public override RenderFrame GetRenderFrame(RenderContext context)
        {
            // Get the relative frame
            var relativeFrame = context.Tags.Get(RelativeSizeSource == RenderFrameRelativeMode.Current ? RenderFrame.Current : SceneGraphicsLayer.Master);

            // Check if we need to resize it
            if (currentFrame != null && (currentFrame.Descriptor != Descriptor || currentFrame.CheckIfResizeRequired(relativeFrame) || Descriptor.Format == RenderFrameFormat.LDR && colorSpace != context.GraphicsDevice.ColorSpace))
            {
                Dispose();
            }

            // Store the colorSpace
            colorSpace = context.GraphicsDevice.ColorSpace;

            // Allocate the render frame if necessary
            // TODO: Should we use allocated shared textures from RenderContext?
            return currentFrame ?? (currentFrame = RenderFrame.New(context.GraphicsDevice, Descriptor, relativeFrame));
        }
    }
}