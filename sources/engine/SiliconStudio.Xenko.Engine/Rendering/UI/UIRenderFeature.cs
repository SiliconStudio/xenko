// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Renderers;

namespace SiliconStudio.Xenko.Rendering.UI
{
    public class UIRenderFeature : RootRenderFeature
    {
        private IGame game;
        private UISystem uiSystem;
        private InputManager input;

        private RendererManager rendererManager;

        private readonly UIRenderingContext renderingContext = new UIRenderingContext();

        private UIBatch batch;

        private readonly LayoutingContext layoutingContext = new LayoutingContext();

        private Vector2 lastMousePosition;

        // object to avoid allocation at each element leave event
        private readonly HashSet<UIElement> newlySelectedElementParents = new HashSet<UIElement>();

        private readonly List<PointerEvent> compactedPointerEvents = new List<PointerEvent>();

        private readonly List<RenderUIElement> uiElementStates = new List<RenderUIElement>();

        private readonly ViewParameters viewParameters = new ViewParameters();

        private Vector2 viewportTargetRatio;

        public override Type SupportedRenderObjectType => typeof(RenderUIElement);

        protected override void InitializeCore()
        {
            base.InitializeCore();

            Name = "UIComponentRenderer";
            game = (IGame)RenderSystem.Services.GetService(typeof(IGame));
            input = (InputManager)RenderSystem.Services.GetService(typeof(InputManager));
            uiSystem = (UISystem)RenderSystem.Services.GetService(typeof(UISystem));

            rendererManager = new RendererManager(new DefaultRenderersFactory(RenderSystem.Services));

            batch = uiSystem.Batch;
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            base.Draw(context, renderView, renderViewStage, startIndex, endIndex);

            var currentRenderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);

            var uiProcessor = renderView.SceneInstance.GetProcessor<UIRenderProcessor>();
            if (uiProcessor == null)
                return;

            //foreach (var uiRoot in uiProcessor.UIRoots)
            //{
            //    // Perform culling on group and accept
            //    if (!renderView.SceneCameraRenderer.CullingMask.Contains(uiRoot.UIComponent.Entity.Group))
            //        continue;

            //    // skips empty UI elements
            //    if (uiRoot.UIComponent.RootElement == null)
            //        continue;

            //    // Project the position
            //    // TODO: This code is duplicated from SpriteComponent -> unify it at higher level?
            //    var worldPosition = new Vector4(uiRoot.TransformComponent.WorldMatrix.TranslationVector, 1.0f);

            //    float projectedZ;
            //    if (uiRoot.UIComponent.IsFullScreen)
            //    {
            //        projectedZ = -uiRoot.TransformComponent.WorldMatrix.M43;
            //    }
            //    else
            //    {
            //        Vector4 projectedPosition;
            //        var cameraComponent = renderView.Camera;
            //        if (cameraComponent == null)
            //            continue;

            //        Vector4.Transform(ref worldPosition, ref cameraComponent.ViewProjectionMatrix, out projectedPosition);
            //        projectedZ = projectedPosition.Z / projectedPosition.W;
            //    }

            //    transparentList.Add(new RenderItem(this, uiRoot, projectedZ));
            //}

            // build the list of the UI elements to render
            uiElementStates.Clear();
            for (var index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);
                var renderElement = (RenderUIElement)renderNode.RenderObject;
 
                uiElementStates.Add(renderElement);
            }

            // evaluate the current draw time (game instance is null for thumbnails)
            var drawTime = game != null ? game.DrawTime : new GameTime();

            // update the rendering context
            renderingContext.GraphicsContext = context.GraphicsContext;
            renderingContext.Time = drawTime;
            renderingContext.RenderTarget = currentRenderFrame.RenderTargets[0]; // TODO: avoid hardcoded index 0

            // cache the ratio between viewport and target.
            var viewportSize = context.CommandList.Viewport.Size;
            viewportTargetRatio = new Vector2(viewportSize.X / renderingContext.RenderTarget.Width, viewportSize.Y / renderingContext.RenderTarget.Height);

            // compact all the pointer events that happened since last frame to avoid performing useless hit tests.
            CompactPointerEvents();

