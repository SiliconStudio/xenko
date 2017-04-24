// Copyright (c) 2011 Silicon Studio

using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Level10 render pass using a depth buffer and a render target.
    /// </summary>
    public class RenderTargetsPlugin : RenderPassPlugin, IRenderPassPluginTarget
    {
        private DelegateHolder<ThreadContext>.DelegateType startPassAction;
        private DelegateHolder<ThreadContext>.DelegateType endPassAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetsPlugin"/> class.
        /// </summary>
        public RenderTargetsPlugin() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetsPlugin"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public RenderTargetsPlugin(string name) : base(name)
        {
            ClearColor = Color.Black;
            ClearDepth = 1.0f;
            ClearStencil = 0;
            EnableClearTarget = true;
            EnableClearDepth = true;
            EnableSetTargets = true;
        }

        /// <summary>
        /// Gets or sets the render target.
        /// </summary>
        /// <value>
        /// The render target.
        /// </value>
        /// <remarks>
        /// This value is a shared parameters with the key <see cref="RenderTargetKeys.RenderTarget"/>
        /// </remarks>
        public virtual RenderTarget RenderTarget { get; set; }

        /// <summary>
        /// Gets or sets the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The depth stencil buffer.
        /// </value>
        /// <remarks>
        /// This value is a shared parameters with the key <see cref="RenderTargetKeys.DepthStencil"/>
        /// </remarks>
        public virtual DepthStencilBuffer DepthStencil { get; set; }


        /// <summary>
        /// Gets or sets the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The depth stencil buffer.
        /// </value>
        /// <remarks>
        /// This value is a shared parameters with the key <see cref="RenderTargetKeys.DepthStencil"/>
        /// </remarks>
        public Texture2D DepthStencilTexture { get; set; }

        /// <summary>
        /// Gets or sets the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The depth stencil buffer.
        /// </value>
        /// <remarks>
        /// This value is a shared parameters with the key <see cref="RenderTargetKeys.DepthStencil"/>
        /// </remarks>
        public DepthStencilBuffer DepthStencilReadOnly { get; set; }

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
        /// Gets or sets the viewport.
        /// </summary>
        /// <value>
        /// The viewport.
        /// </value>
        public Viewport Viewport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable clear render target].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable clear render target]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableClearTarget { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable clear depth stencil].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable clear depth stencil]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableClearDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable set targets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable set targets]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSetTargets { get; set; }

        /// <summary>
        /// Gets or sets the depth stencil state.
        /// </summary>
        public DepthStencilState DepthStencilState
        {
            get { return Parameters.TryGet(EffectPlugin.DepthStencilStateKey); }
            set { Parameters.Set(EffectPlugin.DepthStencilStateKey, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use depth stencil read only].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use depth stencil read only]; otherwise, <c>false</c>.
        /// </value>
        public bool UseDepthStencilReadOnly { get; set; }

        public override void Load()
        {
            base.Load();

            if (OfflineCompilation)
                return;

            if (!Parameters.ContainsKey(EffectPlugin.DepthStencilStateKey))
                DepthStencilState = GraphicsDevice.DepthStencilStates.Default;

            if (EnableSetTargets || EnableClearTarget || EnableClearDepth)
            {
                // Main pre-pass: clear render target & depth buffer, and bind them
                // TODO: Separate prepass from per-rendercontext init
                startPassAction = (threadContext) =>
                    {
                        if (threadContext.FirstContext)
                        {
                            if (EnableClearDepth && DepthStencil != null && !UseDepthStencilReadOnly)
                                threadContext.GraphicsDevice.Clear(DepthStencil, DepthStencilClearOptions.DepthBuffer, ClearDepth, ClearStencil);
                            if (EnableClearTarget && RenderTarget != null)
                                threadContext.GraphicsDevice.Clear(RenderTarget, ClearColor);
                        }

                        if (EnableSetTargets)
                        {
                            // If the Viewport is undefined, use the render target dimension
                            var viewPort = Viewport;
                            if (viewPort == Viewport.Empty)
                            {
                                var desc = RenderTarget != null ? RenderTarget.Description : threadContext.GraphicsDevice.BackBuffer.Description;
                                viewPort = new Viewport(0, 0, desc.Width, desc.Height);
                                Viewport = viewPort;
                            }

                            var depthStencil = UseDepthStencilReadOnly ? DepthStencilReadOnly : DepthStencil;

                            threadContext.GraphicsDevice.SetViewport(viewPort);
                            threadContext.GraphicsDevice.SetRenderTargets(depthStencil, RenderTarget);
                        }
                    };
                RenderPass.StartPass += startPassAction;
            }

            // Unbind depth stencil buffer and render targets
            if (EnableSetTargets)
            {
                endPassAction = (threadContext) => threadContext.GraphicsDevice.UnsetRenderTargets();
                RenderPass.EndPass += endPassAction;
            }
        }

        public override void Unload()
        {
            base.Unload();

            if (OfflineCompilation)
                return;

            RenderPass.StartPass -= startPassAction;
            startPassAction = null;

            RenderPass.EndPass -= endPassAction;
            endPassAction = null;
        }
    }
}
