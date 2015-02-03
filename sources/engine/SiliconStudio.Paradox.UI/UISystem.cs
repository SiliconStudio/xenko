// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Interface of the UI system.
    /// </summary>
    public class UISystem : GameSystemBase
    {
        internal Matrix WorldMatrix;

        private UIElement rootElement;
        /// <summary>
        /// Gets or  sets the root element of the UI
        /// </summary>
        /// <value>The root element.</value>
        public UIElement RootElement
        {
            get { return rootElement;}
            set
            {
                rootElement = value;

                if(rootElement != null)
                    ((IUIElementUpdate)rootElement).UpdateUISystemReference(this);
            }
        }

        internal Matrix ViewProjectionInternal;

        /// <summary>
        /// The view matrix used to render the UI system
        /// </summary>
        /// <value>The view matrix.</value>
        public Matrix ViewMatrix { get; private set; }

        /// <summary>
        /// The project matrix used to render the UI system
        /// </summary>
        /// <value>The projection matrix.</value>
        public Matrix ProjectionMatrix { get; private set; }

        /// <summary>
        /// The view/project matrix used to render the UI system
        /// </summary>
        /// <value>The view projection matrix.</value>
        public Matrix ViewProjectionMatrix { get { return ViewProjectionInternal; } }

        public event EventHandler<EventArgs> ResolutionChanged;

        internal UIBatch Batch { get; private set; }

        internal DepthStencilState KeepStencilValueState { get; private set; }

        internal DepthStencilState IncreaseStencilValueState { get; private set; }

        internal DepthStencilState DecreaseStencilValueState { get; private set; }

        internal Vector2 BackBufferVirtualResolutionRatio { get; private set; }

        private Vector3 virtualResolution;
        
        private InputManagerBase input;

        private UIElement lastTouchedElement;
        private Vector3 lastIntersectionPoint;

        // object to avoid allocation at each element leave event
        private readonly HashSet<UIElement> newlySelectedElementParents = new HashSet<UIElement>();

        private readonly List<PointerEvent> compactedPointerEvents = new List<PointerEvent>();

        private readonly IVirtualResolution gameVirtualResolution;

        private float uiFrustumHeight;

        private Matrix inverseViewMatrix;

        /// <summary>
        /// The position of the view camera along z axis.
        /// </summary>
        private float viewPositionZ;

        private float nearPlane;

        private float farPlane;

        private Vector2 lastMousePosition;

        private UIElement lastOveredElement;

        public UISystem(IServiceRegistry registry)
            : base(registry)
        {
            Services.AddService(typeof(UISystem), this);

            gameVirtualResolution = (IVirtualResolution)Services.GetService(typeof(IVirtualResolution));
            gameVirtualResolution.VirtualResolutionChanged += OnGameVirtualResolutionChanged;
        }

        private void OnGameVirtualResolutionChanged(object sender, EventArgs eventArgs)
        {
            VirtualResolution = gameVirtualResolution.VirtualResolution;
        }

        public override void Initialize()
        {
            base.Initialize();

            input = Services.GetSafeServiceAs<InputManager>();

            VirtualResolution = new Vector3(GraphicsDevice.BackBuffer.ViewWidth, GraphicsDevice.BackBuffer.ViewHeight, 1000);

            Enabled = true;
            Visible = true;

            Game.Window.ClientSizeChanged += WindowOnClientSizeChanged;

            Game.Activated += OnApplicationResumed;
            Game.Deactivated += OnApplicationPaused;
        }

        protected override void Destroy()
        {
            gameVirtualResolution.VirtualResolutionChanged -= OnGameVirtualResolutionChanged;

            Game.Activated -= OnApplicationResumed;
            Game.Deactivated -= OnApplicationPaused;

            // ensure that OnApplicationPaused is called before destruction, when Game.Deactivated event is not triggered.
            OnApplicationPaused(this, EventArgs.Empty);

            base.Destroy();
        }

        private void WindowOnClientSizeChanged(object sender, EventArgs eventArgs)
        {
            BackBufferVirtualResolutionRatio = CalculateBackBufferVirtualResolutionRatio();
        }

        private Vector2 CalculateBackBufferVirtualResolutionRatio()
        {
            return new Vector2(GraphicsDevice.BackBuffer.ViewWidth / virtualResolution.X, GraphicsDevice.BackBuffer.ViewHeight / virtualResolution.Y);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            // create effect and geometric primitives
            Batch = new UIBatch(GraphicsDevice);

            // create depth stencil states
            var depthStencilDescription = new DepthStencilStateDescription(true, true)
                {
                    StencilEnable = true,
                    FrontFace = new DepthStencilStencilOpDescription
                    {
                        StencilDepthBufferFail = StencilOperation.Keep,
                        StencilFail = StencilOperation.Keep,
                        StencilPass = StencilOperation.Keep,
                        StencilFunction = CompareFunction.Equal
                    },
                    BackFace = new DepthStencilStencilOpDescription
                    {
                        StencilDepthBufferFail = StencilOperation.Keep,
                        StencilFail = StencilOperation.Keep,
                        StencilPass = StencilOperation.Keep,
                        StencilFunction = CompareFunction.Equal
                    },
                };
            KeepStencilValueState = DepthStencilState.New(GraphicsDevice, depthStencilDescription);

            depthStencilDescription.FrontFace.StencilPass = StencilOperation.Increment;
            depthStencilDescription.BackFace.StencilPass = StencilOperation.Increment;
            IncreaseStencilValueState = DepthStencilState.New(GraphicsDevice, depthStencilDescription);

            depthStencilDescription.FrontFace.StencilPass = StencilOperation.Decrement;
            depthStencilDescription.BackFace.StencilPass = StencilOperation.Decrement;
            DecreaseStencilValueState = DepthStencilState.New(GraphicsDevice, depthStencilDescription);

            // set the default design of the UI elements.
            var designsTexture = TextureExtensions.CreateTextureFromFileData(GraphicsDevice, DefaultDesigns.Designs);
            Button.PressedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default button pressed design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(71, 3, 32, 32)});
            Button.NotPressedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default button not pressed design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(3, 3, 32, 32) });
            Button.MouseOverImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default button overred design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(37, 3, 32, 32) });
            EditText.ActiveImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default edit active design", designsTexture) { Borders = 12 * Vector4.One, Region = new RectangleF(105, 3, 32, 32) });
            EditText.InactiveImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default edit inactive design", designsTexture) { Borders = 12 * Vector4.One, Region = new RectangleF(139, 3, 32, 32) });
            EditText.MouseOverImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default edit overred design", designsTexture) { Borders = 12 * Vector4.One, Region = new RectangleF(173, 3, 32, 32) });
            ToggleButton.CheckedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default toggle button checked design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(71, 3, 32, 32) });
            ToggleButton.UncheckedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default toggle button unchecked design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(3, 3, 32, 32) });
            ToggleButton.IndeterminateImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default toggle button indeterminate design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(37, 3, 32, 32) });
            Slider.TrackBackgroundImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default slider track background design", designsTexture) { Borders = 14 * Vector4.One, Region = new RectangleF(207, 3, 32, 32) });
            Slider.TrackForegroundImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default slider track foreground design", designsTexture) { Borders = 0 * Vector4.One, Region = new RectangleF(3, 37, 32, 32) });
            Slider.ThumbImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default slider thumb design", designsTexture) { Borders = 4 * Vector4.One, Region = new RectangleF(37, 37, 16, 32) });
            Slider.MouseOverThumbImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default slider thumb overred design", designsTexture) { Borders = 4 * Vector4.One, Region = new RectangleF(71, 37, 16, 32) });
            Slider.TickImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Default slider track foreground design", designsTexture) { Region = new RectangleF(245, 3, 3, 6) });
            Slider.TickOffsetPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(13f);
            Slider.TrackStartingOffsetsrPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new Vector2(3));
        }

        /// <summary>
        /// The method to call when the application is put on background.
        /// </summary>
        void OnApplicationPaused(object sender, EventArgs e)
        {
            // validate the edit text and close the keyboard, if any edit text is currently active
            var focusedEdit = UIElement.FocusedElement as EditText;
            if (focusedEdit != null)
                focusedEdit.IsSelectionActive = false;
        }

        /// <summary>
        /// The method to call when the application is put on foreground.
        /// </summary>
        void OnApplicationResumed(object sender, EventArgs e)
        {
            // revert the state of the edit text here?
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

                if(virtualResolution == value)
                    return;

                virtualResolution = value;

                var uiFieldOfView = (float)Math.Atan2(virtualResolution.Y / 2, virtualResolution.Z + 1f) * 2;
                uiFrustumHeight = 2 * (float)Math.Tan(uiFieldOfView / 2);

                nearPlane = 1f;
                farPlane = 1f + 2 * virtualResolution.Z;
                var projection = Matrix.PerspectiveFovRH(uiFieldOfView, virtualResolution.X / virtualResolution.Y, nearPlane, farPlane);
                projection.M22 = -projection.M22;

                ProjectionMatrix = projection;
                viewPositionZ = virtualResolution.Z + 1f;
                ViewMatrix = Matrix.LookAtRH(new Vector3(0, 0, viewPositionZ), Vector3.Zero, Vector3.UnitY);
                inverseViewMatrix = Matrix.Invert(ViewMatrix);
                ViewProjectionInternal = ViewMatrix * ProjectionMatrix;
                WorldMatrix = Matrix.Translation(-VirtualResolution.X / 2, -VirtualResolution.Y / 2, 0);
                BackBufferVirtualResolutionRatio = CalculateBackBufferVirtualResolutionRatio();

                var handler = ResolutionChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (RootElement == null) // no update if no UI
                return;

            // Analyze the input and trigger the UI element touch and key events
            // ReSharper disable once UnusedVariable
            using (var profiler = Profiler.Begin(UIProfilerKeys.TouchEventsUpdate))
            {
                UpdateMouseOver();
                UpdateTouchEvents(gameTime);
            }
            UpdateKeyEvents();
        }

        private void UpdateKeyEvents()
        {
            foreach (var keyEvent in input.KeyEvents)
            {
                if (UIElement.FocusedElement == null || !UIElement.FocusedElement.IsHierarchyEnabled) return;
                var key = keyEvent.Key;
                if (keyEvent.Type == KeyEventType.Pressed)
                {
                    UIElement.FocusedElement.RaiseKeyPressedEvent(new KeyEventArgs { Key = key, Input = input });
                }
                else
                {
                    UIElement.FocusedElement.RaiseKeyReleasedEvent(new KeyEventArgs { Key = key, Input = input });
                }
            }

            foreach (var key in input.KeyDown)
            {
                if (UIElement.FocusedElement == null || !UIElement.FocusedElement.IsHierarchyEnabled) return;
                UIElement.FocusedElement.RaiseKeyDownEvent(new KeyEventArgs { Key = key, Input = input });
            }
        }

        private void UpdateMouseOver()
        {
            if(!input.HasMouse)
                return;

            var intersectionPoint = Vector3.Zero;
            var mousePosition = input.MousePosition;
            var overredElement = lastOveredElement;
            
            // determine currently overred element.
            if(mousePosition != lastMousePosition)
                overredElement = GetElementFromPointerScreenPosition(mousePosition, ref intersectionPoint);

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
            if(overredElement != null)
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
            lastOveredElement = overredElement;
            lastMousePosition = mousePosition;
        }

        private void UpdateTouchEvents(GameTime gameTime)
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

            var intersectionPoint = Vector3.Zero;
            var lastTouchPosition = new Vector2(float.NegativeInfinity);

            // analyze pointer event input and trigger UI touch events depending on hit Tests
            foreach (var pointerEvent in compactedPointerEvents)
            {
                // performance optimization: skip all the events that started outside of the UI
                if(lastTouchedElement == null && pointerEvent.State != PointerState.Down)
                    continue;

                var time = gameTime.Total;
                var currentTouchPosition = pointerEvent.Position;
                var currentTouchedElement = lastTouchedElement;

                // re-calculate the element under cursor if click position changed.
                if (lastTouchPosition != currentTouchPosition)
                    currentTouchedElement = GetElementFromPointerScreenPosition(currentTouchPosition, ref intersectionPoint);

                if (pointerEvent.State == PointerState.Down || pointerEvent.State == PointerState.Up)
                    lastIntersectionPoint = intersectionPoint;

                var touchEvent = new TouchEventArgs
                {
                    Action = TouchAction.Down, 
                    Timestamp = time, 
                    ScreenPosition = currentTouchPosition, 
                    ScreenTranslation = pointerEvent.DeltaPosition,
                    WorldPosition = intersectionPoint,
                    WorldTranslation = intersectionPoint - lastIntersectionPoint
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
                            if(element.IsTouched)
                                element.RaiseTouchLeaveEvent(touchEvent);
                            element = element.VisualParent;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                lastTouchedElement = currentTouchedElement;
                lastTouchPosition = currentTouchPosition;
                lastIntersectionPoint = intersectionPoint;
            }

            // collect back pointer event not used anymore
            lock (PointerEvent.Pool)
            {
                foreach (var pointerEvent in compactedPointerEvents)
                    PointerEvent.Pool.Enqueue(pointerEvent);
            }
            compactedPointerEvents.Clear();
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
        private UIElement GetElementFromPointerScreenPosition(Vector2 position, ref Vector3 intersectionPoint)
        {
            // calculate the ray corresponding to the click
            var touchPosition = position - new Vector2(0.5f);
            var rayDirectionView = Vector3.Normalize(new Vector3(touchPosition.X * uiFrustumHeight * virtualResolution.X / virtualResolution.Y, touchPosition.Y * uiFrustumHeight, -1));
            var clickRay = new Ray(inverseViewMatrix.TranslationVector, Vector3.TransformNormal(rayDirectionView, inverseViewMatrix));

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
                var depthInView = (intersection.Z - viewPositionZ);
                var depthWithBias = (nearPlane * farPlane) / depthInView / (farPlane - nearPlane) - element.DepthBias * BatchBase<int>.DepthBiasShiftOneUnit;

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
    }
}
