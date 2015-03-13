// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Renderers;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// The renderer in charge of drawing the UI.
    /// </summary>
    public class UIComponentRenderer : EntityComponentRendererBase, IRendererManager
    {
        private IGame game;
        private UISystem uiSystem;
        private InputManager input;

        private RendererManager rendererManager;

        private readonly UIRenderingContext renderingContext = new UIRenderingContext();

        private UIBatch batch;

        private Vector3 virtualResolution;
        private Vector2 worldTranslation;

        private readonly LayoutingContext layoutingContext = new LayoutingContext();

        private Vector2 lastMousePosition;

        // object to avoid allocation at each element leave event
        private readonly HashSet<UIElement> newlySelectedElementParents = new HashSet<UIElement>();

        private readonly List<PointerEvent> compactedPointerEvents = new List<PointerEvent>();

        private readonly List<UIComponentProcessor.UIComponentState> uiElementStates = new List<UIComponentProcessor.UIComponentState>();

        private readonly CameraState cameraState = new CameraState();

        protected override void InitializeCore()
        {
            base.InitializeCore();

            Name = "UIComponentRenderer";
            game = (IGame)Services.GetService(typeof(IGame));
            input = (InputManager)Services.GetService(typeof(InputManager));
            uiSystem = (UISystem)Services.GetService(typeof(UISystem));

            rendererManager = new RendererManager(new DefaultRenderersFactory(Services));

            batch = uiSystem.Batch;
        }

        protected override void Destroy()
        {
            base.Destroy();

            rendererManager.Dispose();
        }

        /// <summary>
        /// Gets or sets the virtual resolution of the screen in Pixels.
        /// </summary>
        /// <value>The virtual resolution.</value>
        /// <exception cref="System.InvalidOperationException">
        /// The resolution along the X axis is not valid. [Value= + value.X + ]
        /// or
        /// The resolution along the Y axis is not valid. [Value= + value.Y + ]
        /// or
        /// The resolution along the Z axis is not valid. [Value= + value.Z + ]
        /// </exception>
        internal Vector3 VirtualResolution
        {
            get
            {
                return virtualResolution;
            }
            set
            {
                if (value.X <= 0)
                    throw new InvalidOperationException("The resolution along the X axis is not valid. [Value=" + value.X + "]");
                if (value.Y <= 0)
                    throw new InvalidOperationException("The resolution along the Y axis is not valid. [Value=" + value.Y + "]");
                if (value.Z <= 0)
                    throw new InvalidOperationException("The resolution along the Z axis is not valid. [Value=" + value.Z + "]");

                if (virtualResolution == value)
                    return;

                virtualResolution = value;
                worldTranslation = new Vector2(-VirtualResolution.X / 2, -VirtualResolution.Y / 2);
            }
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            var uiProcessor = SceneInstance.GetProcessor<UIComponentProcessor>();
            if (uiProcessor == null)
                return;

            foreach (var uiRoot in uiProcessor.UIRoots)
            {
                // Perform culling on group and accept
                if ((uiRoot.UIComponent.Entity.Group & CurrentCullingMask) == 0)
                    continue;

                // Project the position
                // TODO: This code is duplicated from SpriteComponent -> unify it at higher level?
                var worldPosition = new Vector4(uiRoot.TransformComponent.WorldMatrix.TranslationVector, 1.0f);

                Vector4 projectedPosition;
                Vector4.Transform(ref worldPosition, ref context.ViewProjectionMatrix, out projectedPosition);
                var projectedZ = projectedPosition.Z / projectedPosition.W;

                transparentList.Add(new RenderItem(this, uiRoot, projectedZ));
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            // build the list of the UI elements to render
            uiElementStates.Clear();
            foreach (var renderItem in renderItems)
                uiElementStates.Add((UIComponentProcessor.UIComponentState)renderItem.DrawContext);

            // Adapt the projection matrix to the UI coordinate system (Y axis inversed)
            context.ProjectionMatrix.M22 = -context.ProjectionMatrix.M22;
            context.ViewProjectionMatrix.Column2 = -context.ViewProjectionMatrix.Column2;

            // update the needed camera parameters
            var cameraComponentState = context.Tags.Get(CameraComponentRenderer.Current);
            cameraState.Update(cameraComponentState);
            
            // Analyze the input and trigger the UI element touch and key events
            // Note: this is done before measuring/arranging/drawing the element in order to avoid one frame latency on clicks.
            //       But by doing so the world matrices taken for hit test are the ones calculated during last frame.
            CompactPointerEvents();
            using (Profiler.Begin(UIProfilerKeys.TouchEventsUpdate))
            {
                foreach (var uiState in uiElementStates)
                {
                    if (uiState.UIComponent.RootElement == null)
                        continue;

                    UpdateMouseOver(context, uiState);
                    UpdateTouchEvents(context, uiState, game.DrawTime);
                }
            }
            ClearPointerEvents();

            // update the rendering context
            renderingContext.Time = game.DrawTime;
            renderingContext.Resolution = virtualResolution;
            renderingContext.ViewMatrix = context.ViewMatrix;
            renderingContext.ProjectionMatrix = context.ProjectionMatrix;
            renderingContext.ViewProjectionMatrix = context.ViewProjectionMatrix;
            renderingContext.RenderTarget = CurrentRenderFrame.RenderTarget;
            renderingContext.DepthStencilBuffer = CurrentRenderFrame.DepthStencil;
            renderingContext.ShouldSnapText = (context.Tags.Get(SceneEntityRenderer.Current) as SceneUIRenderer) != null; // snaps only if rendered from the SceneUIRenderer

            // cache the ratio between viewport and target.
            var viewportSize = context.GraphicsDevice.Viewport.Size;
            var viewportTargetRatio = new Vector2(viewportSize.X / CurrentRenderFrame.RenderTarget.Width, viewportSize.Y / CurrentRenderFrame.RenderTarget.Height);

            // render the UI elements of all the entities
            foreach (var uiElementState in uiElementStates)
            {
                var rootElement = uiElementState.UIComponent.RootElement;
                if (rootElement == null)
                    return;

                var drawTime = game.DrawTime;
                var updatableRootElement = (IUIElementUpdate)rootElement;

                // build the world matrix of the UI
                var worldMatrix = uiElementState.TransformComponent.WorldMatrix;
                worldMatrix.M41 += worldTranslation.X * worldMatrix.M11 + worldTranslation.Y * worldMatrix.M21;
                worldMatrix.M42 += worldTranslation.X * worldMatrix.M12 + worldTranslation.Y * worldMatrix.M22;
                worldMatrix.M43 += worldTranslation.X * worldMatrix.M13 + worldTranslation.Y * worldMatrix.M23;

                // calculate an estimate of the UI real size by projecting the element virtual resolution on the screen
                var projectedVirtualWidth = virtualResolution.X * new Vector3(worldMatrix.M11, worldMatrix.M12, worldMatrix.M13);
                var projectedVirtualHeight = virtualResolution.Y * new Vector3(worldMatrix.M21, worldMatrix.M22, worldMatrix.M23);
                Vector3.TransformNormal(ref projectedVirtualWidth, ref context.ViewMatrix, out projectedVirtualWidth);
                Vector3.TransformNormal(ref projectedVirtualHeight, ref context.ViewMatrix, out projectedVirtualHeight);
                var projectedVirtualWidthLength = (viewportTargetRatio * (Vector2)projectedVirtualWidth).Length();
                var projectedVirtualHeightLength = (viewportTargetRatio * (Vector2)projectedVirtualHeight).Length();

                // update layouting context.
                layoutingContext.VirtualResolution = virtualResolution;
                layoutingContext.RealResolution = viewportSize;
                layoutingContext.RealVirtualResolutionRatio = new Vector2(viewportSize.X / projectedVirtualWidthLength, viewportSize.Y / projectedVirtualHeightLength);
                rootElement.LayoutingContext = layoutingContext;

                // perform the time-based updates of the UI element
                updatableRootElement.Update(drawTime);

                // update the UI element disposition
                rootElement.Measure(virtualResolution);
                rootElement.Arrange(virtualResolution, false);

                // update the UI element hierarchical properties
                updatableRootElement.UpdateWorldMatrix(ref worldMatrix, worldMatrix == uiElementState.LastWorldMatrix);
                updatableRootElement.UpdateElementState(0);
                uiElementState.LastWorldMatrix = worldMatrix;

                // clear the Depth buffer
                context.GraphicsDevice.Clear(CurrentRenderFrame.DepthStencil, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);

                // start the image draw session
                renderingContext.StencilTestReferenceValue = 0;
                batch.Begin(ref context.ViewProjectionMatrix, context.GraphicsDevice.BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);

                // Render the UI elements in the final render target
                ReccursiveDrawWithClipping(context, rootElement);

                // end the image draw session
                batch.End();
            }

            // revert inversion of the Y axis in the projection matrix (for next rendering)
            context.ProjectionMatrix.M22 = -context.ProjectionMatrix.M22;
            context.ViewProjectionMatrix.Column2 = -context.ViewProjectionMatrix.Column2;
        }

        private void ReccursiveDrawWithClipping(RenderContext context, UIElement element)
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
                batch.Begin(ref context.ViewProjectionMatrix, context.GraphicsDevice.BlendStates.ColorDisabled, uiSystem.IncreaseStencilValueState, renderingContext.StencilTestReferenceValue);
                renderer.RenderClipping(element, renderingContext);
                batch.End();

                // update context and restart the batch
                renderingContext.StencilTestReferenceValue += 1;
                batch.Begin(ref context.ViewProjectionMatrix, context.GraphicsDevice.BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);
            }

            // render the design of the element
            renderer.RenderColor(element, renderingContext);

            // render the children
            foreach (var child in element.VisualChildrenCollection)
                ReccursiveDrawWithClipping(context, child);

            // clear the element clipping region from the stencil buffer
            if (element.ClipToBounds)
            {
                // flush current elements
                batch.End();

                renderingContext.DepthBias = element.MaxChildrenDepthBias;

                // render the clipping region
                batch.Begin(ref context.ViewProjectionMatrix, context.GraphicsDevice.BlendStates.ColorDisabled, uiSystem.DecreaseStencilValueState, renderingContext.StencilTestReferenceValue);
                renderer.RenderClipping(element, renderingContext);
                batch.End();

                // update context and restart the batch
                renderingContext.StencilTestReferenceValue -= 1;
                batch.Begin(ref context.ViewProjectionMatrix, context.GraphicsDevice.BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);
            }
        }

        private void CompactPointerEvents()
        {
            // compact all the move events of the frame together
            var aggregatedTranslation = Vector2.Zero;
            for (var index = 0; index < input.PointerEvents.Count; ++index)
            {
                var pointerEvent = input.PointerEvents[index];

                if (pointerEvent.State != PointerState.Move)
                {
                    aggregatedTranslation = Vector2.Zero;
                    compactedPointerEvents.Add(pointerEvent.Clone());
                    continue;
                }

                aggregatedTranslation += pointerEvent.DeltaPosition;

                if (index + 1 >= input.PointerEvents.Count || input.PointerEvents[index + 1].State != PointerState.Move)
                {
                    var compactedMoveEvent = pointerEvent.Clone();
                    compactedMoveEvent.DeltaPosition = aggregatedTranslation;
                    compactedPointerEvents.Add(compactedMoveEvent);
                }
            }
        }

        private void ClearPointerEvents()
        {
            // collect back pointer event not used anymore
            lock (PointerEvent.Pool)
            {
                foreach (var pointerEvent in compactedPointerEvents)
                    PointerEvent.Pool.Enqueue(pointerEvent);
            }
            compactedPointerEvents.Clear();
        }

        private void UpdateTouchEvents(RenderContext context, UIComponentProcessor.UIComponentState state, GameTime gameTime)
        {
            var rootElement = state.UIComponent.RootElement;
            var intersectionPoint = Vector3.Zero;
            var lastTouchPosition = new Vector2(float.NegativeInfinity);

            // analyze pointer event input and trigger UI touch events depending on hit Tests
            foreach (var pointerEvent in compactedPointerEvents)
            {
                // performance optimization: skip all the events that started outside of the UI
                var lastTouchedElement = state.LastTouchedElement;
                if (lastTouchedElement == null && pointerEvent.State != PointerState.Down)
                    continue;

                var time = gameTime.Total;

                var currentTouchPosition = pointerEvent.Position;
                var currentTouchedElement = lastTouchedElement;

                // re-calculate the element under cursor if click position changed.
                if (lastTouchPosition != currentTouchPosition)
                    currentTouchedElement = GetElementAtScreenPosition(context, rootElement, pointerEvent.Position, ref intersectionPoint);

                if (pointerEvent.State == PointerState.Down || pointerEvent.State == PointerState.Up)
                    state.LastIntersectionPoint = intersectionPoint;

                var touchEvent = new TouchEventArgs
                {
                    Action = TouchAction.Down,
                    Timestamp = time,
                    ScreenPosition = currentTouchPosition,
                    ScreenTranslation = pointerEvent.DeltaPosition,
                    WorldPosition = intersectionPoint,
                    WorldTranslation = intersectionPoint - state.LastIntersectionPoint
                };

                switch (pointerEvent.State)
                {
                    case PointerState.Down:
                        touchEvent.Action = TouchAction.Down;
                        if (currentTouchedElement != null)
                            currentTouchedElement.RaiseTouchDownEvent(touchEvent);
                        break;

                    case PointerState.Up:
                        touchEvent.Action = TouchAction.Up;

                        // generate enter/leave events if we passed from an element to another without move events
                        if (currentTouchedElement != lastTouchedElement)
                            ThrowEnterAndLeaveTouchEvents(currentTouchedElement, lastTouchedElement, touchEvent);

                        // trigger the up event
                        if (currentTouchedElement != null)
                            currentTouchedElement.RaiseTouchUpEvent(touchEvent);
                        break;

                    case PointerState.Move:
                        touchEvent.Action = TouchAction.Move;

                        // first notify the move event (even if the touched element changed in between it is still coherent in one of its parents)
                        if (currentTouchedElement != null)
                            currentTouchedElement.RaiseTouchMoveEvent(touchEvent);

                        // then generate enter/leave events if we passed from an element to another
                        if (currentTouchedElement != lastTouchedElement)
                            ThrowEnterAndLeaveTouchEvents(currentTouchedElement, lastTouchedElement, touchEvent);
                        break;

                    case PointerState.Out:
                    case PointerState.Cancel:
                        touchEvent.Action = TouchAction.Move;

                        // generate enter/leave events if we passed from an element to another without move events
                        if (currentTouchedElement != lastTouchedElement)
                            ThrowEnterAndLeaveTouchEvents(currentTouchedElement, lastTouchedElement, touchEvent);

                        // then raise leave event to all the hierarchy of the previously selected element.
                        var element = currentTouchedElement;
                        while (element != null)
                        {
                            if (element.IsTouched)
                                element.RaiseTouchLeaveEvent(touchEvent);
                            element = element.VisualParent;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                lastTouchPosition = currentTouchPosition;
                state.LastTouchedElement = currentTouchedElement;
                state.LastIntersectionPoint = intersectionPoint;
            }
        }

        private void UpdateMouseOver(RenderContext context, UIComponentProcessor.UIComponentState state)
        {
            if (!input.HasMouse)
                return;

            var intersectionPoint = Vector3.Zero;
            var mousePosition = input.MousePosition;
            var rootElement = state.UIComponent.RootElement;
            var lastOveredElement = state.LastOveredElement;
            var overredElement = lastOveredElement;

            // determine currently overred element.
            if (mousePosition != lastMousePosition)
                overredElement = GetElementAtScreenPosition(context, rootElement, mousePosition, ref intersectionPoint);

            // find the common parent between current and last overred elements
            var commonElement = FindCommonParent(overredElement, lastOveredElement);

            // disable mouse over state to previously overred hierarchy
            var parent = lastOveredElement;
            while (parent != commonElement && parent != null)
            {
                parent.MouseOverState = MouseOverState.MouseOverNone;
                parent = parent.VisualParent;
            }

            // enable mouse over state to currently overred hierarchy
            if (overredElement != null)
            {
                // the element itself
                overredElement.MouseOverState = MouseOverState.MouseOverElement;

                // its hierarchy
                parent = overredElement.VisualParent;
                while (parent != null)
                {
                    if (parent.IsHierarchyEnabled)
                        parent.MouseOverState = MouseOverState.MouseOverChild;

                    parent = parent.VisualParent;
                }
            }

            // update cached values
            state.LastOveredElement = overredElement;
            lastMousePosition = mousePosition;
        }

        private UIElement FindCommonParent(UIElement element1, UIElement element2)
        {
            // build the list of the parents of the newly selected element
            newlySelectedElementParents.Clear();
            var newElementParent = element1;
            while (newElementParent != null)
            {
                newlySelectedElementParents.Add(newElementParent);
                newElementParent = newElementParent.VisualParent;
            }

            // find the common element into the previously and newly selected element hierarchy
            var commonElement = element2;
            while (commonElement != null && !newlySelectedElementParents.Contains(commonElement))
                commonElement = commonElement.VisualParent;

            return commonElement;
        }

        private void ThrowEnterAndLeaveTouchEvents(UIElement currentElement, UIElement previousElement, TouchEventArgs touchEvent)
        {
            var commonElement = FindCommonParent(currentElement, previousElement);

            // raise leave events to the hierarchy: previousElt -> commonElementParent
            var previousElementParent = previousElement;
            while (previousElementParent != commonElement && previousElementParent != null)
            {
                if (previousElementParent.IsHierarchyEnabled && previousElementParent.IsTouched)
                {
                    touchEvent.Handled = false; // reset 'handled' because it corresponds to another event
                    previousElementParent.RaiseTouchLeaveEvent(touchEvent);
                }
                previousElementParent = previousElementParent.VisualParent;
            }

            // raise enter events to the hierarchy: newElt -> commonElementParent
            var newElementParent = currentElement;
            while (newElementParent != commonElement && newElementParent != null)
            {
                if (newElementParent.IsHierarchyEnabled && !newElementParent.IsTouched)
                {
                    touchEvent.Handled = false; // reset 'handled' because it corresponds to another event
                    newElementParent.RaiseTouchEnterEvent(touchEvent);
                }
                newElementParent = newElementParent.VisualParent;
            }
        }

        private UIElement GetElementAtScreenPosition(RenderContext context, UIElement rootElement, Vector2 position, ref Vector3 intersectionPoint)
        {
            // here we use a trick to take into the calculation the viewport => we multiply the screen position by the viewport ratio (easier than modifying the view matrix)
            var viewport = context.GraphicsDevice.Viewport;
            var targetViewportRatio = new Vector2(CurrentRenderFrame.RenderTarget.Width / viewport.Width, CurrentRenderFrame.RenderTarget.Height / viewport.Height);
            var positionForHitTest = targetViewportRatio * position - new Vector2(0.5f);

            // calculate the ray corresponding to the click
            var rayDirectionView = Vector3.Normalize(new Vector3(positionForHitTest.X * cameraState.FrustumHeight * cameraState.AspectRatio, positionForHitTest.Y * cameraState.FrustumHeight, -1));
            var clickRay = new Ray(cameraState.ViewMatrixInverse.TranslationVector, Vector3.TransformNormal(rayDirectionView, cameraState.ViewMatrixInverse));

            // perform the hit test
            UIElement clickedElement = null;
            var smallestDepth = float.PositiveInfinity;
            PerformRecursiveHitTest(context, rootElement, ref clickRay, ref clickedElement, ref intersectionPoint, ref smallestDepth);

            return clickedElement;
        }

        private void PerformRecursiveHitTest(RenderContext context, UIElement element, ref Ray ray, ref UIElement clickedElement, ref Vector3 intersectionPoint, ref float smallestDepth)
        {
            // if the element is not visible, we also remove all its children
            if (!element.IsVisible)
                return;

            if (element.ClipToBounds || element.CanBeHitByUser)
            {
                Vector3 intersection;
                var intersect = element.Intersects(ref ray, out intersection);

                // don't perform the hit test on children if clipped and parent no hit
                if (element.ClipToBounds && !intersect)
                    return;

                // Calculate the depth of the element with the depth bias so that hit test corresponds to visuals.
                Vector4 projectedIntersection;
                var intersection4 = new Vector4(intersection, 1);
                Vector4.Transform(ref intersection4, ref context.ViewProjectionMatrix, out projectedIntersection);
                var depthWithBias = projectedIntersection.Z / projectedIntersection.W - element.DepthBias * BatchBase<int>.DepthBiasShiftOneUnit;

                // update the closest element hit
                if (element.CanBeHitByUser && intersect && depthWithBias < smallestDepth)
                {
                    smallestDepth = depthWithBias;
                    intersectionPoint = intersection;
                    clickedElement = element;
                }
            }

            // render the children
            foreach (var child in element.HitableChildren)
                PerformRecursiveHitTest(context, child, ref ray, ref clickedElement, ref intersectionPoint, ref smallestDepth);
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

        private class CameraState
        {
            public float AspectRatio;
            public float FrustumHeight;
            public Matrix ViewMatrixInverse;

            public void Update(CameraComponentState state)
            {
                AspectRatio = state.CameraComponent.AspectRatio;
                FrustumHeight = 2 * (float)Math.Tan(MathUtil.DegreesToRadians(state.CameraComponent.VerticalFieldOfView) / 2);
                ViewMatrixInverse = Matrix.Invert(state.View);
            }
        }
    }
}