            // allocate temporary graphics resources if needed
            Texture scopedDepthBuffer = null;
            foreach (var uiElement in uiElementStates)
            {
                if (uiElement.UIComponent.IsFullScreen)
                {
                    var renderTarget = renderingContext.RenderTarget;
                    var description = TextureDescription.New2D(renderTarget.Width, renderTarget.Height, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);
                    scopedDepthBuffer = context.RenderContext.Allocator.GetTemporaryTexture(description);
                    break;
                }
            }

            // render the UI elements of all the entities
            foreach (var uiElementState in uiElementStates)
            {
                var uiComponent = uiElementState.UIComponent;
                var rootElement = uiComponent.RootElement;
                if (rootElement == null)
                    continue;

                var updatableRootElement = (IUIElementUpdate)rootElement;

                // calculate the size of the virtual resolution depending on target size (UI canvas)
                var virtualResolution = uiComponent.VirtualResolution;
                var targetSize = new Vector2(renderingContext.RenderTarget.Width, renderingContext.RenderTarget.Height);
                if (uiComponent.IsFullScreen)
                {
                    // update the virtual resolution of the renderer
                    if (uiComponent.VirtualResolutionMode == VirtualResolutionMode.FixedWidthAdaptableHeight)
                        virtualResolution.Y = virtualResolution.X * targetSize.Y / targetSize.X;
                    if (uiComponent.VirtualResolutionMode == VirtualResolutionMode.FixedHeightAdaptableWidth)
                        virtualResolution.X = virtualResolution.Y * targetSize.X / targetSize.Y;
                }

                // Update the view parameters
                if (uiComponent.IsFullScreen)
                {
                    viewParameters.Update(uiComponent.Entity, virtualResolution);
                }
                else
                {
                    var cameraComponent = context.RenderContext.Tags.Get(CameraComponentRendererExtensions.Current);
                    viewParameters.Update(uiComponent.Entity, cameraComponent);
                }

                // Analyze the input and trigger the UI element touch and key events
                // Note: this is done before measuring/arranging/drawing the element in order to avoid one frame latency on clicks.
                //       But by doing so the world matrices taken for hit test are the ones calculated during last frame.
                using (Profiler.Begin(UIProfilerKeys.TouchEventsUpdate))
                {
                    foreach (var uiState in uiElementStates)
                    {
                        if (uiState.UIComponent.RootElement == null)
                            continue;

                        UpdateMouseOver(uiState);
                        UpdateTouchEvents(uiState, drawTime);
                    }
                }

                // update the rendering context values specific to this element
                renderingContext.Resolution = virtualResolution;
                renderingContext.ViewMatrix = viewParameters.ViewMatrix;
                renderingContext.ProjectionMatrix = viewParameters.ProjectionMatrix;
                renderingContext.ViewProjectionMatrix = viewParameters.ViewProjectionMatrix;
                renderingContext.DepthStencilBuffer = uiComponent.IsFullScreen ? scopedDepthBuffer : currentRenderFrame.DepthStencil;
                renderingContext.ShouldSnapText = uiComponent.SnapText;

                // calculate an estimate of the UI real size by projecting the element virtual resolution on the screen
                var virtualOrigin = viewParameters.ViewProjectionMatrix.Row4;
                var virtualWidth = new Vector4(virtualResolution.X / 2, 0, 0, 1);
                var virtualHeight = new Vector4(0, virtualResolution.Y / 2, 0, 1);
                var transformedVirtualWidth = Vector4.Zero;
                var transformedVirtualHeight = Vector4.Zero;
                for (int i = 0; i < 4; i++)
                {
                    transformedVirtualWidth[i] = virtualWidth[0] * viewParameters.ViewProjectionMatrix[0 + i] + viewParameters.ViewProjectionMatrix[12 + i];
                    transformedVirtualHeight[i] = virtualHeight[1] * viewParameters.ViewProjectionMatrix[4 + i] + viewParameters.ViewProjectionMatrix[12 + i];
                }
                var projectedOrigin = virtualOrigin.XY() / virtualOrigin.W;
                var projectedVirtualWidth = viewportSize * (transformedVirtualWidth.XY() / transformedVirtualWidth.W - projectedOrigin);
                var projectedVirtualHeight = viewportSize * (transformedVirtualHeight.XY() / transformedVirtualHeight.W - projectedOrigin);

                // update layouting context.
                layoutingContext.VirtualResolution = virtualResolution;
                layoutingContext.RealResolution = viewportSize;
                layoutingContext.RealVirtualResolutionRatio = new Vector2(projectedVirtualWidth.Length() / virtualResolution.X, projectedVirtualHeight.Length() / virtualResolution.Y);
                rootElement.LayoutingContext = layoutingContext;

                // perform the time-based updates of the UI element
                updatableRootElement.Update(drawTime);

                // update the UI element disposition
                rootElement.Measure(virtualResolution);
                rootElement.Arrange(virtualResolution, false);

                // update the UI element hierarchical properties
                var rootMatrix = Matrix.Translation(-virtualResolution / 2); // UI world is rotated of 180degrees along Ox
                updatableRootElement.UpdateWorldMatrix(ref rootMatrix, rootMatrix != uiElementState.LastRootMatrix);
                updatableRootElement.UpdateElementState(0);
                uiElementState.LastRootMatrix = rootMatrix;

                // clear and set the Depth buffer as required
                if (uiComponent.IsFullScreen)
                {
                    context.CommandList.Clear(renderingContext.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);
                }
                context.CommandList.SetRenderTargetAndViewport(renderingContext.DepthStencilBuffer, renderingContext.RenderTarget);

                // start the image draw session
                renderingContext.StencilTestReferenceValue = 0;
                batch.Begin(context.GraphicsContext, ref viewParameters.ViewProjectionMatrix, BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);

                // Render the UI elements in the final render target
                ReccursiveDrawWithClipping(context, rootElement);

                // end the image draw session
                batch.End();
            }

