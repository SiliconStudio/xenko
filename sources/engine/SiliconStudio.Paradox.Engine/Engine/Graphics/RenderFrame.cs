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
    public class RenderFrame : IDisposable
    {
        private readonly bool isOwner;

        private readonly Texture ReferenceTexture;

        /// <summary>
        /// Property key to access the Current <see cref="RenderFrame"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<RenderFrame> Current = new PropertyKey<RenderFrame>("RenderFrame.Current", typeof(RenderFrame));

        // TODO: Should we move this to Graphics instead?
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderFrame" /> class.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="renderTargets">The render target.</param>
        /// <param name="depthStencil">The depth stencil.</param>
        /// <param name="isOwner">if set to <c>true</c> this instance is owning the rendertargets and depth stencil buffer.</param>
        private RenderFrame(RenderFrameDescriptor descriptor, Texture[] renderTargets, Texture depthStencil, bool isOwner)
        {
            Descriptor = descriptor;
            RenderTargets = renderTargets;
            DepthStencil = depthStencil;
            this.isOwner = isOwner;
            if (renderTargets != null)
            {
                foreach (var renderTarget in renderTargets)
                {
                    if (renderTarget != null)
                    {
                        ReferenceTexture = renderTarget;
                        break;
                    }
                }
            }
            if (ReferenceTexture == null && depthStencil != null)
            {
                ReferenceTexture = depthStencil;
            }
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
        public Texture[] RenderTargets { get; private set; }

        /// <summary>
        /// Gets or sets the depth stencil.
        /// </summary>
        /// <value>The depth stencil.</value>
        public Texture DepthStencil { get; private set; }

        public int Width
        {
            get
            {
                return ReferenceTexture.Width;

            }
        }

        public int Height
        {
            get
            {
                return ReferenceTexture.Height;
            }
        }

        public void Dispose()
        {
            if (isOwner)
            {
                foreach (var renderTarget in RenderTargets)
                {
                    if (renderTarget != null)
                    {
                        renderTarget.Dispose();
                    }
                }

                if (Descriptor.DepthFormat != RenderFrameDepthFormat.Shared && DepthStencil != null)
                {
                    DepthStencil.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks if resizing this instance is required.
        /// </summary>
        /// <param name="referenceFrame">The reference frame.</param>
        /// <returns><c>true</c> if resizing this instance is required, <c>false</c> otherwise.</returns>
        public bool CheckIfResizeRequired(RenderFrame referenceFrame)
        {
            if (Descriptor.Mode == RenderFrameSizeMode.Relative && referenceFrame != null)
            {
                var renderTarget = referenceFrame.RenderTargets[0];

                var targetWidth = (int)((double)Descriptor.Width * renderTarget.Width / 100);
                var targetHeight = (int)((double)Descriptor.Height * renderTarget.Height / 100);

                return renderTarget.Width != targetWidth || renderTarget.Height != targetHeight;
            }
            return false;
        }

        /// <summary>
        /// Activates the specified render context.
        /// </summary>
        /// <param name="renderContext">The render context.</param>
        /// <exception cref="System.ArgumentNullException">renderContext</exception>
        public void Activate(RenderContext renderContext)
        {
            if (renderContext == null) throw new ArgumentNullException("renderContext");

            // TODO: Handle support for shared depth stencil buffer

            // Sets the depth and render target
            renderContext.GraphicsDevice.SetDepthAndRenderTargets(DepthStencil, RenderTargets);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="RenderFrame"/> to <see cref="Texture"/>.
        /// </summary>
        /// <param name="from">The render frame.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Texture(RenderFrame from)
        {
            return from != null ? from.RenderTargets[0] : null;
        }

        /// <summary>
        /// Recover a <see cref="RenderFrame" /> from a texture that has been created for a render frame.
        /// </summary>
        /// <param name="renderTextures">The texture.</param>
        /// <param name="depthStencilTexture">The depth stencil texture.</param>
        /// <returns>The instance of RenderFrame or null if no render frame was used to create this texture.</returns>
        /// <exception cref="System.InvalidOperationException">The texture must be a render target</exception>
        public static RenderFrame FromTexture(Texture[] renderTextures, Texture depthStencilTexture = null)
        {
            Texture referenceTexture = null;

            if (renderTextures != null)
            {
                foreach (var renderTexture in renderTextures)
                {
                    if (renderTexture != null && !renderTexture.IsRenderTarget)
                    {
                        throw new ArgumentException("The texture must be a render target", "renderTextures");
                    }

                    if (referenceTexture == null && renderTexture != null)
                    {
                        referenceTexture = renderTexture;
                    }
                    else if (renderTexture != null)
                    {
                        if (referenceTexture.Width != renderTexture.Width || referenceTexture.Height != renderTexture.Height)
                        {
                            throw new ArgumentException("Invalid textures. The textures must have the same width/height", "renderTextures");
                        }
                    }
                }
            }

            if (depthStencilTexture != null && !depthStencilTexture.IsDepthStencil)
            {
                throw new ArgumentException("The texture must be a depth stencil texture", "depthStencilTexture");
            }

            if (referenceTexture == null && depthStencilTexture != null)
            {
                referenceTexture = depthStencilTexture;
            }
            else if (depthStencilTexture != null)
            {
                if (referenceTexture.Width != depthStencilTexture.Width || referenceTexture.Height != depthStencilTexture.Height)
                {
                    throw new ArgumentException("Invalid textures/depthstencil. The textures must have the same width/height", "depthStencilTexture");
                }
            }

            // If no relevant textures, than return null
            if (referenceTexture == null)
            {
                return null;
            }

            var descriptor = RenderFrameDescriptor.Default();

            // TODO: Check for formats?
            var renderFrameFormat = RenderFrameFormat.LDR;
            if (referenceTexture.Format == PixelFormat.R16G16B16A16_Float)
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
            descriptor.Width = referenceTexture.Width;
            descriptor.Height = referenceTexture.Height;

            return new RenderFrame(descriptor, renderTextures, depthStencilTexture, false);
        }

        /// <summary>
        /// Recover a <see cref="RenderFrame" /> from a texture that has been created for a render frame.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="depthStencilTexture">The depth stencil texture.</param>
        /// <returns>The instance of RenderFrame or null if no render frame was used to create this texture.</returns>
        /// <exception cref="System.InvalidOperationException">The texture must be a render target</exception>
        public static RenderFrame FromTexture(Texture texture, Texture depthStencilTexture = null)
        {
            if (texture == null && depthStencilTexture == null)
            {
                return null;
            }

            return FromTexture(new [] { texture }, depthStencilTexture);
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

            var referenceTexture = referenceFrame != null ? referenceFrame.ReferenceTexture : graphicsDevice.BackBuffer;

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

            // Create the render target
            var renderTarget = Texture.New2D(graphicsDevice, width, height, 1, pixelFormat, TextureFlags.RenderTarget | TextureFlags.ShaderResource);

            // Create the depth stencil buffer
            Texture depthStencil = null;

            // TODO: Better handle the case where shared cannot be used. Should we throw an exception?
            if (frameDescriptor.DepthFormat == RenderFrameDepthFormat.Shared && referenceFrame != null && referenceFrame.DepthStencil != null &&
                referenceFrame.DepthStencil.Width == width && referenceFrame.DepthStencil.Height == height)
            {
                depthStencil = referenceFrame.DepthStencil;
            }
            else if (frameDescriptor.DepthFormat == RenderFrameDepthFormat.Depth || frameDescriptor.DepthFormat == RenderFrameDepthFormat.DepthAndStencil)
            {
                depthStencil = Texture.New2D(graphicsDevice, width, height, 1, depthFormat, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
            }

            // Create a render frame.
            return new RenderFrame(frameDescriptor, new [] { renderTarget }, depthStencil, true);
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