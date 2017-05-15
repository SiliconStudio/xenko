// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SiliconStudio.Xenko.Games;
using WinFormsKeys = System.Windows.Forms.Keys;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides support for mouse and keyboard input on windows forms
    /// </summary>
    internal class InputSourceWinforms : InputSourceBase
    {
        private readonly HashSet<WinFormsKeys> heldKeys = new HashSet<WinFormsKeys>();

        private KeyboardWinforms keyboard;
        private MouseWinforms mouse;

        private IntPtr defaultWndProc;
        private Win32Native.WndProc inputWndProc;

        // My input devices
        private GameContext<Control> gameContext;
        private GameBase game;
        private Control uiControl;
        private InputManager input;

        /// <summary>
        /// Gets the value indicating if the mouse position is currently locked or not.
        /// </summary>
        public bool IsMousePositionLocked { get; protected set; }

        public override void Initialize(InputManager inputManager)
        {
            input = inputManager;
            gameContext = inputManager.Game.Context as GameContext<Control>;
            game = inputManager.Game;
            uiControl = gameContext.Control;
            uiControl.LostFocus += UIControlOnLostFocus;

            // Hook window proc
            defaultWndProc = Win32Native.GetWindowLong(uiControl.Handle, Win32Native.WindowLongType.WndProc);
            // This is needed to prevent garbage collection of the delegate.
            inputWndProc = WndProc;
            var inputWndProcPtr = Marshal.GetFunctionPointerForDelegate(inputWndProc);
            Win32Native.SetWindowLong(uiControl.Handle, Win32Native.WindowLongType.WndProc, inputWndProcPtr);

            // Do not register keyboard devices when using raw input instead
            keyboard = new KeyboardWinforms(this, uiControl);
            RegisterDevice(keyboard);

            mouse = new MouseWinforms(this, game, uiControl);
            RegisterDevice(mouse);
        }

        public override void Dispose()
        {
            // Unregisters devices
            base.Dispose();

            mouse?.Dispose();
            keyboard?.Dispose();
        }

        public override void Update()
        {
        }
        
        private void UIControlOnLostFocus(object sender, EventArgs eventArgs)
        {
            // Release keys/buttons when control focus is lost (this prevents some keys getting stuck when a focus loss happens when moving the camera)
            if (keyboard != null)
            {
                foreach(var key in keyboard.KeyRepeats.Keys.ToArray())
                {
                    keyboard.HandleKeyUp(key);
                }
            }

            if (mouse != null)
            {
                foreach (var button in mouse.DownButtons.ToArray())
                {
                    mouse.HandleButtonUp(button);
                }
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
    }
}

#endif