// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for mouse devices, implements some common functionality of <see cref="IMouseDevice"/>, inherits from <see cref="PointerDeviceBase"/>
    /// </summary>
    public abstract class MouseDeviceBase : PointerDeviceBase, IMouseDevice
    {
        public readonly HashSet<MouseButton> DownButtons = new HashSet<MouseButton>();
        protected readonly List<MouseInputEvent> mouseInputEvents = new List<MouseInputEvent>();

        /// <inheritdoc />
        public EventHandler<MouseButtonEvent> OnMouseButton { get; set; }

        /// <inheritdoc />
        public EventHandler<MouseWheelEvent> OnMouseWheel { get; set; }

        /// <inheritdoc />
        public abstract bool IsMousePositionLocked { get; }

        /// <inheritdoc />
        public override PointerType Type => PointerType.Mouse;

        /// <inheritdoc />
        public override void Update(List<InputEvent> inputEvents)
        {
            base.Update(inputEvents);

            // Fire events
            foreach (var evt in mouseInputEvents)
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
            mouseInputEvents.Clear();
        }

        /// <inheritdoc />
        public virtual bool IsMouseButtonDown(MouseButton button)
        {
            return DownButtons.Contains(button);
        }

        /// <inheritdoc />
        public abstract void SetMousePosition(Vector2 normalizedPosition);

        /// <inheritdoc />
        public abstract void LockMousePosition(bool forceCenter = false);

        /// <inheritdoc />
        public abstract void UnlockMousePosition();

        public void HandleButtonDown(MouseButton button)
        {
            DownButtons.Add(button);
            mouseInputEvents.Add(new MouseInputEvent { Button = button, Type = MouseInputEventType.Down });

            // Simulate tap on primary mouse button
            if (button == MouseButton.Left)
                HandlePointerDown();
        }

        public void HandleButtonUp(MouseButton button)
        {
            DownButtons.Remove(button);
            mouseInputEvents.Add(new MouseInputEvent { Button = button, Type = MouseInputEventType.Up });

            // Simulate tap on primary mouse button
            if (button == MouseButton.Left)
                HandlePointerUp();
        }

        public void HandleMouseWheel(int wheelDelta)
        {
            mouseInputEvents.Add(new MouseInputEvent { Type = MouseInputEventType.Scroll, WheelDelta = wheelDelta });
        }

        protected struct MouseInputEvent
        {
            public MouseButton Button;
            public MouseInputEventType Type;
            public int WheelDelta;
        }

        protected enum MouseInputEventType
        {
            Up,
            Down,
            Scroll,
        }
    }
}