// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_UI_WPF && !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;
using Vector2 = SiliconStudio.Core.Mathematics.Vector2;

using WinFormsKeys = System.Windows.Forms.Keys;

namespace SiliconStudio.Xenko.Input
{
    internal class InputManagerWpf : InputManagerWindows<Window>
    {
        public InputManagerWpf(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = true;
            HasPointer = true;

            GamePadFactories.Add(new XInputGamePadFactory());
            GamePadFactories.Add(new DirectInputGamePadFactory());
        }

        public override void Initialize(GameContext<Window> context)
        {
            switch (context.ContextType)
            {
                case AppContextType.DesktopWpf:
                    InitializeFromWindowsWpf(context);
                    break;
                default:
                    throw new ArgumentException(string.Format("WindowContext [{0}] not supported", Game.Context.ContextType));
            }

            // Scan all registered inputs
            Scan();
        }

        private System.Drawing.Point capturedPosition;
        private bool wasMouseVisibleBeforeCapture;

        public override void LockMousePosition(bool forceCenter = false)
        {
            if (!IsMousePositionLocked)
            {
                wasMouseVisibleBeforeCapture = Game.IsMouseVisible;
                Game.IsMouseVisible = false;
                if (forceCenter)
                {
                    SetMousePosition(new Vector2(0.5f, 0.5f));
                }
                capturedPosition = Cursor.Position;
                IsMousePositionLocked = true;
            }
        }

        public override void UnlockMousePosition()
        {
            if (IsMousePositionLocked)
            {
                IsMousePositionLocked = false;
                capturedPosition = System.Drawing.Point.Empty;
                Game.IsMouseVisible = wasMouseVisibleBeforeCapture;
            }
        }

        protected override void SetMousePosition(Vector2 normalizedPosition)
        {
            var newPos = UiControl.PointToScreen(
                new System.Windows.Point((int)(ClientWidth(UiControl)*normalizedPosition.X), (int)(ClientHeight(UiControl)*normalizedPosition.Y)));
            Cursor.Position = new System.Drawing.Point((int)newPos.X, (int)newPos.Y);
        }

        private void InitializeFromWindowsWpf(GameContext<Window> uiContext)
        {
            UiControl = uiContext.Control;

            BindRawInputKeyboard(UiControl);
            UiControl.LostFocus += (_, e) => OnUiControlLostFocus();
            UiControl.Deactivated += (_, e) => OnUiControlLostFocus();
            UiControl.MouseMove += (_, e) => OnMouseMoveEvent(PointToVector2(e.GetPosition(UiControl)));
            UiControl.MouseDown += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(UiControl)), ConvertMouseButton(e.ChangedButton), InputEventType.Down);
            UiControl.MouseUp += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(UiControl)), ConvertMouseButton(e.ChangedButton), InputEventType.Up);
            UiControl.MouseWheel += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(UiControl)), MouseButton.Middle, InputEventType.Wheel, e.Delta);
            UiControl.SizeChanged += OnWpfSizeChanged;

            ControlWidth = (float)UiControl.ActualWidth;
            ControlHeight = (float)UiControl.ActualHeight;
        }

        private static int ClientWidth(Window ctrl)
        {
            var content = ctrl.Content as FrameworkElement;
            if (content != null)
            {
                return (int) content.ActualWidth;
            }
            else
            {
                // Control has no content, we can only estimate the window size.
                // Should we use a Pinvoke call here to get the info?
                return (int) ctrl.ActualWidth;
            }
        }

        private static int ClientHeight(Window ctrl)
        {
            var content = ctrl.Content as FrameworkElement;
            if (content != null)
            {
                return (int) content.ActualHeight;
            }
            else
            {
                // Control has no content, we can only estimate the window size.
                // Should we use a Pinvoke call here to get the info?
                return (int) ctrl.ActualHeight;
            }
        }

        private void OnWpfSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ControlWidth = (float)e.NewSize.Width;
            ControlHeight = (float)e.NewSize.Height;
        }

        private void OnMouseInputEvent(Vector2 pixelPosition, MouseButton button, InputEventType type, float value = 0)
        {
            // The mouse wheel event are still received even when the mouse cursor is out of the control boundaries. Discard the event in this case.
            if (type == InputEventType.Wheel && !UiControl.IsMouseOver)
                return;

            // the mouse events series has been interrupted because out of the window.
            if (type == InputEventType.Up && !MouseButtonCurrentlyDown[(int)button])
                return;

            CurrentMousePosition = NormalizeScreenPosition(pixelPosition);

            var mouseInputEvent = new MouseInputEvent { Type = type, MouseButton = button, Value = value };
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
            var previousMousePosition = CurrentMousePosition;
            CurrentMousePosition = NormalizeScreenPosition(pixelPosition);
            // Discard this event if it has been triggered by the replacing the cursor to its capture initial position
            if (IsMousePositionLocked && Cursor.Position == capturedPosition)
                return;

            CurrentMouseDelta += CurrentMousePosition - previousMousePosition;

            // trigger touch move events
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                var buttonId = (int)button;
                if (MouseButtonCurrentlyDown[buttonId])
                    HandlePointerEvents(buttonId, CurrentMousePosition, PointerState.Move, PointerType.Mouse);
            }

            if (IsMousePositionLocked)
            {
                Cursor.Position = capturedPosition;
            }
        }

        private void OnUiControlLostFocus()
        {
            LostFocus = true;
        }

        private static MouseButton ConvertMouseButton(System.Windows.Input.MouseButton mouseButton)
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

        private static PointerState InputEventTypeToPointerState(InputEventType type)
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

        private static Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        // There is no multi-touch on windows, so there is nothing specific to do.
        public override bool MultiTouchEnabled { get; set; }
    }
}
#endif
