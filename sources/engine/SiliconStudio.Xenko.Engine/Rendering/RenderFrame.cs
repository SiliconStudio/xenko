// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A render frame is a container for a render target and its depth stencil buffer.
    /// </summary>
    [DataSerializerGlobal(typeof(ReferenceSerializer<RenderFrame>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<RenderFrame>))]
    [DataSerializer(typeof(RenderFrameSerializer))]
    public class RenderFrame : IDisposable
    {
        private bool isOwner;
        private Texture ReferenceTexture;

        /// <summary>
        /// Property key to access the Current <see cref="RenderFrame"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<RenderFrame> Current = new PropertyKey<RenderFrame>("RenderFrame.Current", typeof(RenderFrame));

        /// <summary>
        /// Creates a new render for serialization
        /// </summary>
        public RenderFrame()
        {
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
                if (RenderTargets != null)
                {
                    foreach (var renderTarget in RenderTargets)
                    {
                        if (renderTarget != null)
                        {
                            renderTarget.Dispose();
                        }
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
                var targetWidth = (int)((double)Descriptor.Width * referenceFrame.Width / 100);
                var targetHeight = (int)((double)Descriptor.Height * referenceFrame.Height / 100);

                return Width != targetWidth || Height != targetHeight;
            }
            return false;
        }

        /// <summary>
        /// Activates the specified render context.
        /// </summary>
        /// <param name="renderContext">The render context.</param>
        /// <param name="enableDepth">if set to <c>true</c> [enable depth].</param>
        /// <exception cref="System.ArgumentNullException">renderContext</exception>
        public void Activate(RenderDrawContext renderContext, bool enableDepth = true)
        {
            if (renderContext == null) throw new ArgumentNullException("renderContext");

            // TODO: Handle support for shared depth stencil buffer

            renderContext.CommandList.SetRenderTargetsAndViewport(enableDepth ? DepthStencil : null, RenderTargets);
        }

        public void Activate(RenderDrawContext renderContext, Texture depthStencilTexture)
        {
            if (renderContext == null) throw new ArgumentNullException("renderContext");

            renderContext.CommandList.SetRenderTargetsAndViewport(depthStencilTexture, RenderTargets);
        }


        /// <summary>
        /// Gets a <see cref="RenderOutputDescription"/> that matches current depth stencil and render target formats.
        /// </summary>
        /// <returns>The <see cref="RenderOutputDescription"/>.</returns>
        public unsafe RenderOutputDescription GetRenderOutputDescription()
        {
            var result = new RenderOutputDescription
            {
                DepthStencilFormat = DepthStencil != null ? DepthStencil.ViewFormat : PixelFormat.None,
                MultiSampleLevel = DepthStencil != null ? DepthStencil.MultiSampleLevel : MSAALevel.None,
            };

            if (RenderTargets != null)
            {
                result.RenderTargetCount = RenderTargets.Length;
                var renderTargetFormat = &result.RenderTargetFormat0;
                for (int i = 0; i < RenderTargets.Length; ++i)
                {
                    *renderTargetFormat++ = RenderTargets[i].ViewFormat;
                    result.MultiSampleLevel = RenderTargets[i].MultiSampleLevel; // multisample should all be equal
                }
            }

            return result;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="RenderFrame"/> to <see cref="Texture"/>.
        /// </summary>
        /// <param name="from">The render frame.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Texture(RenderFrame from)
        {
            return from != null && from.RenderTargets != null && from.RenderTargets.Length > 0 ? from.RenderTargets[0] : null;
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
                    if (!renderTexture.IsRenderTarget)
                    {
                        throw new ArgumentException("The texture must be a render target", "renderTextures");
                    }

                    if (referenceTexture == null)
                    {
                        referenceTexture = renderTexture;
                    }
                    else
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

            var renderFrame = new RenderFrame();
            renderFrame.InitializeFrom(descriptor, renderTextures, depthStencilTexture, false);
            return renderFrame;
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

            return FromTexture(texture != null ? new[] { texture } : null, depthStencilTexture);
        }

        /// <summary>
        /// Creates a fake instance of <see cref="RenderFrame"/> for serialization.
        /// </summary>
        /// <param name="frameDescriptor">The frame descriptor.</param>
        /// <returns>A new instance of <see cref="RenderFrame"/>.</returns>
        public static RenderFrame NewFake(RenderFrameDescriptor frameDescriptor)
        {
            return new RenderFrame { Descriptor = frameDescriptor};
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
            // Just return null if no render frame is defined
            if (frameDescriptor.DepthFormat == RenderFrameDepthFormat.None && frameDescriptor.Format == RenderFrameFormat.None)
                return null;

            var renderFrame = new RenderFrame();
            renderFrame.InitializeFrom(graphicsDevice, frameDescriptor, referenceFrame);
            return renderFrame;
        }

        internal void InitializeFrom(GraphicsDevice device, RenderFrameDescriptor description, RenderFrame referenceFrame = null)
        {
            if (device == null) throw new ArgumentNullException("device");

            if (description.DepthFormat == RenderFrameDepthFormat.None && description.Format == RenderFrameFormat.None)
                return;

            int width = description.Width;
            int height = description.Height;

            if (description.Mode == RenderFrameSizeMode.Relative)
            {
                // TODO GRAPHICS REFACTOR check if it's OK to use Presenter targets
                var referenceTexture = referenceFrame != null ? referenceFrame.ReferenceTexture : device.Presenter.BackBuffer;

                width = (width * referenceTexture.Width) / 100;
                height = (height * referenceTexture.Height) / 100;
            }

            var pixelFormat = PixelFormat.None;
            if (description.Format == RenderFrameFormat.LDR)
            {
                pixelFormat = device.ColorSpace == ColorSpace.Linear ? PixelFormat.R8G8B8A8_UNorm_SRgb : PixelFormat.R8G8B8A8_UNorm;
            }
            else if (description.Format == RenderFrameFormat.HDR)
            {
                pixelFormat = PixelFormat.R16G16B16A16_Float;
            }

            var depthFormat = PixelFormat.None;
            switch (description.DepthFormat)
            {
                case RenderFrameDepthFormat.Depth:
                    depthFormat = PixelFormat.D32_Float;
                    break;
                case RenderFrameDepthFormat.DepthAndStencil:
                    depthFormat = PixelFormat.D24_UNorm_S8_UInt;
                    break;
            }

            // Create the render target
            Texture renderTarget = null;
            if (pixelFormat != PixelFormat.None)
            {
                renderTarget = Texture.New2D(device, width, height, 1, pixelFormat, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            }

            // Create the depth stencil buffer
            Texture depthStencil = null;

            // TODO: Better handle the case where shared cannot be used. Should we throw an exception?
            if (description.DepthFormat == RenderFrameDepthFormat.Shared && referenceFrame != null && referenceFrame.DepthStencil != null &&
                referenceFrame.DepthStencil.Width == width && referenceFrame.DepthStencil.Height == height)
            {
                depthStencil = referenceFrame.DepthStencil;
            }
            else if (description.DepthFormat == RenderFrameDepthFormat.Depth || description.DepthFormat == RenderFrameDepthFormat.DepthAndStencil)
            {
                var depthStencilExtraFlag = device.Features.CurrentProfile >= GraphicsProfile.Level_10_0 ? TextureFlags.ShaderResource : TextureFlags.None;
                depthStencil = Texture.New2D(device, width, height, 1, depthFormat, TextureFlags.DepthStencil | depthStencilExtraFlag);
            }

            InitializeFrom(description, renderTarget != null ? new[] { renderTarget } : null, depthStencil, true);
        }
        
        // TODO: Should we move this to Graphics instead?
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderFrame" /> class.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="renderTargets">The render target.</param>
        /// <param name="depthStencil">The depth stencil.</param>
        /// <param name="ownsResources">if set to <c>true</c> this instance is owning the rendertargets and depth stencil buffer.</param>
        private void InitializeFrom(RenderFrameDescriptor descriptor, Texture[] renderTargets, Texture depthStencil, bool ownsResources)
        {
            Descriptor = descriptor;
            RenderTargets = renderTargets;
            DepthStencil = depthStencil;
            isOwner = ownsResources;
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

        internal class RenderFrameSerializer : DataSerializer<RenderFrame>
        {
            public override void Serialize(ref RenderFrame renderFrame, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Deserialize)
                {
                    var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                    var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();

                    var descriptor = stream.Read<RenderFrameDescriptor>();
                    renderFrame.InitializeFrom(graphicsDeviceService.GraphicsDevice, descriptor);
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
