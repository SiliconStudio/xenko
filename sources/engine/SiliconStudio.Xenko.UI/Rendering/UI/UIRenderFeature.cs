// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
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
    public partial class UIRenderFeature : RootRenderFeature
    {
        private IGame game;
        private UISystem uiSystem;
        private InputManager input;
        private IGraphicsDeviceService graphicsDeviceService;

        private RendererManager rendererManager;

        private readonly UIRenderingContext renderingContext = new UIRenderingContext();

        private UIBatch batch;

        private readonly LayoutingContext layoutingContext = new LayoutingContext();

        private readonly List<RenderUIElement> uiElementStates = new List<RenderUIElement>();

        private readonly ViewParameters viewParameters = new ViewParameters();

        private Vector2 viewportTargetRatio;
        private Vector2 viewportOffset;

        public override Type SupportedRenderObjectType => typeof(RenderUIElement);

        protected override void InitializeCore()
        {
            base.InitializeCore();

            Name = "UIComponentRenderer";
            game = (IGame)RenderSystem.Services.GetService(typeof(IGame));
            input = (InputManager)RenderSystem.Services.GetService(typeof(InputManager));
            uiSystem = (UISystem)RenderSystem.Services.GetService(typeof(UISystem));
            graphicsDeviceService = RenderSystem.Services.GetSafeServiceAs<IGraphicsDeviceService>();

            if (uiSystem == null)
            {
                var gameSytems = RenderSystem.Services.GetServiceAs<IGameSystemCollection>();
                uiSystem = new UISystem(RenderSystem.Services);
                gameSytems.Add(uiSystem);
            }

            rendererManager = new RendererManager(new DefaultRenderersFactory(RenderSystem.Services));

            batch = uiSystem.Batch;
        }

        partial void PickingPrepare(RenderDrawContext context);

        partial void PickingUpdate(RenderUIElement renderUIElement, Viewport viewport, GameTime drawTime);

        partial void PickingClear();

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            base.Draw(context, renderView, renderViewStage, startIndex, endIndex);

            var currentRenderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);

            var uiProcessor = renderView.SceneInstance.GetProcessor<UIRenderProcessor>();
            if (uiProcessor == null)
                return;


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

            // Prepare content required for Picking and MouseOver events
            PickingPrepare(context);

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

            // TODO Include using (Profiler.Begin(UIProfilerKeys.TouchEventsUpdate)) somewhere...

            // render the UI elements of all the entities
            foreach (var uiElementState in uiElementStates)
            {
                var uiComponent = uiElementState.UIComponent;
                var rootElement = uiComponent.RootElement;
                if (rootElement == null)
                    continue;

                var updatableRootElement = (IUIElementUpdate)rootElement;

                // calculate the size of the virtual resolution depending on target size (UI canvas)
                var virtualResolution = uiComponent.Resolution;
                
                if (uiComponent.IsFullScreen)
                {
                    //var targetSize = viewportSize;
                    var targetSize = new Vector2(renderingContext.RenderTarget.Width, renderingContext.RenderTarget.Height);

                    // update the virtual resolution of the renderer
                    if (uiComponent.ResolutionStretch == ResolutionStretch.FixedWidthAdaptableHeight)
                        virtualResolution.Y = virtualResolution.X * targetSize.Y / targetSize.X;
                    if (uiComponent.ResolutionStretch == ResolutionStretch.FixedHeightAdaptableWidth)
                        virtualResolution.X = virtualResolution.Y * targetSize.X / targetSize.Y;

                    viewParameters.Update(uiComponent.Entity, virtualResolution);
                }
                else
                {
                    var cameraComponent = context.RenderContext.Tags.Get(CameraComponentRendererExtensions.Current);
                    if (cameraComponent != null)
                        viewParameters.Update(uiComponent.Entity, cameraComponent);
                }

                PickingUpdate(uiElementState, context.CommandList.Viewport, drawTime);


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
                for (var i = 0; i < 4; i++)
                {
                    transformedVirtualWidth[i] = virtualWidth[0] * viewParameters.ViewProjectionMatrix[0 + i] + viewParameters.ViewProjectionMatrix[12 + i];
                    transformedVirtualHeight[i] = virtualHeight[1] * viewParameters.ViewProjectionMatrix[4 + i] + viewParameters.ViewProjectionMatrix[12 + i];
                }

                var viewportSize = context.CommandList.Viewport.Size;
                var projectedOrigin = virtualOrigin.XY() / virtualOrigin.W;
                var projectedVirtualWidth = viewportSize * (transformedVirtualWidth.XY() / transformedVirtualWidth.W - projectedOrigin);
                var projectedVirtualHeight = viewportSize * (transformedVirtualHeight.XY() / transformedVirtualHeight.W - projectedOrigin);

                // Set default services
                rootElement.UIElementServices = new UIElementServices { Services = RenderSystem.Services };

                // set default resource dictionary
                rootElement.ResourceDictionary = uiSystem.DefaultResourceDictionary;

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
                context.CommandList.SetRenderTarget(renderingContext.DepthStencilBuffer, renderingContext.RenderTarget);

                // start the image draw session
                renderingContext.StencilTestReferenceValue = 0;
                batch.Begin(context.GraphicsContext, ref viewParameters.ViewProjectionMatrix, BlendStates.AlphaBlend, uiSystem.KeepStencilValueState, renderingContext.StencilTestReferenceValue);

                // Render the UI elements in the final render target
                ReccursiveDrawWithClipping(context, rootElement);

                // end the image draw session
                batch.End();
            }

            PickingClear();

            // revert the depth stencil buffer to the default value 
            context.CommandList.SetRenderTargets(currentRenderFrame.DepthStencil, currentRenderFrame.RenderTargets);

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

                if (!uiComponent.IsFullScreen && (uiComponent.IsBillboard || uiComponent.IsFixedSize))
                {
                    Matrix viewInverse;
                    Matrix.Invert(ref camera.ViewMatrix, out viewInverse);
                    var forwardVector = viewInverse.Forward;

                    if (uiComponent.IsBillboard)
                    {
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

                    if (uiComponent.IsFixedSize)
                    {
                        forwardVector.Normalize();
                        var distVec = (worldMatrix.TranslationVector - camera.Entity.Transform.Position);
                        float distScalar;
                        Vector3.Dot(ref forwardVector, ref distVec, out distScalar);
                        distScalar = Math.Abs(distScalar);

                        var worldScale = FrustumHeight * distScalar * UIComponent.FixedSizeVerticalUnit; // FrustumHeight already is 2*Tan(FOV/2)

                        worldMatrix.Row1 *= worldScale;
                        worldMatrix.Row2 *= worldScale;
                        worldMatrix.Row3 *= worldScale;
                    }
                }


                // Rotation of Pi along 0x to go from UI space to world space
                worldMatrix.Row2 = -worldMatrix.Row2;
                worldMatrix.Row3 = -worldMatrix.Row3;

                // If the UI component is not drawn fullscreen it should be drawn as a quad with world sizes corresponding to its actual size
                if (!uiComponent.IsFullScreen)
                {
                    worldMatrix = Matrix.Scaling(uiComponent.Size / uiComponent.Resolution) * worldMatrix;
                }

                ProjectionMatrix = camera.ProjectionMatrix;
                Matrix.Multiply(ref worldMatrix, ref camera.ViewMatrix, out ViewMatrix);
                Matrix.Invert(ref ViewMatrix, out ViewMatrixInverse);
                Matrix.Multiply(ref ViewMatrix, ref ProjectionMatrix, out ViewProjectionMatrix);

                // TODO XK-3367 This only works for a single view
                // Save the World-View-Projection matrix with which this component is being currently drawn
                Matrix.Multiply(ref entity.Get<TransformComponent>().WorldMatrix, ref ViewProjectionMatrix, out uiComponent.WorldViewProjectionCached);
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
