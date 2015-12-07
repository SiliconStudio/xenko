// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_UI_SDL
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.SDL;
using Vector2 = SiliconStudio.Core.Mathematics.Vector2;
using SDL2;

namespace SiliconStudio.Xenko.Input
{
    internal class InputManagerSDL: InputManagerWindows<Window>
    {

        public InputManagerSDL(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = true;
            HasPointer = true;

            _pointerClock = new Stopwatch();
        }

        public override void Initialize(GameContext<Window> context)
        {
            switch (context.ContextType)
            {
                case AppContextType.Desktop:
                    InitializeFromContext(context, false);
                    break;
                case AppContextType.DesktopOpenTK:
                    InitializeFromContext(context, true);
                    break;

                default:
                    throw new ArgumentException(string.Format("WindowContext [{0}] not supported", Game.Context.ContextType));
            }

            // Scan all registered inputs
            Scan();
        }

        public override void LockMousePosition(bool forceCenter = false)
        {
            if (!IsMousePositionLocked)
            {
                _wasMouseVisibleBeforeCapture = Game.IsMouseVisible;
                Game.IsMouseVisible = false;
                if (forceCenter)
                {
                    SetMousePosition(new Vector2(0.5f, 0.5f));
                }
                _capturedPosition = Cursor.Position;
                IsMousePositionLocked = true;
            }
        }

        public override void UnlockMousePosition()
        {
            if (IsMousePositionLocked)
            {
                IsMousePositionLocked = false;
                _capturedPosition = Point.Zero;
                Game.IsMouseVisible = _wasMouseVisibleBeforeCapture;
            }
        }

        // FIXME: SDL seems to be always enabled for multitouch.
        public override bool MultiTouchEnabled { get; set; }

        protected override void SetMousePosition(Vector2 normalizedPosition)
        {
            Cursor.Position = new Point(
                (int)(Control.ClientRectangle.Width * normalizedPosition.X),
                (int)(Control.ClientRectangle.Height * normalizedPosition.Y));
        }

        private void InitializeFromContext(GameContext<Window> context, bool isOpenGL)
        {
            Control = context.Control;

            _pointerClock.Restart();

            EnsureMapKeys();
            Control.KeyDownActions += e => OnKeyEvent(e, false);
            Control.KeyUpActions += e => OnKeyEvent(e, true);
            Control.FocusGainedActions += e => OnUiControlGotFocus();
            Control.FocusLostActions += e => OnUiControlLostFocus();
            Control.MouseMoveActions += OnMouseMoveEvent;
            Control.PointerButtonPressActions += e => { OnMouseInputEvent(new Vector2(e.x, e.y), ConvertMouseButton(e.button), InputEventType.Down); };
            Control.PointerButtonReleaseActions += e => OnMouseInputEvent(new Vector2(e.x, e.y), ConvertMouseButton(e.button), InputEventType.Up);
            Control.MouseWheelActions += e =>
            {
                Point pos = Cursor.Position;
                OnMouseInputEvent(new Vector2(pos.X, pos.Y), MouseButton.Middle, InputEventType.Wheel, Math.Max(e.x, e.y));
            };
            Control.ResizeEndActions += UiWindowOnSizeChanged;

            ControlWidth = Control.ClientSize.Width;
            ControlHeight = Control.ClientSize.Height;
        }

        private void OnKeyEvent(SDL.SDL_KeyboardEvent e, bool isKeyUp)
        {
            lock (KeyboardInputEvents)
            {
                Keys key;
                if (MapKeys.TryGetValue(e.keysym.sym, out key) && key != Keys.None)
                {
                    var type = isKeyUp ? InputEventType.Up : InputEventType.Down;
                    KeyboardInputEvents.Add(new KeyboardInputEvent { Key = key, Type = type });
                }
            }
        }

        private void UiWindowOnSizeChanged(SDL.SDL_WindowEvent eventArgs)
        {
            ControlWidth = Control.ClientSize.Width;
            ControlHeight = Control.ClientSize.Height;
        }

