// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Runtime.InteropServices;
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Games;
using Vector2 = SiliconStudio.Core.Mathematics.Vector2;

using WinFormsKeys = System.Windows.Forms.Keys;

namespace SiliconStudio.Paradox.Input
{
    public partial class InputManager
    {
        private Control uiControl;
        private readonly Stopwatch pointerClock;

        public static bool UseRawInput = true;

        public InputManager(IServiceRegistry registry)
            : base(registry)
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

        private System.Drawing.Point capturedPosition;
        private bool wasMouseVisibleBeforeCapture;

        private IntPtr defaultWndProc;
        private Win32Native.WndProc inputWndProc;

        public override void LockMousePosition()
        {
            if (!IsMousePositionLocked)
            {
                wasMouseVisibleBeforeCapture = Game.IsMouseVisible;
                Game.IsMouseVisible = false;
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

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            WinFormsKeys virtualKey;
            switch (msg)
            {
                case Win32Native.WM_KEYDOWN:
                case Win32Native.WM_SYSKEYDOWN:
                    virtualKey = (WinFormsKeys)wParam.ToInt64();
                    virtualKey = GetCorrectExtendedKey(virtualKey, lParam.ToInt64());
                    OnKeyEvent(virtualKey, false);
                    break;
                case Win32Native.WM_KEYUP:
                case Win32Native.WM_SYSKEYUP:
                    virtualKey = (WinFormsKeys)wParam.ToInt64();
                    virtualKey = GetCorrectExtendedKey(virtualKey, lParam.ToInt64());
                    OnKeyEvent(virtualKey, true);
                    break;
            }

            var result = Win32Native.CallWindowProc(defaultWndProc, hWnd, msg, wParam, lParam);
            return result;
        }

        private static WinFormsKeys GetCorrectExtendedKey(WinFormsKeys virtualKey, long lParam)
        {
            if (virtualKey == WinFormsKeys.ControlKey)
            {
                // We check if the key is an extended key. Extended keys are R-keys, non-extended are L-keys.
                return (lParam & 0x01000000) == 0 ? WinFormsKeys.LControlKey : WinFormsKeys.RControlKey;
            }
            if (virtualKey == WinFormsKeys.ShiftKey)
            {
                // We need to check the scan code to check which SHIFT key it is.
                var scanCode = (lParam & 0x00FF0000) >> 16;
                return (scanCode != 36) ? WinFormsKeys.LShiftKey : WinFormsKeys.RShiftKey;
            }
            if (virtualKey == WinFormsKeys.Menu)
            {
                // We check if the key is an extended key. Extended keys are R-keys, non-extended are L-keys.
                return (lParam & 0x01000000) == 0 ? WinFormsKeys.LMenu : WinFormsKeys.RMenu;
            }
            return virtualKey;
        }

        private void InitializeFromWindowsForms(GameContext uiContext)
        {
            uiControl = (Control) uiContext.Control;

            pointerClock.Restart();

            if (UseRawInput)
            {
                BindRawInputKeyboard(uiControl);
            }
            else
            {
                EnsureMapKeys();
                defaultWndProc = Win32Native.GetWindowLong(new HandleRef(this, uiControl.Handle), Win32Native.WindowLongType.WndProc);
                // This is needed to prevent garbage collection of the delegate.
                inputWndProc = WndProc;
                var inputWndProcPtr = Marshal.GetFunctionPointerForDelegate(inputWndProc);
                Win32Native.SetWindowLong(new HandleRef(this, uiControl.Handle), Win32Native.WindowLongType.WndProc, inputWndProcPtr);
            }
            uiControl.GotFocus += (_, e) => OnUiControlGotFocus();
            uiControl.LostFocus += (_, e) => OnUiControlLostFocus();
            uiControl.MouseMove += (_, e) => OnMouseMoveEvent(new Vector2(e.X, e.Y));
            uiControl.MouseDown += (_, e) => { uiControl.Focus(); OnMouseInputEvent(new Vector2(e.X, e.Y), ConvertMouseButton(e.Button), InputEventType.Down); };
            uiControl.MouseUp += (_, e) => OnMouseInputEvent(new Vector2(e.X, e.Y), ConvertMouseButton(e.Button), InputEventType.Up);
            uiControl.MouseWheel += (_, e) => OnMouseInputEvent(new Vector2(e.X, e.Y), MouseButton.Middle, InputEventType.Wheel, e.Delta);
            uiControl.MouseCaptureChanged += (_, e) => OnLostMouseCaptureWinForms();
            uiControl.SizeChanged += UiControlOnSizeChanged;

            ControlWidth = uiControl.ClientSize.Width;
            ControlHeight = uiControl.ClientSize.Height;
        }

        private void OnKeyEvent(WinFormsKeys keyCode, bool isKeyUp)
        {
            Keys key;
            if (mapKeys.TryGetValue(keyCode, out key) && key != Keys.None)
            {
                var type = isKeyUp ? InputEventType.Up : InputEventType.Down;
                lock (KeyboardInputEvents)
                {
                    KeyboardInputEvents.Add(new KeyboardInputEvent { Key = key, Type = type });
                }
            }
        }

        private void InitializeFromWindowsWpf(GameContext uiContext)
        {
            var uiControlWpf = (Window)uiContext.Control;

            var inputElement = uiControlWpf;

            BindRawInputKeyboard(uiControl);
            uiControlWpf.LostFocus += (_, e) => OnUiControlLostFocus();
            uiControlWpf.Deactivated += (_, e) => OnUiControlLostFocus();
            uiControlWpf.MouseMove += (_, e) => OnMouseMoveEvent(PointToVector2(e.GetPosition(inputElement)));
            uiControlWpf.MouseDown += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(inputElement)), ConvertMouseButton(e.ChangedButton), InputEventType.Down);
            uiControlWpf.MouseUp += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(inputElement)), ConvertMouseButton(e.ChangedButton), InputEventType.Up);
            uiControlWpf.MouseWheel += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(inputElement)), MouseButton.Middle, InputEventType.Wheel, e.Delta);
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
            if (type == InputEventType.Wheel && !uiControl.ClientRectangle.Contains(uiControl.PointToClient(Control.MousePosition)))
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
            var previousMousePosition = CurrentMousePosition;
            CurrentMousePosition = NormalizeScreenPosition(pixelPosition);
            // Discard this event if it has been triggered by the replacing the cursor to its capture initial position
            if (IsMousePositionLocked && Cursor.Position == capturedPosition)
                return;

            CurrentMouseDelta = CurrentMousePosition - previousMousePosition;
            
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

        private void OnLostMouseCaptureWinForms()
        {
            // On windows forms, the controls capture of the mouse button events at the first button pressed and release them at the first button released.
            // This has for consequence that all up-events of button simultaneously pressed are lost after the release of first button (if outside of the window).
            // This function fix the problem by forcing the mouse event capture if any mouse buttons are still down at the first button release.
 
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                var buttonId = (int)button;
                if (MouseButtonCurrentlyDown[buttonId])
                    uiControl.Capture = true;
            }
        }

        private void OnUiControlGotFocus()
        {
            lock (KeyboardInputEvents)
            {
                foreach (var key in mapKeys)
                {
                    var state = Win32Native.GetKeyState((int)key.Key);
                    if ((state & 0x8000) == 0x8000)
                        KeyboardInputEvents.Add(new KeyboardInputEvent { Key = key.Value, Type = InputEventType.Down, OutOfFocus = true });
                }
            }
        }

        private void OnUiControlLostFocus()
        {
            LostFocus = true;
        }

        private static MouseButton ConvertMouseButton(MouseButtons mouseButton)
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