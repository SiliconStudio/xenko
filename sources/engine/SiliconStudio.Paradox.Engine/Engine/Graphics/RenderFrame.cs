// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A render frame is a container for a render target and its depth stencil buffer.
    /// </summary>
    public class RenderFrame
    {
        private static readonly PropertyKey<RenderFrame> RenderFrameKey = new PropertyKey<RenderFrame>("RenderFrameKey", typeof(RenderFrame));

        // TODO: Should we move this to Graphics instead?
        private RenderFrame(RenderFrameDescriptor descriptor, Texture renderTarget, Texture depthStencil)
        {
            Descriptor = descriptor;
            RenderTarget = renderTarget;
            DepthStencil = depthStencil;
        }

        /// <summary>
        /// Gets the descriptor of this render frame.
        /// </summary>
        /// <value>The descriptor.</value>
        public RenderFrameDescriptor Descriptor { get; private set; }

        /// <summary>
        /// Gets or sets the render target.
        /// </summary>
        /// <value>The render target.</value>
        public Texture RenderTarget { get; internal set; }

        /// <summary>
        /// Gets or sets the depth stencil.
        /// </summary>
        /// <value>The depth stencil.</value>
        public Texture DepthStencil { get; internal set; }

        /// <summary>
        /// Recover a <see cref="RenderFrame"/> from a texture that has been created for a render frame.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <returns>The instance of RenderFrame or null if no render frame was used to create this texture.</returns>
        public static RenderFrame FromTexture(Texture texture)
        {
            if (texture == null)
            {
                return null;
            }
            return texture.Tags.Get(RenderFrameKey);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RenderFrame"/> from the specified parameters.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="frameDescriptor">The frame descriptor.</param>
        /// <param name="referenceFrame">The reference frame, when using relative mode for <see cref="RenderFrameDescriptor.Mode"/>.</param>
        /// <returns>A new instance of <see cref="RenderFrame"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// graphicsDevice
        /// or
        /// frameDescriptor
        /// </exception>
        public static RenderFrame New(GraphicsDevice graphicsDevice, RenderFrameDescriptor frameDescriptor, RenderFrame referenceFrame = null)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (frameDescriptor == null) throw new ArgumentNullException("frameDescriptor");

            var referenceTexture = graphicsDevice.BackBuffer;
            if (referenceFrame != null && referenceFrame.RenderTarget != null)
                referenceTexture = referenceFrame.RenderTarget;

            int width = frameDescriptor.Width;
            int height = frameDescriptor.Height;

            if (frameDescriptor.Mode == RenderFrameSizeMode.Relative)
            {
                width = (width * referenceTexture.Width) / 100;
                height = (height * referenceTexture.Height) / 100;
            }

            var pixelFormat = PixelFormat.R8G8B8A8_UNorm;
            if (frameDescriptor.Format == RenderFrameFormat.HDR)
            {
                pixelFormat = PixelFormat.R16G16B16A16_Float;
            }

            var depthFormat = PixelFormat.None;
            switch (frameDescriptor.DepthFormat)
            {
                case RenderFrameDepthFormat.Depth:
                    depthFormat = PixelFormat.D32_Float;
                    break;
                case RenderFrameDepthFormat.DepthAndStencil:
                    depthFormat = PixelFormat.D24_UNorm_S8_UInt;
                    break;
            }

            var frame = new RenderFrame(
                frameDescriptor,
                Texture.New2D(graphicsDevice, width, height, 1, pixelFormat, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
                frameDescriptor.DepthFormat != RenderFrameDepthFormat.None ? Texture.New2D(graphicsDevice, width, height, 1, depthFormat, TextureFlags.DepthStencil) : null);

            // Set the render frame 
            frame.RenderTarget.Tags.Set(RenderFrameKey, frame);
            if (frame.DepthStencil != null)
            {
                frame.DepthStencil.Tags.Set(RenderFrameKey, frame);
            }

            return frame;
        }
    }
}