        private void OnMouseInputEvent(Vector2 pixelPosition, MouseButton button, InputEventType type, float value = 0)
        {
            // The mouse wheel event are still received even when the mouse cursor is out of the Window boundaries. Discard the event in this case.
            if (type == InputEventType.Wheel && !Control.ClientRectangle.Contains(Cursor.Position))
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

        private void OnMouseMoveEvent(SDL.SDL_MouseMotionEvent e)
        {
            var previousMousePosition = CurrentMousePosition;
            CurrentMousePosition = NormalizeScreenPosition(new Vector2(e.x, e.y));
            // Discard this event if it has been triggered by the replacing the cursor to its capture initial position
            if (IsMousePositionLocked && Cursor.Position == _capturedPosition)
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
                Cursor.Position = _capturedPosition;
            }
        }

        private unsafe void OnUiControlGotFocus()
        {
            lock (KeyboardInputEvents)
            {
                int nb;
                    // Get the state for all keys on the keyboard.
                byte *p = (byte *) SDL.SDL_GetKeyboardState(out nb);
                for (int i = 0; i < nb; i++)
                {
                        // Check if key of scancode `i' is pressed.
                    if (p[i] != 0)
                    {
                        SDL.SDL_Keycode keyCode = SDL.SDL_GetKeyFromScancode((SDL.SDL_Scancode) i);
                        Keys key;
                        if (MapKeys.TryGetValue(keyCode, out key) && key != Keys.None)
                        {
                            KeyboardInputEvents.Add(new KeyboardInputEvent
                            {
                                Key = key,
                                Type = InputEventType.Down,
                                OutOfFocus = true
                            });
                        }
                    }
                }
            }
        }

        private void OnUiControlLostFocus()
        {
            LostFocus = true;
        }

        private static MouseButton ConvertMouseButton(uint mouseButton)
        {
            switch (mouseButton)
            {
                case SDL.SDL_BUTTON_LEFT:
                    return MouseButton.Left;
                case SDL.SDL_BUTTON_RIGHT:
                    return MouseButton.Right;
                case SDL.SDL_BUTTON_MIDDLE:
                    return MouseButton.Middle;
                case SDL.SDL_BUTTON_X1:
                    return MouseButton.Extended1;
                case SDL.SDL_BUTTON_X2:
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
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private readonly Stopwatch _pointerClock;
        private Point _capturedPosition;
        private bool _wasMouseVisibleBeforeCapture;
        private static readonly Dictionary<SDL.SDL_Keycode, Keys> MapKeys = new Dictionary<SDL.SDL_Keycode, Keys>();

        private static void AddKeys(SDL.SDL_Keycode fromKey, Keys toKey)
        {
            if (!MapKeys.ContainsKey(fromKey))
            {
                MapKeys.Add(fromKey, toKey);
            }
        }

        private static void EnsureMapKeys()
        {
            lock (MapKeys)
            {
                if (MapKeys.Count > 0)
                {
                    return;
                }
                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.None);
                AddKeys(SDL.SDL_Keycode.SDLK_CANCEL, Keys.Cancel);
                AddKeys(SDL.SDL_Keycode.SDLK_BACKSPACE, Keys.Back);
                AddKeys(SDL.SDL_Keycode.SDLK_TAB, Keys.Tab);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_TAB, Keys.Tab);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.LineFeed);
                AddKeys(SDL.SDL_Keycode.SDLK_CLEAR, Keys.Clear);
                AddKeys(SDL.SDL_Keycode.SDLK_CLEARAGAIN, Keys.Clear);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_CLEAR, Keys.Clear);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_CLEARENTRY, Keys.Clear);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_ENTER, Keys.Enter);
                AddKeys(SDL.SDL_Keycode.SDLK_RETURN, Keys.Return);
                AddKeys(SDL.SDL_Keycode.SDLK_RETURN2, Keys.Return);
                AddKeys(SDL.SDL_Keycode.SDLK_PAUSE, Keys.Pause);
                AddKeys(SDL.SDL_Keycode.SDLK_CAPSLOCK, Keys.Capital);
                AddKeys(SDL.SDL_Keycode.SDLK_CAPSLOCK, Keys.CapsLock);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.HangulMode);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.KanaMode);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.JunjaMode);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.FinalMode);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.HanjaMode);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.KanjiMode);
                AddKeys(SDL.SDL_Keycode.SDLK_ESCAPE, Keys.Escape);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.ImeConvert);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.ImeNonConvert);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.ImeAccept);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.ImeModeChange);
                AddKeys(SDL.SDL_Keycode.SDLK_SPACE, Keys.Space);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_SPACE, Keys.Space);
                AddKeys(SDL.SDL_Keycode.SDLK_PAGEUP, Keys.PageUp);
                AddKeys(SDL.SDL_Keycode.SDLK_PRIOR, Keys.Prior);
