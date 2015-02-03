// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    public class MultipleRenderTargetsSetter : RenderTargetSetter
    {
        public MultipleRenderTargetsSetter(IServiceRegistry services)
            : base(services)
        {
            RenderTargets = null;
            ClearColors = null;
        }

        /// <summary>
        /// Gets or sets the key to get the render targets from <see cref="RenderPipeline.Parameters"/>.
        /// </summary>
        /// <value>
        /// The render target key.
        /// </value>
        public Texture[] RenderTargets { get; set; }

        /// <summary>
        /// Gets or sets the color used to clear the render target.
        /// </summary>
        /// <value>
        /// The colosr used to clear the render targets.
        /// </value>
        public Color[] ClearColors { get; set; }

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
            if (EnableClearTarget && RenderTargets != null)
            {
                for (var i = 0; i < RenderTargets.Length; ++i)
                {
                    if (RenderTargets[i] != null)
                    {
                        if (ClearColors != null && i < ClearColors.Length)
                            graphicsDevice.Clear(RenderTargets[i], ClearColors[i]);
                        else
                            graphicsDevice.Clear(RenderTargets[i], ClearColor);
                    }
                }
            }

            // set the view size parameter
            var pass = context.CurrentPass;
            var viewParameters = pass.Parameters;
            viewParameters.Set(CameraKeys.ViewSize, new Vector2(Viewport.Width, Viewport.Height));

            // set the targets
            if (EnableSetTargets)
            {
                if (RenderTargets != null)
                {
                    graphicsDevice.SetDepthAndRenderTargets(DepthStencil, RenderTargets);
                }
                
                var viewPort = Viewport;
                if (viewPort != Viewport.Empty)
                    graphicsDevice.SetViewport(viewPort);
            }
        }
    }
}