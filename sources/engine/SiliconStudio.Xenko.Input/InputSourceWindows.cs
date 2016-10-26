// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using WinFormsKeys = System.Windows.Forms.Keys;

namespace SiliconStudio.Xenko.Input
{
    public class InputSourceWindows : InputSourceBase
    {
        /// <summary>
        /// Gets the value indicating if the mouse position is currently locked or not.
        /// </summary>
        public bool IsMousePositionLocked { get; protected set; }

        private KeyboardWinforms keyboard;
        private MouseWinforms mouse;
        
        private IntPtr defaultWndProc;
        private Win32Native.WndProc inputWndProc;

        private HashSet<WinFormsKeys> heldKeys = new HashSet<WinFormsKeys>();

        // My input devices
        private GameContext<Control> gameContext;
        private GameBase game;
        private Control uiControl;
        private InputManager input;

        public override void Initialize(InputManager inputManager)
        {
            input = inputManager;
            gameContext = inputManager.Game.Context as GameContext<Control>;
            game = inputManager.Game;
            uiControl = gameContext.Control;

            // Hook window proc
            defaultWndProc = Win32Native.GetWindowLong(uiControl.Handle, Win32Native.WindowLongType.WndProc);
            // This is needed to prevent garbage collection of the delegate.
            inputWndProc = WndProc;
            var inputWndProcPtr = Marshal.GetFunctionPointerForDelegate(inputWndProc);
            Win32Native.SetWindowLong(uiControl.Handle, Win32Native.WindowLongType.WndProc, inputWndProcPtr);

            // Do not register keyboard devices when using raw input instead
            if (!InputManager.UseRawInput)
            {
                keyboard = new KeyboardWinforms();
                RegisterDevice(keyboard);
            }

            mouse = new MouseWinforms(game, uiControl);
            RegisterDevice(mouse);
        }

        public override bool IsEnabled(GameContext gameContext)
        {
            return gameContext is GameContext<Control>;
        }

        public override void Update()
        {
            if (keyboard != null)
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
        }

        public override void Dispose()
        {
            // Unregisters devices
            base.Dispose();

            mouse?.Dispose();
            keyboard?.Dispose();
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
                    keyboard?.HandleKeyDown(virtualKey);
                    heldKeys.Add(virtualKey);
                    break;
                case Win32Native.WM_KEYUP:
                case Win32Native.WM_SYSKEYUP:
                    virtualKey = (WinFormsKeys)wParam.ToInt64();
                    virtualKey = GetCorrectExtendedKey(virtualKey, lParam.ToInt64());
                    heldKeys.Remove(virtualKey);
                    keyboard?.HandleKeyUp(virtualKey);
                    break;
                case Win32Native.WM_CHAR:
                    // TODO: Handle text
                    break;
                case Win32Native.WM_DEVICECHANGE:
                    // Trigger scan on device changed
                    input.Scan();
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