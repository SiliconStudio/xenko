// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for mouse devices, implements some common functionality of <see cref="IMouseDevice"/>, inherits from <see cref="PointerDeviceBase"/>
    /// </summary>
    public abstract class MouseDeviceBase : PointerDeviceBase, IMouseDevice
    {
        public readonly HashSet<MouseButton> DownButtons = new HashSet<MouseButton>();
        protected readonly List<MouseInputEvent> MouseInputEvents = new List<MouseInputEvent>();
        
        public EventHandler<MouseButtonEvent> OnMouseButton { get; set; }
        
        public EventHandler<MouseWheelEvent> OnMouseWheel { get; set; }
        
        public abstract bool IsMousePositionLocked { get; }
        
        public override PointerType Type => PointerType.Mouse;
        
        public override void Update(List<InputEvent> inputEvents)
        {
            base.Update(inputEvents);

            // Fire events
            foreach (var evt in MouseInputEvents)
            {
                if (evt.Type == MouseInputEventType.Down)
                {
                    var buttonEvent = new MouseButtonEvent(this) { State = ButtonState.Pressed, Button = evt.Button };
                    OnMouseButton?.Invoke(this, buttonEvent);
                    inputEvents.Add(buttonEvent);
                }
                else if (evt.Type == MouseInputEventType.Up)
                {
                    var buttonEvent = new MouseButtonEvent(this) { State = ButtonState.Released, Button = evt.Button };
                    OnMouseButton?.Invoke(this, buttonEvent);
                    inputEvents.Add(buttonEvent);
                }
                else if (evt.Type == MouseInputEventType.Scroll)
                {
                    var wheelEvent = new MouseWheelEvent(this) { WheelDelta = evt.WheelDelta };
                    OnMouseWheel?.Invoke(this, wheelEvent);
                    inputEvents.Add(wheelEvent);
                }
            }
            MouseInputEvents.Clear();
        }
        
        public virtual bool IsMouseButtonDown(MouseButton button)
        {
            return DownButtons.Contains(button);
        }
        
        public abstract void SetMousePosition(Vector2 normalizedPosition);
        
        public abstract void LockMousePosition(bool forceCenter = false);
        
        public abstract void UnlockMousePosition();

        public void HandleButtonDown(MouseButton button)
        {
            // Prevent duplicate events
            if (DownButtons.Contains(button))
                return;

            DownButtons.Add(button);
            MouseInputEvents.Add(new MouseInputEvent { Button = button, Type = MouseInputEventType.Down });

            // Simulate tap on primary mouse button
            if (button == MouseButton.Left)
                HandlePointerDown();
        }

        public void HandleButtonUp(MouseButton button)
        {
            // Prevent duplicate events
            if (!DownButtons.Contains(button))
                return;

            DownButtons.Remove(button);
            MouseInputEvents.Add(new MouseInputEvent { Button = button, Type = MouseInputEventType.Up });

            // Simulate tap on primary mouse button
            if (button == MouseButton.Left)
                HandlePointerUp();
        }

        public void HandleMouseWheel(float wheelDelta)
        {
            MouseInputEvents.Add(new MouseInputEvent { Type = MouseInputEventType.Scroll, WheelDelta = wheelDelta });
        }

        protected struct MouseInputEvent
        {
            public MouseButton Button;
            public MouseInputEventType Type;
            public float WheelDelta;
        }

        protected enum MouseInputEventType
        {
            Up,
            Down,
            Scroll,
        }
    }
}