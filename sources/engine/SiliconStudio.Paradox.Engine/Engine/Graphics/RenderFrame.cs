// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A render frame is a container for a render target and its depth stencil buffer.
    /// </summary>
    [DataSerializerGlobal(typeof(ReferenceSerializer<RenderFrame>), Profile = "Asset")]
    [ContentSerializer(typeof(DataContentSerializer<RenderFrame>))]
    [DataSerializer(typeof(RenderFrameSerializer))]
    public class RenderFrame
    {
        /// <summary>
        /// The current render frame for the context.
        /// </summary>
        public static readonly ParameterKey<RenderFrame> Current = ParameterKeys.New<RenderFrame>();

        private static readonly PropertyKey<RenderFrame> RenderFrameKey = new PropertyKey<RenderFrame>("RenderFrameKey", typeof(RenderFrame));

        // TODO: Should we move this to Graphics instead?
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderFrame"/> class.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="depthStencil">The depth stencil.</param>
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
        /// Recover a <see cref="RenderFrame" /> from a texture that has been created for a render frame.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="depthStencilTexture">The depth stencil texture.</param>
        /// <returns>The instance of RenderFrame or null if no render frame was used to create this texture.</returns>
        /// <exception cref="System.InvalidOperationException">The texture must be a render target</exception>
        public static RenderFrame FromTexture(Texture texture, Texture depthStencilTexture = null)
        {
            if (texture == null)
            {
                return null;
            }

            if (!texture.IsRenderTarget)
            {
                throw new ArgumentException("The texture must be a render target", "texture");
            }

            if (depthStencilTexture != null && !depthStencilTexture.IsDepthStencil)
            {
                throw new ArgumentException("The texture must be a depth stencil texture", "depthStencilTexture");
            }

            // Retrieve the render frame from the texture if any
            var renderFrame = texture.Tags.Get(RenderFrameKey);

            // OR create a render frame from the specified texture
            if (renderFrame == null)
            {
                var descriptor = new RenderFrameDescriptor();

                // TODO: Check for formats?
                var renderFrameFormat = RenderFrameFormat.LDR;
                if (texture.Format == PixelFormat.R16G16B16A16_Float)
                {
                    renderFrameFormat = RenderFrameFormat.HDR;
                }

                var depthFrameFormat = RenderFrameDepthFormat.None;
                if (depthStencilTexture != null)
                {
                    depthFrameFormat = depthStencilTexture.HasStencil ? RenderFrameDepthFormat.DepthAndStencil : RenderFrameDepthFormat.Depth;
                }

                descriptor.Format = renderFrameFormat;
                descriptor.DepthFormat = depthFrameFormat;
                descriptor.Mode = RenderFrameSizeMode.Fixed;
                descriptor.Width = texture.Width;
                descriptor.Height = texture.Height;

                renderFrame = new RenderFrame(descriptor, texture, depthStencilTexture);
                texture.Tags.Set(RenderFrameKey, renderFrame);
                if (depthStencilTexture != null)
                {
                    depthStencilTexture.Tags.Set(RenderFrameKey, renderFrame);
                }
            }
            return renderFrame;
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

            // Create a render frame.
            var frame = new RenderFrame(
                frameDescriptor,
                Texture.New2D(graphicsDevice, width, height, 1, pixelFormat, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
                frameDescriptor.DepthFormat != RenderFrameDepthFormat.None ? Texture.New2D(graphicsDevice, width, height, 1, depthFormat, TextureFlags.DepthStencil) : null);

            // Attach the render frame to the RenderTarget and DepthStencil
            // in order to be able to recover from it
            frame.RenderTarget.Tags.Set(RenderFrameKey, frame);
            if (frame.DepthStencil != null)
            {
                frame.DepthStencil.Tags.Set(RenderFrameKey, frame);
            }

            return frame;
        }

        internal class RenderFrameSerializer : DataSerializer<RenderFrame>
        {
            public override void PreSerialize(ref RenderFrame renderFrame, ArchiveMode mode, SerializationStream stream)
            {
                // Do not create object during preserialize (OK because not recursive)
            }

            public override void Serialize(ref RenderFrame renderFrame, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Deserialize)
                {
                    var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                    var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();

                    var descriptor = stream.Read<RenderFrameDescriptor>();
                    renderFrame = New(graphicsDeviceService.GraphicsDevice, descriptor);
                }
                else
                {
                    var descriptor = renderFrame.Descriptor;
                    stream.Write(descriptor);
                }
            }
        }
    }
}