//                AddKeys(SDL.SDL_Keycode.SDLK_PAGEDOWN, Keys.Next); // Next is the same as PageDown
                AddKeys(SDL.SDL_Keycode.SDLK_PAGEDOWN, Keys.PageDown);
                AddKeys(SDL.SDL_Keycode.SDLK_END, Keys.End);
                AddKeys(SDL.SDL_Keycode.SDLK_HOME, Keys.Home);
                AddKeys(SDL.SDL_Keycode.SDLK_AC_HOME, Keys.Home);
                AddKeys(SDL.SDL_Keycode.SDLK_LEFT, Keys.Left);
                AddKeys(SDL.SDL_Keycode.SDLK_UP, Keys.Up);
                AddKeys(SDL.SDL_Keycode.SDLK_RIGHT, Keys.Right);
                AddKeys(SDL.SDL_Keycode.SDLK_DOWN, Keys.Down);
                AddKeys(SDL.SDL_Keycode.SDLK_SELECT, Keys.Select);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Print);
                AddKeys(SDL.SDL_Keycode.SDLK_EXECUTE, Keys.Execute);
                AddKeys(SDL.SDL_Keycode.SDLK_PRINTSCREEN, Keys.PrintScreen);
//                AddKeys(SDL.SDL_Keycode.SDLK_PRINTSCREEN, Keys.Snapshot); // Snapshot is the same as PageDown
                AddKeys(SDL.SDL_Keycode.SDLK_INSERT, Keys.Insert);
                AddKeys(SDL.SDL_Keycode.SDLK_DELETE, Keys.Delete);
                AddKeys(SDL.SDL_Keycode.SDLK_HELP, Keys.Help);
                AddKeys(SDL.SDL_Keycode.SDLK_1, Keys.D0);
                AddKeys(SDL.SDL_Keycode.SDLK_2, Keys.D2);
                AddKeys(SDL.SDL_Keycode.SDLK_3, Keys.D3);
                AddKeys(SDL.SDL_Keycode.SDLK_4, Keys.D4);
                AddKeys(SDL.SDL_Keycode.SDLK_5, Keys.D5);
                AddKeys(SDL.SDL_Keycode.SDLK_6, Keys.D6);
                AddKeys(SDL.SDL_Keycode.SDLK_7, Keys.D7);
                AddKeys(SDL.SDL_Keycode.SDLK_8, Keys.D8);
                AddKeys(SDL.SDL_Keycode.SDLK_9, Keys.D9);
                AddKeys(SDL.SDL_Keycode.SDLK_a, Keys.A);
                AddKeys(SDL.SDL_Keycode.SDLK_b, Keys.B);
                AddKeys(SDL.SDL_Keycode.SDLK_c, Keys.C);
                AddKeys(SDL.SDL_Keycode.SDLK_d, Keys.D);
                AddKeys(SDL.SDL_Keycode.SDLK_e, Keys.E);
                AddKeys(SDL.SDL_Keycode.SDLK_f, Keys.F);
                AddKeys(SDL.SDL_Keycode.SDLK_g, Keys.G);
                AddKeys(SDL.SDL_Keycode.SDLK_h, Keys.H);
                AddKeys(SDL.SDL_Keycode.SDLK_i, Keys.I);
                AddKeys(SDL.SDL_Keycode.SDLK_j, Keys.J);
                AddKeys(SDL.SDL_Keycode.SDLK_k, Keys.K);
                AddKeys(SDL.SDL_Keycode.SDLK_l, Keys.L);
                AddKeys(SDL.SDL_Keycode.SDLK_m, Keys.M);
                AddKeys(SDL.SDL_Keycode.SDLK_n, Keys.N);
                AddKeys(SDL.SDL_Keycode.SDLK_o, Keys.O);
                AddKeys(SDL.SDL_Keycode.SDLK_p, Keys.P);
                AddKeys(SDL.SDL_Keycode.SDLK_q, Keys.Q);
                AddKeys(SDL.SDL_Keycode.SDLK_r, Keys.R);
                AddKeys(SDL.SDL_Keycode.SDLK_s, Keys.S);
                AddKeys(SDL.SDL_Keycode.SDLK_t, Keys.T);
                AddKeys(SDL.SDL_Keycode.SDLK_u, Keys.U);
                AddKeys(SDL.SDL_Keycode.SDLK_v, Keys.V);
                AddKeys(SDL.SDL_Keycode.SDLK_w, Keys.W);
                AddKeys(SDL.SDL_Keycode.SDLK_x, Keys.X);
                AddKeys(SDL.SDL_Keycode.SDLK_y, Keys.Y);
                AddKeys(SDL.SDL_Keycode.SDLK_z, Keys.Z);
                AddKeys(SDL.SDL_Keycode.SDLK_LGUI, Keys.LeftWin);
                AddKeys(SDL.SDL_Keycode.SDLK_RGUI, Keys.RightWin);
                AddKeys(SDL.SDL_Keycode.SDLK_APPLICATION, Keys.Apps); // TODO: Verify value.
                AddKeys(SDL.SDL_Keycode.SDLK_SLEEP, Keys.Sleep);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_0, Keys.NumPad0);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_1, Keys.NumPad1);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_2, Keys.NumPad2);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_3, Keys.NumPad3);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_4, Keys.NumPad4);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_5, Keys.NumPad5);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_6, Keys.NumPad6);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_7, Keys.NumPad7);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_8, Keys.NumPad8);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_9, Keys.NumPad9);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_MULTIPLY, Keys.Multiply);
                AddKeys(SDL.SDL_Keycode.SDLK_PLUS, Keys.Add);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_PLUS, Keys.Add);
                AddKeys(SDL.SDL_Keycode.SDLK_SEPARATOR, Keys.Separator);
                AddKeys(SDL.SDL_Keycode.SDLK_MINUS, Keys.Subtract);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_MINUS, Keys.Subtract);
                AddKeys(SDL.SDL_Keycode.SDLK_DECIMALSEPARATOR, Keys.Decimal);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_DECIMAL, Keys.Decimal);
                AddKeys(SDL.SDL_Keycode.SDLK_KP_DIVIDE, Keys.Divide);
                AddKeys(SDL.SDL_Keycode.SDLK_F1, Keys.F1);
                AddKeys(SDL.SDL_Keycode.SDLK_F2, Keys.F2);
                AddKeys(SDL.SDL_Keycode.SDLK_F3, Keys.F3);
                AddKeys(SDL.SDL_Keycode.SDLK_F4, Keys.F4);
                AddKeys(SDL.SDL_Keycode.SDLK_F5, Keys.F5);
                AddKeys(SDL.SDL_Keycode.SDLK_F6, Keys.F6);
                AddKeys(SDL.SDL_Keycode.SDLK_F7, Keys.F7);
                AddKeys(SDL.SDL_Keycode.SDLK_F8, Keys.F8);
                AddKeys(SDL.SDL_Keycode.SDLK_F9, Keys.F9);
                AddKeys(SDL.SDL_Keycode.SDLK_F10, Keys.F10);
                AddKeys(SDL.SDL_Keycode.SDLK_F11, Keys.F11);
                AddKeys(SDL.SDL_Keycode.SDLK_F12, Keys.F12);
                AddKeys(SDL.SDL_Keycode.SDLK_F13, Keys.F13);
                AddKeys(SDL.SDL_Keycode.SDLK_F14, Keys.F14);
                AddKeys(SDL.SDL_Keycode.SDLK_F15, Keys.F15);
                AddKeys(SDL.SDL_Keycode.SDLK_F16, Keys.F16);
                AddKeys(SDL.SDL_Keycode.SDLK_F17, Keys.F17);
                AddKeys(SDL.SDL_Keycode.SDLK_F18, Keys.F18);
                AddKeys(SDL.SDL_Keycode.SDLK_F19, Keys.F19);
                AddKeys(SDL.SDL_Keycode.SDLK_F20, Keys.F20);
                AddKeys(SDL.SDL_Keycode.SDLK_F21, Keys.F21);
                AddKeys(SDL.SDL_Keycode.SDLK_F22, Keys.F22);
                AddKeys(SDL.SDL_Keycode.SDLK_F23, Keys.F23);
                AddKeys(SDL.SDL_Keycode.SDLK_F24, Keys.F24);
                AddKeys(SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR, Keys.NumLock);
                AddKeys(SDL.SDL_Keycode.SDLK_SCROLLLOCK, Keys.Scroll);
                AddKeys(SDL.SDL_Keycode.SDLK_LSHIFT, Keys.LeftShift);
                AddKeys(SDL.SDL_Keycode.SDLK_RSHIFT, Keys.RightShift);
                AddKeys(SDL.SDL_Keycode.SDLK_LCTRL, Keys.LeftCtrl);
                AddKeys(SDL.SDL_Keycode.SDLK_RCTRL, Keys.RightCtrl);
                AddKeys(SDL.SDL_Keycode.SDLK_LALT, Keys.LeftAlt);
                AddKeys(SDL.SDL_Keycode.SDLK_RALT, Keys.RightAlt);
                AddKeys(SDL.SDL_Keycode.SDLK_AC_BACK, Keys.BrowserBack);
                AddKeys(SDL.SDL_Keycode.SDLK_AC_FORWARD, Keys.BrowserForward);
                AddKeys(SDL.SDL_Keycode.SDLK_AC_REFRESH, Keys.BrowserRefresh);
                AddKeys(SDL.SDL_Keycode.SDLK_AC_STOP, Keys.BrowserStop);
                AddKeys(SDL.SDL_Keycode.SDLK_AC_SEARCH, Keys.BrowserSearch);
                AddKeys(SDL.SDL_Keycode.SDLK_AC_BOOKMARKS, Keys.BrowserFavorites);
                AddKeys(SDL.SDL_Keycode.SDLK_AC_HOME, Keys.BrowserHome);
                AddKeys(SDL.SDL_Keycode.SDLK_AUDIOMUTE, Keys.VolumeMute);
                AddKeys(SDL.SDL_Keycode.SDLK_VOLUMEDOWN, Keys.VolumeDown);
                AddKeys(SDL.SDL_Keycode.SDLK_VOLUMEUP, Keys.VolumeUp);
                AddKeys(SDL.SDL_Keycode.SDLK_AUDIONEXT, Keys.MediaNextTrack);
                AddKeys(SDL.SDL_Keycode.SDLK_AUDIOPREV, Keys.MediaPreviousTrack);
                AddKeys(SDL.SDL_Keycode.SDLK_AUDIOSTOP, Keys.MediaStop);
                AddKeys(SDL.SDL_Keycode.SDLK_AUDIOPLAY, Keys.MediaPlayPause);
                AddKeys(SDL.SDL_Keycode.SDLK_MAIL, Keys.LaunchMail);
                AddKeys(SDL.SDL_Keycode.SDLK_MEDIASELECT, Keys.SelectMedia);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.LaunchApplication1);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.LaunchApplication2);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem1);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemSemicolon);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemPlus);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemComma);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemMinus);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemPeriod);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem2);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemQuestion);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem3);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemTilde);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem4);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemOpenBrackets);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem5);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemPipe);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem6);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemCloseBrackets);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem7);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemQuotes);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem8);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Oem102);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemBackslash);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Attn);
                AddKeys(SDL.SDL_Keycode.SDLK_CRSEL, Keys.CrSel);
                AddKeys(SDL.SDL_Keycode.SDLK_EXSEL, Keys.ExSel);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.EraseEof);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Play);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Zoom);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.NoName);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Pa1);
//                AddKeys(SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.OemClear);
            }
        }
    }
}
#endif
