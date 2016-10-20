using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public enum ScrollWheelDirection
    {
        Vertical,
        Horizontal,
    }

    public enum MouseButtonState
    {
        Pressed,
        Released
    }

    public class MouseButtonEvent : EventArgs
    {
        public MouseButton Button;
        public MouseButtonState State;
    }

    public class MouseWheelEvent : EventArgs
    {
        public ScrollWheelDirection Direction;
        public int WheelDelta;
    }

    public interface IMouseDevice
    {
        /// <summary>
        /// Raised when a mouse/pen button is pressed/released
        /// </summary>
        EventHandler<MouseButtonEvent> OnMouseButton { get; set; }

        /// <summary>
        /// Raised when a scroll wheel is used
        /// </summary>
        EventHandler<MouseWheelEvent> OnMouseWheel { get; set; }

        /// <summary>
        /// Gets or sets if the mouse is locked to the screen
        /// </summary>
        bool IsMousePositionLocked { get; }

        /// <summary>
        /// Locks the mouse position to the screen
        /// </summary>
        /// <param name="forceCenter">Force the mouse position to the center of the screen</param>
        void LockMousePosition(bool forceCenter = false);

        /// <summary>
        /// Unlocks the mouse position if it was locked
        /// </summary>
        void UnlockMousePosition();

        /// <summary>
        /// Determines whether the specified button is being pressed down
        /// </summary>
        /// <param name="button">The button</param>
        /// <returns><c>true</c> if the specified button is being pressed down; otherwise, <c>false</c>.</returns>
        bool IsMouseButtonDown(MouseButton button);

        /// <summary>
        /// Attempts to set the pointer position, this only makes sense for mouse pointers
        /// </summary>
        /// <param name="absolutePosition">The desired position</param>
        void SetMousePosition(Vector2 absolutePosition);
    }

    public abstract class MouseDeviceBase : PointerDeviceBase, IMouseDevice
    {
        public EventHandler<MouseButtonEvent> OnMouseButton { get; set; }
        public EventHandler<MouseWheelEvent> OnMouseWheel { get; set; }
        public abstract bool IsMousePositionLocked { get; }
        public override PointerType Type => PointerType.Mouse;

        public readonly HashSet<MouseButton> DownButtons = new HashSet<MouseButton>();
        public readonly HashSet<MouseButton> PressedButtons = new HashSet<MouseButton>();
        public readonly HashSet<MouseButton> ReleasedButtons = new HashSet<MouseButton>();

        private readonly List<MouseInputEvent> mouseInputEvents = new List<MouseInputEvent>();

        public override void Update()
        {
            base.Update();

            // Clear collection of pressed/released buttons
            PressedButtons.Clear();
            ReleasedButtons.Clear();

            // Fire events
            foreach (var evt in mouseInputEvents)
            {
                if (evt.Type == InputEventType.Down)
                {
                    PressedButtons.Add(evt.Button);
                    OnMouseButton?.Invoke(this, new MouseButtonEvent { Button = evt.Button, State = MouseButtonState.Pressed });
                }
                else if (evt.Type == InputEventType.Up)
                {
                    ReleasedButtons.Add(evt.Button);
                    OnMouseButton?.Invoke(this, new MouseButtonEvent { Button = evt.Button, State = MouseButtonState.Released });
                }
                else if (evt.Type == InputEventType.Scroll)
                {
                    OnMouseWheel?.Invoke(this, new MouseWheelEvent { Direction = evt.ScrollDirection, WheelDelta = evt.WheelDelta });
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
            mouseInputEvents.Add(new MouseInputEvent { Button = button, Type = InputEventType.Down });

            // Simulate tap on primary mouse button
            if(button == MouseButton.Left)
                HandlePointerDown();
        }

        public void HandleButtonUp(MouseButton button)
        {
            DownButtons.Remove(button);
            mouseInputEvents.Add(new MouseInputEvent { Button = button, Type = InputEventType.Up });

            // Simulate tap on primary mouse button
            if (button == MouseButton.Left)
                HandlePointerUp();
        }

        public void HandleMouseWheel(ScrollWheelDirection direction, int wheelDelta)
        {
            mouseInputEvents.Add(new MouseInputEvent { Type = InputEventType.Scroll, ScrollDirection = direction, WheelDelta = wheelDelta });
        }

        public abstract void LockMousePosition(bool forceCenter = false);
        public abstract void UnlockMousePosition();

        protected struct MouseInputEvent
        {
            public MouseButton Button;
            public InputEventType Type;
            public int WheelDelta;
            public ScrollWheelDirection ScrollDirection;
        }

        protected enum InputEventType
        {
            Up,
            Down,
            Move,
            Scroll,
        }
    }
}