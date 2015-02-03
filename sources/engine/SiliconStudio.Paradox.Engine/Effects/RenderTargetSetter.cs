// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// A processor that setup a <see cref="RenderTarget"/> and a <see cref="DepthStencil"/> on a <see cref="RenderPass"/>.
    /// </summary>
    public class RenderTargetSetter : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Renderer" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public RenderTargetSetter(IServiceRegistry services)
            : base(services)
        {
            ClearColor = Color.Black;
            ClearDepth = 1.0f;
            ClearStencil = 0;
            EnableClearTarget = true;
            EnableClearDepth = true;
            EnableClearStencil = false;
            EnableSetTargets = true;

            RenderTarget = GraphicsDevice.BackBuffer;
            DepthStencil = GraphicsDevice.DepthStencilBuffer;
        }

        /// <summary>
        /// Gets or sets the key to get the render target from <see cref="RenderPipeline.Parameters"/>.
        /// </summary>
        /// <value>
        /// The render target key.
        /// </value>
        public Texture RenderTarget { get; set; }

        /// <summary>
        /// Gets or sets the key to get the depth stencil from <see cref="RenderPipeline.Parameters"/>.
        /// </summary>
        /// <value>
        /// The depth stencil key.
        /// </value>
        public Texture DepthStencil { get; set; }

        /// <summary>
        /// Gets or sets the viewport.
        /// </summary>
        /// <value>
        /// The viewport.
        /// </value>
        public Viewport Viewport { get; set; }

        /// <summary>
        /// Gets or sets the color used to clear the render target.
        /// </summary>
        /// <value>
        /// The the color used to clear the render target.
        /// </value>
        public Color ClearColor { get; set; }

        /// <summary>
        /// Gets or sets the depth value used to clear the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The depth value used to clear the depth stencil buffer.
        /// </value>
        public float ClearDepth { get; set; }

        /// <summary>
        /// Gets or sets the stencil value used to clear the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The stencil value used to clear the depth stencil buffer.
        /// </value>
        public byte ClearStencil { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable clear render target].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable clear render target]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableClearTarget { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable clear depth].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable clear depth ; otherwise, <c>false</c>.
        /// </value>
        public bool EnableClearDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable clear stencil].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable clear stencil]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableClearStencil { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable set targets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable set targets]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSetTargets { get; set; }

        protected override void OnRendering(RenderContext context)
        {
            var graphicsDevice = context.GraphicsDevice;

            // clear the targets
            if ((EnableClearDepth || EnableClearStencil) && DepthStencil != null)
            {
                var clearOptions = (DepthStencilClearOptions)0;
                if (EnableClearDepth)
                    clearOptions |= DepthStencilClearOptions.DepthBuffer;
                if (EnableClearStencil)
                    clearOptions |= DepthStencilClearOptions.Stencil;

                graphicsDevice.Clear(DepthStencil, clearOptions, ClearDepth, ClearStencil);
            }
            if (EnableClearTarget && RenderTarget != null)
                graphicsDevice.Clear(RenderTarget, ClearColor);

            // set the view size parameter
            var pass = context.CurrentPass;
            var viewParameters = pass.Parameters;
            viewParameters.Set(CameraKeys.ViewSize, new Vector2(Viewport.Width, Viewport.Height));

            // set the targets
            if (EnableSetTargets)
            {
                graphicsDevice.SetDepthAndRenderTarget(DepthStencil, RenderTarget);
                var viewPort = Viewport;
                if (viewPort != Viewport.Empty)
                    graphicsDevice.SetViewport(viewPort);
            }
        }
    }
}