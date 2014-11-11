// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Games;
using Vector2 = SiliconStudio.Core.Mathematics.Vector2;

namespace SiliconStudio.Paradox.Input
{
    public partial class InputManager
    {
        private Control uiControl;
        private Stopwatch pointerClock;

        public InputManager(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = true;
            HasPointer = true;

            pointerClock = new Stopwatch();

            GamePadFactories.Add(new XInputGamePadFactory());
            GamePadFactories.Add(new DirectInputGamePadFactory());
        }

        public override void Initialize()
        {
            base.Initialize();

            switch (Game.Context.ContextType)
            {
                case AppContextType.Desktop:
                    InitializeFromWindowsForms(Game.Context);
                    break;
                case AppContextType.DesktopWpf:
                    InitializeFromWindowsWpf(Game.Context);
                    break;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL
                case AppContextType.DesktopOpenTK:
                    InitializeFromOpenTK(Game.Context);
                    break;
#endif
                default:
                    throw new ArgumentException(string.Format("WindowContext [{0}] not supported", Game.Context.ContextType));
            }

            // Scan all registered inputs
            Scan();
        }

        private void InitializeFromWindowsForms(GameContext uiContext)
        {
            uiControl = (Control) uiContext.Control;

            pointerClock.Restart();

            BindRawInputKeyboard(uiControl);

            uiControl.MouseEnter += (_, e) => uiControl.Focus();
            uiControl.MouseMove += (_, e) => OnMouseMoveEvent(new Vector2(e.X, e.Y));
            uiControl.MouseDown += (_, e) => OnMouseInputEvent(new Vector2(e.X, e.Y), ConvertMouseButton(e.Button), InputEventType.Down);
            uiControl.MouseUp += (_, e) => OnMouseInputEvent(new Vector2(e.X, e.Y), ConvertMouseButton(e.Button), InputEventType.Up);
            uiControl.MouseWheel += (_, e) => OnMouseInputEvent(new Vector2(e.X, e.Y), MouseButton.Middle, InputEventType.Wheel, e.Delta);
            uiControl.MouseLeave += (_, e) => OnMouseLeaveEvent();
            uiControl.SizeChanged += UiControlOnSizeChanged;

            ControlWidth = uiControl.ClientSize.Width;
            ControlHeight = uiControl.ClientSize.Height;
        }

        private void InitializeFromWindowsWpf(GameContext uiContext)
        {
            var uiControlWpf = (Window)uiContext.Control;

            var inputElement = uiControlWpf;

            BindRawInputKeyboard(uiControl);
            uiControlWpf.Deactivated += OnUiControlLostFocus;
            uiControlWpf.MouseMove += (_, e) => OnMouseMoveEvent(PointToVector2(e.GetPosition(inputElement)));
            uiControlWpf.MouseDown += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(inputElement)), ConvertMouseButton(e.ChangedButton), InputEventType.Down);
            uiControlWpf.MouseUp += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(inputElement)), ConvertMouseButton(e.ChangedButton), InputEventType.Up);
            uiControlWpf.MouseWheel += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(inputElement)), MouseButton.Middle, InputEventType.Wheel, e.Delta);
            uiControlWpf.MouseLeave += (_, e) => OnMouseLeaveEvent();
            uiControlWpf.SizeChanged += OnWpfSizeChanged;

            ControlWidth = (float)uiControlWpf.ActualWidth;
            ControlHeight = (float)uiControlWpf.ActualHeight;
        }

        private void UiControlOnSizeChanged(object sender, EventArgs eventArgs)
        {
            ControlWidth = uiControl.ClientSize.Width;
            ControlHeight = uiControl.ClientSize.Height;
        }

        private void OnWpfSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ControlWidth = (float)e.NewSize.Width;
            ControlHeight = (float)e.NewSize.Height;
        }
        
        private void OnMouseInputEvent(Vector2 pixelPosition, MouseButton button, InputEventType type, float value = 0)
        {
            // The mouse wheel event are still received even when the mouse cursor is out of the control boundaries. Discard the event in this case.
            if (!uiControl.ClientRectangle.Contains(uiControl.PointToClient(Control.MousePosition)))
                return;

            // the mouse events series has been interrupted because out of the window.
            if (type == InputEventType.Up && !MouseButtonCurrentlyDown[(int)button])
                return;

            CurrentMousePosition = NormalizeScreenPosition(pixelPosition);

            var mouseInputEvent = new MouseInputEvent { Type = type, MouseButton = button, Value = value};
            lock (MouseInputEvents)
                MouseInputEvents.Add(mouseInputEvent);

            if (type != InputEventType.Wheel)
            {
                var buttonId = (int)button;
                MouseButtonCurrentlyDown[buttonId] = type == InputEventType.Down;
                HandlePointerEvents(buttonId, CurrentMousePosition, InputEventTypeToPointerState(type), PointerType.Mouse);
            }
        }

        private void OnMouseMoveEvent(Vector2 pixelPosition)
        {
            CurrentMousePosition = NormalizeScreenPosition(pixelPosition);

            // trigger touch move events
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                var buttonId = (int)button;
                if (MouseButtonCurrentlyDown[buttonId])
                    HandlePointerEvents(buttonId, CurrentMousePosition, PointerState.Move, PointerType.Mouse);
            }
        }

        private void OnMouseLeaveEvent()
        {
            if (HasDownMouseButtons())
                LostFocus = true;

            // trigger touch leave events
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                var buttonId = (int)button;
                if (MouseButtonCurrentlyDown[buttonId])
                {
                    HandlePointerEvents(buttonId, PointerInfos[buttonId].LastPosition, PointerState.Out, PointerType.Mouse);

                    MouseButtonCurrentlyDown[buttonId] = false;
                }
            }
        }
        
        private void OnUiControlLostFocus(object sender, EventArgs e)
        {
            LostFocus = true;
        }

        private MouseButton ConvertMouseButton(MouseButtons mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButtons.Left:
                    return MouseButton.Left;
                case MouseButtons.Right:
                    return MouseButton.Right;
                case MouseButtons.Middle:
                    return MouseButton.Middle;
                case MouseButtons.XButton1:
                    return MouseButton.Extended1;
                case MouseButtons.XButton2:
                    return MouseButton.Extended2;
            }
            return (MouseButton)(-1);
        }

        private MouseButton ConvertMouseButton(System.Windows.Input.MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    return MouseButton.Left;
                case System.Windows.Input.MouseButton.Right:
                    return MouseButton.Right;
                case System.Windows.Input.MouseButton.Middle:
                    return MouseButton.Middle;
                case System.Windows.Input.MouseButton.XButton1:
                    return MouseButton.Extended1;
                case System.Windows.Input.MouseButton.XButton2:
                    return MouseButton.Extended2;
            }
            return (MouseButton)(-1);
        }

        private PointerState InputEventTypeToPointerState(InputEventType type)
        {
            switch (type)
            {
                case InputEventType.Up:
                    return PointerState.Up;
                case InputEventType.Down:
                    return PointerState.Down;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
        
        private Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        // There is no multi-touch on windows, so there is nothing specific to do.
        public override bool MultiTouchEnabled { get; set; }
    }
}
#endif