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
        public EventHandler<MouseButtonEvent> OnMouseButton { get; set; }
        public EventHandler<MouseWheelEvent> OnMouseWheel { get; set; }
        public abstract bool IsMousePositionLocked { get; }
        public override PointerType Type => PointerType.Mouse;

        public readonly HashSet<MouseButton> DownButtons = new HashSet<MouseButton>();

        private readonly List<MouseInputEvent> mouseInputEvents = new List<MouseInputEvent>();

        public override void Update()
        {
            base.Update();

            // Fire events
            foreach (var evt in mouseInputEvents)
            {
                if (evt.Type == MouseInputEventType.Down)
                {
                    OnMouseButton?.Invoke(this, new MouseButtonEvent { Button = evt.Button, State = MouseButtonState.Pressed });
                }
                else if (evt.Type == MouseInputEventType.Up)
                {
                    OnMouseButton?.Invoke(this, new MouseButtonEvent { Button = evt.Button, State = MouseButtonState.Released });
                }
                else if (evt.Type == MouseInputEventType.Scroll)
                {
                    OnMouseWheel?.Invoke(this, new MouseWheelEvent { WheelDelta = evt.WheelDelta });
                }
            }
            mouseInputEvents.Clear();
        }

        public virtual bool IsMouseButtonDown(MouseButton button)
        {
            return DownButtons.Contains(button);
        }

        public abstract void SetMousePosition(Vector2 absolutePosition);

        public void HandleButtonDown(MouseButton button)
        {
            DownButtons.Add(button);
            mouseInputEvents.Add(new MouseInputEvent { Button = button, Type = MouseInputEventType.Down });

            // Simulate tap on primary mouse button
            if(button == MouseButton.Left)
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

        public abstract void LockMousePosition(bool forceCenter = false);
        public abstract void UnlockMousePosition();

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