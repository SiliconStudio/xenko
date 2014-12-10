// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Renderers;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// The renderer in charge of drawing the UI.
    /// </summary>
    public class UIRenderer : Renderer, IRendererManager
    {
        private readonly IGame game;
        private readonly UISystem uiSystem;

        private readonly UIBatch batch;

        private UIRenderingContext renderingContext;

        private readonly RendererManager rendererManager;

        private bool uiResolutionChanged;
        
        public UIRenderer(IServiceRegistry services)
            : base(services)
        {
            game = (IGame)services.GetService(typeof(IGame));
            uiSystem = (UISystem)services.GetService(typeof(UISystem));

            batch = uiSystem.Batch;

            rendererManager = new RendererManager(new DefaultRenderersFactory(services));

            DebugName = "UIRenderer";
        }

        public override void Load()
        {
            base.Load();

            uiSystem.ResolutionChanged += UISystemOnResolutionChanged;

            renderingContext = new UIRenderingContext
            {
                DepthStencilBuffer = GraphicsDevice.DepthStencilBuffer,
                RenderTarget = GraphicsDevice.BackBuffer,
            };
        }

        private void UISystemOnResolutionChanged(object sender, EventArgs eventArgs)
        {
            uiResolutionChanged = true;
        }

        public override void Unload()
        {
            base.Unload();

            if (uiSystem != null)
                uiSystem.ResolutionChanged -= UISystemOnResolutionChanged;
        }

        protected override void OnRendering(RenderContext context)
        {
            if (uiSystem.RootElement == null)
                return;

            var drawTime = game.DrawTime;
            var rootElement = uiSystem.RootElement;
            var virtualResolution = uiSystem.VirtualResolution;
            var updatableRootElement = (IUIElementUpdate)rootElement;

            // perform the time-based updates of the UI element
            updatableRootElement.Update(drawTime);

            // update the UI element disposition
            rootElement.Measure(virtualResolution);
            rootElement.Arrange(virtualResolution, false);

            // update the UI element hierarchical properties
            updatableRootElement.UpdateWorldMatrix(ref uiSystem.WorldMatrix, uiResolutionChanged);
            updatableRootElement.UpdateElementState(0);
            uiResolutionChanged = false;

            // set render targets and reset Depth buffer
            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);

            // update the context time
            renderingContext.Time = game.DrawTime;

            // start the image draw session
            renderingContext.StencilTestReferenceValue = 0;
            batch.Begin(ref uiSystem.ViewProjectionInternal, GraphicsDevice.BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);

            // Render the UI elements in the final render target
            ReccursiveDrawWithClipping(rootElement);

            // end the image draw session
            batch.End();
        }

        private void ReccursiveDrawWithClipping(UIElement element)
        {
            // if the element is not visible, we also remove all its children
            if (!element.IsVisible)
                return;

            var renderer = rendererManager.GetRenderer(element);
            renderingContext.DepthBias = element.DepthBias;

            // render the clipping region of the element
            if (element.ClipToBounds)
            {
                // flush current elements
                batch.End();
                
                // render the clipping region
                batch.Begin(ref uiSystem.ViewProjectionInternal, GraphicsDevice.BlendStates.ColorDisabled, uiSystem.IncreaseStencilValueState, renderingContext.StencilTestReferenceValue);
                renderer.RenderClipping(element, renderingContext);
                batch.End();

                // update context and restart the batch
                renderingContext.StencilTestReferenceValue += 1;
                batch.Begin(ref uiSystem.ViewProjectionInternal, GraphicsDevice.BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);
            }

            // render the design of the element
            renderer.RenderColor(element, renderingContext);

            // render the children
            foreach (var child in element.VisualChildrenCollection)
                ReccursiveDrawWithClipping(child);

            // clear the element clipping region from the stencil buffer
            if (element.ClipToBounds)
            {
                // flush current elements
                batch.End();

                renderingContext.DepthBias = element.MaxChildrenDepthBias;

                // render the clipping region
                batch.Begin(ref uiSystem.ViewProjectionInternal, GraphicsDevice.BlendStates.ColorDisabled, uiSystem.DecreaseStencilValueState, renderingContext.StencilTestReferenceValue);
                renderer.RenderClipping(element, renderingContext);
                batch.End();

                // update context and restart the batch
                renderingContext.StencilTestReferenceValue -= 1;
                batch.Begin(ref uiSystem.ViewProjectionInternal, GraphicsDevice.BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);
            }
        }

        public ElementRenderer GetRenderer(UIElement element)
        {
            return rendererManager.GetRenderer(element);
        }

        public void RegisterRendererFactory(Type uiElementType, IElementRendererFactory factory)
        {
            rendererManager.RegisterRendererFactory(uiElementType, factory);
        }

        public void RegisterRenderer(UIElement element, ElementRenderer renderer)
        {
            rendererManager.RegisterRenderer(element, renderer);
        }
    }
}