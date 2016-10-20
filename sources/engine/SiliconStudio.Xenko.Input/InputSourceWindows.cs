// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;
using SharpDX.Multimedia;
using SharpDX.RawInput;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using Point = System.Drawing.Point;
using WinFormsKeys = System.Windows.Forms.Keys;

namespace SiliconStudio.Xenko.Input
{
    public class KeyboardWinforms : KeyboardDeviceBase
    {
        public override string DeviceName => "Windows Keyboard";

        internal void HandleKeyDown(WinFormsKeys winFormsKey)
        {
            // Translate from windows key enum to Xenko key enum
            Keys xenkoKey;
            if (WinKeys.mapKeys.TryGetValue(winFormsKey, out xenkoKey) && xenkoKey != Keys.None)
            {
                HandleKeyDown(xenkoKey);
            }
        }
        internal void HandleKeyUp(WinFormsKeys winFormsKey)
        {
            // Translate from windows key enum to Xenko key enum
            Keys xenkoKey;
            if (WinKeys.mapKeys.TryGetValue(winFormsKey, out xenkoKey) && xenkoKey != Keys.None)
            {
                HandleKeyUp(xenkoKey);
            }
        }
    }

    public class MouseWinforms : MouseDeviceBase
    {
        public override string DeviceName => "Windows Mouse";
        public override bool IsMousePositionLocked => isMousePositionLocked;
        public override PointerType Type => PointerType.Mouse;
        public override Vector2 SurfaceSize => surfaceSize;

        private Vector2 surfaceSize;
        private readonly GameBase game;
        private readonly Control uiControl;
        private bool isMousePositionLocked;
        private bool wasMouseVisibleBeforeCapture;
        private Point capturedPosition;

        public MouseWinforms(GameBase game, Control uiControl)
        {
            this.game = game;
            this.uiControl = uiControl;
            surfaceSize = new Vector2(uiControl.ClientSize.Width, uiControl.ClientSize.Height);

            uiControl.GotFocus += (_, e) => OnUiControlGotFocus();
            uiControl.LostFocus += (_, e) => OnUiControlLostFocus();
            uiControl.MouseMove += (_, e) => HandleMove(new Vector2(e.X, e.Y));
            uiControl.MouseDown += (_, e) =>
            {
                uiControl.Focus();
                HandleButtonDown(ConvertMouseButton(e.Button));
            };
            uiControl.MouseUp += (_, e) => HandleButtonUp(ConvertMouseButton(e.Button));
            uiControl.MouseWheel += (_, e) => HandleMouseWheel(ScrollWheelDirection.Vertical, e.Delta);
            uiControl.MouseCaptureChanged += (_, e) => OnLostMouseCaptureWinForms();
            uiControl.SizeChanged += UiControlOnSizeChanged;
        }

