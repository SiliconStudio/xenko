// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class CommandList : GraphicsResourceBase
    {
        private const int MaxRenderTargetCount = 8;

        private Viewport[] viewports = new Viewport[MaxRenderTargetCount];

        private Texture depthStencilBuffer;

        private Texture[] renderTargets = new Texture[MaxRenderTargetCount];

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

        public IReadOnlyList<Viewport> Viewports => viewports;

        public void Begin()
        {
            
        }

        public void End()
        {
            
        }

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
            SetDepthAndRenderTarget(deviceDepthStencilBuffer, deviceBackBuffer);
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a single render target to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilView">A view of the depth-stencil buffer to bind.</param>
        /// <param name="renderTargetView">A view of the render target to bind.</param>
        public void SetDepthAndRenderTarget(Texture depthStencilView, Texture renderTargetView)
        {
            depthStencilBuffer = depthStencilView;
            renderTargets[0] = renderTargetView;

            // Clear the other render targets bound
            for (int i = 1; i < renderTargets.Length; i++)
            {
                renderTargets[i] = null;
            }

            CommonSetDepthAndRenderTargets(depthStencilBuffer, renderTargets);
        }

        /// <summary>
        ///     Sets a new depthStencilBuffer to this GraphicsDevice. If there is any RenderTarget already bound, it will be unbinded. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth stencil.</param>
        public void SetDepthTarget(Texture depthStencilBuffer)
        {
            SetDepthAndRenderTarget(depthStencilBuffer, null);
        }

        /// <summary>
        /// Binds a single render target to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="renderTargetView">A view of the render target to bind.</param>
        public void SetRenderTarget(Texture renderTargetView)
        {
            SetDepthAndRenderTarget(null, renderTargetView);
        }

        /// <summary>
        ///     <p>Bind one or more render targets atomically and the depth-stencil buffer to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.</p>
        /// </summary>
        /// <param name="renderTargetViews">A set of render target views to bind.</param>
        public void SetRenderTargets(params Texture[] renderTargetViews)
        {
            SetDepthAndRenderTargets(null, renderTargetViews);
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
            viewports[index] = value;
        }

        /// <summary>
        /// Sets the viewport for the specified render target.
        /// </summary>
        /// <value>The viewport.</value>
        public void SetViewports(Viewport[] values)
        {
            for (int i = 0; i < viewports.Length; i++)
            {
                if (i >= values.Length)
                    break;

                viewports[i] = values[i];
            }
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a set of render targets to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilView">A view of the depth-stencil buffer to bind.</param>
        /// <param name="renderTargetViews">A set of render target views to bind.</param>
        /// <exception cref="System.ArgumentNullException">renderTargetViews</exception>
        public void SetDepthAndRenderTargets(Texture depthStencilView, params Texture[] renderTargetViews)
        {
            depthStencilBuffer = depthStencilView;

            if (renderTargetViews != null)
            {
                for (int i = 0; i < renderTargets.Length; i++)
                {
                    renderTargets[i] = i < renderTargetViews.Length ? renderTargetViews[i] : null;
                }
            }
            else
            {
                for (int i = 0; i < renderTargets.Length; i++)
                {
                    renderTargets[i] = null;
                }
            }

            CommonSetDepthAndRenderTargets(depthStencilBuffer, renderTargets);
        }

        private void CommonSetDepthAndRenderTargets(Texture depthStencilView, Texture[] renderTargetViews)
        {
            if (depthStencilView != null)
            {
                SetViewport(new Viewport(0, 0, depthStencilView.ViewWidth, depthStencilView.ViewHeight));
            }
            else
            {
                // Setup the viewport from the rendertarget view
                foreach (var rtv in renderTargetViews)
                {
                    if (rtv != null)
                    {
                        SetViewport(new Viewport(0, 0, rtv.ViewWidth, rtv.ViewHeight));
                        break;
                    }
                }
            }

            SetDepthAndRenderTargetsImpl(depthStencilView, renderTargetViews);
        }

        #region DrawQuad/DrawTexture Helpers
        /// <summary>
        /// Draws a full screen quad. An <see cref="Effect"/> must be applied before calling this method.
        /// </summary>
        public void DrawQuad()
        {
            GraphicsDevice.PrimitiveQuad.Draw(this);
        }

        /// <summary>
        /// Draws a fullscreen texture using a <see cref="SamplerStateFactory.LinearClamp"/> sampler. See <see cref="Draw+a+texture"/> to learn how to use it.
        /// </summary>
        /// <param name="texture">The texture. Expecting an instance of <see cref="Texture"/>.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void DrawTexture(Texture texture, bool applyEffectStates = false)
        {
            DrawTexture(texture, null, Color4.White, applyEffectStates);
        }

        /// <summary>
        /// Draws a fullscreen texture using the specified sampler. See <see cref="Draw+a+texture"/> to learn how to use it.
        /// </summary>
        /// <param name="texture">The texture. Expecting an instance of <see cref="Texture"/>.</param>
        /// <param name="sampler">The sampler.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void DrawTexture(Texture texture, SamplerState sampler, bool applyEffectStates = false)
        {
            DrawTexture(texture, sampler, Color4.White, applyEffectStates);
        }

        /// <summary>
        /// Draws a fullscreen texture using a <see cref="SamplerStateFactory.LinearClamp"/> sampler
        /// and the texture color multiplied by a custom color. See <see cref="Draw+a+texture"/> to learn how to use it.
        /// </summary>
        /// <param name="texture">The texture. Expecting an instance of <see cref="Texture"/>.</param>
        /// <param name="color">The color.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void DrawTexture(Texture texture, Color4 color, bool applyEffectStates = false)
        {
            DrawTexture(texture, null, color, applyEffectStates);
        }

        /// <summary>
        /// Draws a fullscreen texture using the specified sampler
        /// and the texture color multiplied by a custom color. See <see cref="Draw+a+texture"/> to learn how to use it.
        /// </summary>
        /// <param name="texture">The texture. Expecting an instance of <see cref="Texture"/>.</param>
        /// <param name="sampler">The sampler.</param>
        /// <param name="color">The color.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void DrawTexture(Texture texture, SamplerState sampler, Color4 color, bool applyEffectStates = false)
        {
            GraphicsDevice.PrimitiveQuad.Draw(this, texture, sampler, color, applyEffectStates);
        }
        #endregion
    }
}