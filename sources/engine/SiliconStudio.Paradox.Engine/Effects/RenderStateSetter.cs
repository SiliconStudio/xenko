// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Sets given render state during rendering.
    /// </summary>
    public class RenderStateSetter : Renderer
    {
        public RenderStateSetter(IServiceRegistry services) : base(services)
        {
        }

        /// <summary>
        /// Gets or sets the depth stencil state.
        /// </summary>
        /// <value>
        /// The depth stencil state.
        /// </value>
        public DepthStencilState DepthStencilState { get; set; }

        /// <summary>
        /// Gets or sets the blend state.
        /// </summary>
        /// <value>
        /// The blend state.
        /// </value>
        public BlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets the rasterizer state.
        /// </summary>
        /// <value>
        /// The rasterizer state.
        /// </value>
        public RasterizerState RasterizerState { get; set; }

        /// <inheritdoc/>
        public override void Load()
        {
            base.Load();
            Pass.EndPass += EndPass;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            base.Unload();
            Pass.EndPass -= EndPass;
        }

        protected override void OnRendering(RenderContext context)
        {
            var graphicsParameters = context.GraphicsDevice.Parameters;

            // Set states
            if (DepthStencilState != null)
                graphicsParameters.Set(Effect.DepthStencilStateKey, DepthStencilState);

            if (BlendState != null)
                graphicsParameters.Set(Effect.BlendStateKey, BlendState);

            if (RasterizerState != null)
                graphicsParameters.Set(Effect.RasterizerStateKey, RasterizerState);
        }

        private void EndPass(RenderContext context)
        {
            var graphicsParameters = context.GraphicsDevice.Parameters;

            // Reset states
            if (DepthStencilState != null)
                graphicsParameters.Reset(Effect.DepthStencilStateKey);

            if (BlendState != null)
                graphicsParameters.Reset(Effect.BlendStateKey);

            if (RasterizerState != null)
                graphicsParameters.Reset(Effect.RasterizerStateKey);
        }
    }
}