        public override void Dispose()
        {
            base.Dispose();
            uiControl.SizeChanged -= UiControlOnSizeChanged;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void SetMousePosition(Vector2 absolutePosition)
        {
            var newPos = uiControl.PointToScreen(new System.Drawing.Point((int)absolutePosition.X, (int)absolutePosition.Y));
            Cursor.Position = newPos;
        }

        public override void LockMousePosition(bool forceCenter = false)
        {
            if (!isMousePositionLocked)
            {
                wasMouseVisibleBeforeCapture = game.IsMouseVisible;
                game.IsMouseVisible = false;
                if (forceCenter)
                {
                    SetMousePosition(new Vector2(0.5f, 0.5f));
                }
                capturedPosition = Cursor.Position;
                isMousePositionLocked = true;
            }
        }

        public override void UnlockMousePosition()
        {
            if (isMousePositionLocked)
            {
                isMousePositionLocked = false;
                capturedPosition = System.Drawing.Point.Empty;
                game.IsMouseVisible = wasMouseVisibleBeforeCapture;
            }
        }

        private void UiControlOnSizeChanged(object sender, EventArgs eventArgs)
        {
            surfaceSize = new Vector2(uiControl.ClientSize.Width, uiControl.ClientSize.Height);
        }

        private void OnLostMouseCaptureWinForms()
        {
            // On windows forms, the controls capture of the mouse button events at the first button pressed and release them at the first button released.
            // This has for consequence that all up-events of button simultaneously pressed are lost after the release of first button (if outside of the window).
            // This function fix the problem by forcing the mouse event capture if any mouse buttons are still down at the first button release.

            //            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            //            {
            //                var buttonId = (int)button;
            //                if (MouseButtonCurrentlyDown[buttonId])
            //                    UiControl.Capture = true;
            //            }
        }

        private void OnUiControlGotFocus()
        {
            //lock (KeyboardInputEvents)
            //{
            //    foreach (var key in WinKeys.mapKeys)
            //    {
            //        var state = Win32Native.GetKeyState((int)key.Key);
            //        if ((state & 0x8000) == 0x8000)
            //            KeyboardInputEvents.Add(new KeyboardWinforms.KeyboardInputEvent { Key = key.Value, Type = KeyboardWinforms.InputEventType.Down, OutOfFocus = true });
            //    }
            //}
            //LostFocus = false;
        }

        private void OnUiControlLostFocus()
        {
            //LostFocus = true;
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
    }

    public class InputSourceWindows : InputSourceBase
    {
        /// <summary>
        /// Gets the value indicating if the mouse position is currently locked or not.
        /// </summary>
        public bool IsMousePositionLocked { get; protected set; }

        private KeyboardWinforms keyboard;
        private MouseWinforms mouse;

        private System.Drawing.Point capturedPosition;
        private bool wasMouseVisibleBeforeCapture;

        private IntPtr defaultWndProc;
        private Win32Native.WndProc inputWndProc;

        private HashSet<WinFormsKeys> heldKeys = new HashSet<WinFormsKeys>();

        // My input devices
        private GameContext<Control> gameContext;
        private GameBase game;
        private Control uiControl;

        public override void Initialize(InputManager inputManager)
        {
            gameContext = inputManager.Game.Context as GameContext<Control>;
            game = inputManager.Game;
            uiControl = gameContext.Control;

            // Do not register keyboard devices when using raw input instead
            if (!InputManager.UseRawInput)
            {
                keyboard = new KeyboardWinforms();

                defaultWndProc = Win32Native.GetWindowLong(uiControl.Handle, Win32Native.WindowLongType.WndProc);
                // This is needed to prevent garbage collection of the delegate.
                inputWndProc = WndProc;
                var inputWndProcPtr = Marshal.GetFunctionPointerForDelegate(inputWndProc);
                Win32Native.SetWindowLong(uiControl.Handle, Win32Native.WindowLongType.WndProc, inputWndProcPtr);

                RegisterDevice(keyboard);
            }

            mouse = new MouseWinforms(game, uiControl);
            RegisterDevice(mouse);
        }

        public override void Update()
        {
            // Manually force releasing keys if their up event never gets fired
            WinFormsKeys[] keysToCheck = heldKeys.ToArray();
            foreach (WinFormsKeys key in keysToCheck)
            {
                short keyState = Win32Native.GetKeyState((int)key);
                if ((keyState & 0x8000) == 0)
                {
                    keyboard.HandleKeyUp(key);
                    heldKeys.Remove(key);
                }
            }
        }

        public override void Dispose()
        {
            // Unregisters devices
            base.Dispose();

            mouse?.Dispose();
            keyboard?.Dispose();
        }

        public void LockMousePosition(bool forceCenter = false)
        {
            if (!IsMousePositionLocked)
            {
                wasMouseVisibleBeforeCapture = game.IsMouseVisible;
                game.IsMouseVisible = false;
                if (forceCenter)
                {
                    SetMousePosition(new Vector2(0.5f, 0.5f));
                }
                capturedPosition = Cursor.Position;
                IsMousePositionLocked = true;
            }
        }

        public void UnlockMousePosition()
        {
            if (IsMousePositionLocked)
            {
                IsMousePositionLocked = false;
                capturedPosition = System.Drawing.Point.Empty;
                game.IsMouseVisible = wasMouseVisibleBeforeCapture;
            }
        }

        private void SetMousePosition(Vector2 normalizedPosition)
        {
            var newPos = uiControl.PointToScreen(
                new System.Drawing.Point((int)(uiControl.ClientRectangle.Width*normalizedPosition.X), (int)(uiControl.ClientRectangle.Height*normalizedPosition.Y)));
            Cursor.Position = newPos;
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
                    keyboard.HandleKeyDown(virtualKey);
                    break;
                case Win32Native.WM_KEYUP:
                case Win32Native.WM_SYSKEYUP:
                    virtualKey = (WinFormsKeys)wParam.ToInt64();
                    virtualKey = GetCorrectExtendedKey(virtualKey, lParam.ToInt64());
                    keyboard.HandleKeyUp(virtualKey);
                    break;
                case Win32Native.WM_CHAR:
                    // TODO: Handle text
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

        /*private void OnMouseInputEvent(Vector2 pixelPosition, MouseButton button, KeyboardWinforms.InputEventType type, float value = 0)
        {
            // The mouse wheel event are still received even when the mouse cursor is out of the control boundaries. Discard the event in this case.
            if (type == KeyboardWinforms.InputEventType.Wheel && !UiControl.ClientRectangle.Contains(UiControl.PointToClient(Control.MousePosition)))
                return;

            // the mouse events series has been interrupted because out of the window.
            if (type == KeyboardWinforms.InputEventType.Up && !MouseButtonCurrentlyDown[(int)button])
                return;

            CurrentMousePosition = NormalizeScreenPosition(pixelPosition);

            var mouseInputEvent = new InputManager.MouseInputEvent { Type = type, MouseButton = button, Value = value };
            lock (MouseInputEvents)
                MouseInputEvents.Add(mouseInputEvent);

            if (type != KeyboardWinforms.InputEventType.Wheel)
            {
                var buttonId = (int)button;
                MouseButtonCurrentlyDown[buttonId] = type == KeyboardWinforms.InputEventType.Down;
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

            // TODO: have a proper distinction between Touch and Mouse
            HandlePointerEvents(-1, CurrentMousePosition, PointerState.Move, PointerType.Mouse);

            if (IsMousePositionLocked)
            {
                Cursor.Position = capturedPosition;
            }
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

        private static PointerState InputEventTypeToPointerState(KeyboardWinforms.InputEventType type)
        {
            switch (type)
            {
                case KeyboardWinforms.InputEventType.Up:
                    return PointerState.Up;
                case KeyboardWinforms.InputEventType.Down:
                    return PointerState.Down;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }*/
    }
}

#endif