            // clear the list of compacted pointer events of time frame
            ClearPointerEvents();

            // revert the depth stencil buffer to the default value 
            context.CommandList.SetRenderTargetsAndViewport(currentRenderFrame.DepthStencil, currentRenderFrame.RenderTargets);

            // Release scroped texture
            if (scopedDepthBuffer != null)
            {
                context.RenderContext.Allocator.ReleaseReference(scopedDepthBuffer);
            }
        }

        private void ReccursiveDrawWithClipping(RenderDrawContext context, UIElement element)
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
                batch.Begin(context.GraphicsContext, ref viewParameters.ViewProjectionMatrix, BlendStates.ColorDisabled, uiSystem.IncreaseStencilValueState, renderingContext.StencilTestReferenceValue);
                renderer.RenderClipping(element, renderingContext);
                batch.End();

                // update context and restart the batch
                renderingContext.StencilTestReferenceValue += 1;
                batch.Begin(context.GraphicsContext, ref viewParameters.ViewProjectionMatrix, BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);
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
                batch.Begin(context.GraphicsContext, ref viewParameters.ViewProjectionMatrix, BlendStates.ColorDisabled, uiSystem.DecreaseStencilValueState, renderingContext.StencilTestReferenceValue);
                renderer.RenderClipping(element, renderingContext);
                batch.End();

                // update context and restart the batch
                renderingContext.StencilTestReferenceValue -= 1;
                batch.Begin(context.GraphicsContext, ref viewParameters.ViewProjectionMatrix, BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);
            }
        }

        private void CompactPointerEvents()
        {
            if (input == null) // no input for thumbnails
                return;

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

        private void UpdateTouchEvents(RenderUIElement state, GameTime gameTime)
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
                    currentTouchedElement = GetElementAtScreenPosition(rootElement, pointerEvent.Position, ref intersectionPoint);

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

        private void UpdateMouseOver(RenderUIElement state)
        {
            if (input == null || !input.HasMouse)
                return;

            var intersectionPoint = Vector3.Zero;
            var mousePosition = input.MousePosition;
            var rootElement = state.UIComponent.RootElement;
            var lastOveredElement = state.LastOveredElement;
            var overredElement = lastOveredElement;

            // determine currently overred element.
            if (mousePosition != lastMousePosition)
                overredElement = GetElementAtScreenPosition(rootElement, mousePosition, ref intersectionPoint);

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

        private UIElement GetElementAtScreenPosition(UIElement rootElement, Vector2 position, ref Vector3 intersectionPoint)
        {
            // here we use a trick to take into the calculation the viewport => we multiply the screen position by the viewport ratio (easier than modifying the view matrix)
            var positionForHitTest = Vector2.Demodulate(position, viewportTargetRatio) - new Vector2(0.5f);

            // calculate the ray corresponding to the click
            var rayDirectionView = Vector3.Normalize(new Vector3(positionForHitTest.X * viewParameters.FrustumHeight * viewParameters.AspectRatio, -positionForHitTest.Y * viewParameters.FrustumHeight, -1));
            var clickRay = new Ray(viewParameters.ViewMatrixInverse.TranslationVector, Vector3.TransformNormal(rayDirectionView, viewParameters.ViewMatrixInverse));

            // perform the hit test
            UIElement clickedElement = null;
            var smallestDepth = float.PositiveInfinity;
            PerformRecursiveHitTest(rootElement, ref clickRay, ref clickedElement, ref intersectionPoint, ref smallestDepth);

            return clickedElement;
        }

        private void PerformRecursiveHitTest(UIElement element, ref Ray ray, ref UIElement clickedElement, ref Vector3 intersectionPoint, ref float smallestDepth)
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
                Vector4.Transform(ref intersection4, ref viewParameters.ViewProjectionMatrix, out projectedIntersection);
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
                PerformRecursiveHitTest(child, ref ray, ref clickedElement, ref intersectionPoint, ref smallestDepth);
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

        private class ViewParameters
        {
            public float AspectRatio;
            public float FrustumHeight;
            public Matrix ViewMatrix;
            public Matrix ViewMatrixInverse;
            public Matrix ProjectionMatrix;
            public Matrix ViewProjectionMatrix;

            public void Update(Entity entity, CameraComponent camera)
            {
                AspectRatio = camera.AspectRatio;
                FrustumHeight = 2 * (float)Math.Tan(MathUtil.DegreesToRadians(camera.VerticalFieldOfView) / 2);

                // extract the world matrix of the UI entity
                var worldMatrix = entity.Get<TransformComponent>().WorldMatrix;

                // rotate the UI element perpendicular to the camera view vector, if billboard is activated
                var uiComponent = entity.Get<UIComponent>();
                if (!uiComponent.IsFullScreen && uiComponent.IsBillboard)
                {
                    Matrix viewInverse;
                    Matrix.Invert(ref camera.ViewMatrix, out viewInverse);

                    // remove scale of the camera
                    viewInverse.Row1 /= viewInverse.Row1.XYZ().Length();
                    viewInverse.Row2 /= viewInverse.Row2.XYZ().Length();

                    // set the scale of the object
                    viewInverse.Row1 *= worldMatrix.Row1.XYZ().Length();
                    viewInverse.Row2 *= worldMatrix.Row2.XYZ().Length();

                    // set the adjusted world matrix
                    worldMatrix.Row1 = viewInverse.Row1;
                    worldMatrix.Row2 = viewInverse.Row2;
                    worldMatrix.Row3 = viewInverse.Row3;
                }

                // Rotation of Pi along 0x to go from UI space to world space
                worldMatrix.Row2 = -worldMatrix.Row2;
                worldMatrix.Row3 = -worldMatrix.Row3;

                ProjectionMatrix = camera.ProjectionMatrix;
                Matrix.Multiply(ref worldMatrix, ref camera.ViewMatrix, out ViewMatrix);
                Matrix.Invert(ref ViewMatrix, out ViewMatrixInverse);
                Matrix.Multiply(ref ViewMatrix, ref ProjectionMatrix, out ViewProjectionMatrix);
            }

            public void Update(Entity entity, Vector3 virtualResolution)
            {
                var nearPlane = virtualResolution.Z / 2;
                var farPlane = nearPlane + virtualResolution.Z;
                var zOffset = nearPlane + virtualResolution.Z / 2;
                var aspectRatio = virtualResolution.X / virtualResolution.Y;
                var verticalFov = (float)Math.Atan2(virtualResolution.Y / 2, zOffset) * 2;

                var cameraComponent = new CameraComponent(nearPlane, farPlane)
                {
                    UseCustomAspectRatio = true,
                    AspectRatio = aspectRatio,
                    VerticalFieldOfView = MathUtil.RadiansToDegrees(verticalFov),
                    ViewMatrix = Matrix.LookAtRH(new Vector3(0, 0, zOffset), Vector3.Zero, Vector3.UnitY),
                    ProjectionMatrix = Matrix.PerspectiveFovRH(verticalFov, aspectRatio, nearPlane, farPlane),
                };

                Update(entity, cameraComponent);
            }
        }
    }
}
