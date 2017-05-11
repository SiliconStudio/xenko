// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for mouse devices, implements some common functionality of <see cref="IMouseDevice"/>, inherits from <see cref="PointerDeviceBase"/>
    /// </summary>
    public abstract class MouseDeviceBase : PointerDeviceBase, IMouseDevice
    {
        private Vector2 nextDelta = Vector2.Zero;

        protected readonly List<InputEvent> Events = new List<InputEvent>();

        private readonly HashSet<MouseButton> pressedButtons = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> releasedButtons = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> downButtons = new HashSet<MouseButton>();

        protected MouseDeviceBase()
        {
            DownButtons = new ReadOnlySet<MouseButton>(downButtons);
            PressedButtons = new ReadOnlySet<MouseButton>(pressedButtons);
            ReleasedButtons = new ReadOnlySet<MouseButton>(releasedButtons);
        }

        public abstract bool IsPositionLocked { get; }
        
        public Vector2 Position { get; protected set; }
        public Vector2 Delta { get; protected set; }

        public IReadOnlySet<MouseButton> PressedButtons { get; }
        public IReadOnlySet<MouseButton> ReleasedButtons { get; }
        public IReadOnlySet<MouseButton> DownButtons { get; }

        public override PointerType Type => PointerType.Mouse;
        
        public override void Update(List<InputEvent> inputEvents)
        {
            base.Update(inputEvents);
            
            pressedButtons.Clear();
            releasedButtons.Clear();

            // Collect events from queue
            foreach (var evt in Events)
            {
                inputEvents.Add(evt);

                var mouseButtonEvent = evt as MouseButtonEvent;
                if (mouseButtonEvent != null)
                {
                    if (mouseButtonEvent.IsDown)
                    {
                        pressedButtons.Add(mouseButtonEvent.Button);
                    }
                    else
                    {
                        releasedButtons.Add(mouseButtonEvent.Button);
                    }
                }
            }
            Events.Clear();

            // Reset mouse delta
            Delta = nextDelta;
            nextDelta = Vector2.Zero;
        }

        public bool IsButtonPressed(MouseButton mouseButton)
        {
            return pressedButtons.Contains(mouseButton);
        }

        public bool IsButtonReleased(MouseButton mouseButton)
        {
            return releasedButtons.Contains(mouseButton);
        }

        public virtual bool IsButtonDown(MouseButton mouseButton)
        {
            return downButtons.Contains(mouseButton);
        }

        public abstract void SetPosition(Vector2 normalizedPosition);
        
        public abstract void LockPosition(bool forceCenter = false);
        
        public abstract void UnlockPosition();

        /// <summary>
        /// Special move that generates pointer events with just delta
        /// </summary>
        /// <param name="delta">The movement delta</param>
        protected void HandleMouseDelta(Vector2 delta)
        {
            if (delta == Vector2.Zero)
                return;

            // Normalize delta
            delta *= InverseSurfaceSize;

            var data = GetPointerData(0);

            // Update pointer position + delta
            // Update delta
            data.Delta = delta;
            nextDelta += delta;
            
            data.Clock.Restart();

            var pointerEvent = InputEventPool<PointerEvent>.GetOrCreate(this);
            pointerEvent.Position = data.Position;
            pointerEvent.DeltaPosition = data.Delta;
            pointerEvent.DeltaTime = data.Clock.Elapsed;
            pointerEvent.IsDown = data.IsDown;
            pointerEvent.PointerId = 0;
            pointerEvent.PointerType = Type;
            pointerEvent.EventType = PointerEventType.Moved;

            Events.Add(pointerEvent);
        }

        public void HandleButtonDown(MouseButton button)
        {
            // Prevent duplicate events
            if (downButtons.Contains(button))
                return;

            downButtons.Add(button);

            var buttonEvent = InputEventPool<MouseButtonEvent>.GetOrCreate(this);
            buttonEvent.Button = button;
            buttonEvent.IsDown = true;
            Events.Add(buttonEvent);

            // Simulate tap on primary mouse button
            if (button == MouseButton.Left)
                HandlePointerDown();
        }

        public void HandleButtonUp(MouseButton button)
        {
            // Prevent duplicate events
            if (!downButtons.Contains(button))
                return;

            downButtons.Remove(button);

            var buttonEvent = InputEventPool<MouseButtonEvent>.GetOrCreate(this);
            buttonEvent.Button = button;
            buttonEvent.IsDown = false;
            Events.Add(buttonEvent);

            // Simulate tap on primary mouse button
            if (button == MouseButton.Left)
                HandlePointerUp();
        }

        public void HandleMouseWheel(float wheelDelta)
        {
            var wheelEvent = InputEventPool<MouseWheelEvent>.GetOrCreate(this);
            wheelEvent.WheelDelta = wheelDelta;
            Events.Add(wheelEvent);
        }
        
        /// <summary>
        /// Handles a single pointer down
        /// </summary>
        protected void HandlePointerDown()
        {
            PointerInputEvents.Add(new PointerInputEvent { Type = PointerEventType.Pressed, Position = Position, Id = 0 });
        }

        /// <summary>
        /// Handles a single pointer up
        /// </summary>
        protected void HandlePointerUp()
        {
            PointerInputEvents.Add(new PointerInputEvent { Type = PointerEventType.Released, Position = Position, Id = 0 });
        }

        /// <summary>
        /// Handles a single pointer move
        /// </summary>
        /// <param name="newPosition">New position of the pointer</param>
        protected void HandleMove(Vector2 newPosition)
        {
            // Normalize position
            newPosition *= InverseSurfaceSize;

            if (newPosition != Position)
            {
                nextDelta += newPosition - Position;
                Position = newPosition;

                // Generate Event
                PointerInputEvents.Add(new PointerInputEvent { Type = PointerEventType.Moved, Position = newPosition, Id = 0 });
            }
        }
    }
}