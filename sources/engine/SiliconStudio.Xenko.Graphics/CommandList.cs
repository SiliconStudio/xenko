// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Performs resource bindings and primitive-based rendering. See <see cref="The+GraphicsDevice+class"/> to learn more about the class.
    /// </summary>
    public partial class CommandList : GraphicsResourceBase
    {
        private const int MaxRenderTargetCount = 8;
        private bool viewportDirty = false;

        private Viewport[] viewports = new Viewport[MaxRenderTargetCount];

        private Texture depthStencilBuffer;

        private Texture[] renderTargets = new Texture[MaxRenderTargetCount];
        private int renderTargetCount;

        /// <summary>
        ///     Gets the first viewport.
        /// </summary>
        /// <value>The first viewport.</value>
        public Viewport Viewport => viewports[0];

        /// <summary>
        ///     Gets the depth stencil buffer currently sets on this instance.
        /// </summary>
        /// <value>
        ///     The depth stencil buffer currently sets on this instance.
        /// </value>
        public Texture DepthStencilBuffer => depthStencilBuffer;

        /// <summary>
        ///     Gets the render target buffer currently sets on this instance.
        /// </summary>
        /// <value>
        ///     The render target buffer currently sets on this instance.
        /// </value>
        public Texture RenderTarget => renderTargets[0];

        public IReadOnlyList<Texture> RenderTargets => renderTargets;

        public int RenderTargetCount => renderTargetCount;

        public IReadOnlyList<Viewport> Viewports => viewports;

        /// <summary>
        /// Clears the state and restore the state of the device.
        /// </summary>
        public void ClearState()
        {
            ClearStateImpl();

            // Setup empty viewports
            for (int i = 0; i < viewports.Length; i++)
                viewports[i] = new Viewport();

            // Setup the default render target
            var deviceDepthStencilBuffer = GraphicsDevice.Presenter?.DepthStencilBuffer;
            var deviceBackBuffer = GraphicsDevice.Presenter?.BackBuffer;
            SetRenderTargetAndViewport(deviceDepthStencilBuffer, deviceBackBuffer);
        }

        /// <summary>
        /// Unbinds all depth-stencil buffer and render targets from the output-merger stage.
        /// </summary>
        public void ResetTargets()
        {
            ResetTargetsImpl();

            depthStencilBuffer = null;
            for (int i = 0; i < renderTargets.Length; i++)
                renderTargets[i] = null;
        }

        /// <summary>
        /// Sets the viewport for the first render target.
        /// </summary>
        /// <value>The viewport.</value>
        public void SetViewport(Viewport value)
        {
            SetViewport(0, value);
        }

        /// <summary>
        /// Sets the viewport for the specified render target.
        /// </summary>
        /// <value>The viewport.</value>
        public void SetViewport(int index, Viewport value)
        {
            if (viewports[index] != value)
            {
                viewportDirty = true;
                viewports[index] = value;
            }
        }

        /// <summary>
        /// Sets the viewport for the specified render target.
        /// </summary>
        /// <value>The viewport.</value>
        public void SetViewports(Viewport[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (viewports[i] != values[i])
                {
                    viewportDirty = true;
                    viewports[i] = values[i];
                }
            }
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a single render target to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilView">A view of the depth-stencil buffer to bind.</param>
        /// <param name="renderTargetView">A view of the render target to bind.</param>
        public void SetRenderTargetAndViewport(Texture depthStencilView, Texture renderTargetView)
        {
            depthStencilBuffer = depthStencilView;
            renderTargets[0] = renderTargetView;
            renderTargetCount = renderTargetView != null ? 1 : 0;

            CommonSetRenderTargetsAndViewport(depthStencilBuffer, renderTargetCount, renderTargets);
        }

        /// <summary>
        ///     <p>Bind one or more render targets atomically and the depth-stencil buffer to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.</p>
        /// </summary>
        /// <param name="renderTargetViews">A set of render target views to bind.</param>
        public void SetRenderTargetsAndViewport(Texture[] renderTargetViews)
        {
            SetRenderTargetsAndViewport(null, renderTargetViews);
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a set of render targets to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilView">A view of the depth-stencil buffer to bind.</param>
        /// <param name="renderTargetViews">A set of render target views to bind.</param>
        /// <exception cref="System.ArgumentNullException">renderTargetViews</exception>
        public void SetRenderTargetsAndViewport(Texture depthStencilView, Texture[] renderTargetViews)
        {
            depthStencilBuffer = depthStencilView;

            if (renderTargetViews != null)
            {
                renderTargetCount = renderTargetViews.Length;
                for (int i = 0; i < renderTargetViews.Length; i++)
                {
                    renderTargets[i] = renderTargetViews[i];
                }
            }
            else
            {
                renderTargetCount = 0;
            }

            CommonSetRenderTargetsAndViewport(depthStencilBuffer, renderTargetCount, renderTargets);
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a single render target to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilView">A view of the depth-stencil buffer to bind.</param>
        /// <param name="renderTargetView">A view of the render target to bind.</param>
        public void SetRenderTarget(Texture depthStencilView, Texture renderTargetView)
        {
            depthStencilBuffer = depthStencilView;
            renderTargets[0] = renderTargetView;
            renderTargetCount = renderTargetView != null ? 1 : 0;

            SetRenderTargetsImpl(depthStencilBuffer, renderTargetCount, renderTargets);
        }

        /// <summary>
        ///     <p>Bind one or more render targets atomically and the depth-stencil buffer to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.</p>
        /// </summary>
        /// <param name="renderTargetViews">A set of render target views to bind.</param>
        public void SetRenderTargets(Texture[] renderTargetViews)
        {
            SetRenderTargets(null, renderTargetViews);
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a set of render targets to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilView">A view of the depth-stencil buffer to bind.</param>
        /// <param name="renderTargetViews">A set of render target views to bind.</param>
        /// <exception cref="System.ArgumentNullException">renderTargetViews</exception>
        public void SetRenderTargets(Texture depthStencilView, Texture[] renderTargetViews)
        {
            depthStencilBuffer = depthStencilView;

            if (renderTargetViews != null)
            {
                renderTargetCount = renderTargetViews.Length;
                for (var i = 0; i < renderTargetViews.Length; i++)
                {
                    renderTargets[i] = renderTargetViews[i];
                }
            }
            else
            {
                renderTargetCount = 0;
            }

            SetRenderTargetsImpl(depthStencilBuffer, renderTargetCount, renderTargets);
        }

        private void CommonSetRenderTargetsAndViewport(Texture depthStencilView, int currentRenderTargetCount, Texture[] renderTargetViews)
        {
            if (depthStencilView != null)
            {
                SetViewport(new Viewport(0, 0, depthStencilView.ViewWidth, depthStencilView.ViewHeight));
            }
            else if (currentRenderTargetCount > 0)
            {
                // Setup the viewport from the rendertarget view
                var rtv = renderTargetViews[0];
                SetViewport(new Viewport(0, 0, rtv.ViewWidth, rtv.ViewHeight));
            }

            SetRenderTargetsImpl(depthStencilView, currentRenderTargetCount, renderTargetViews);
        }
    }
}
