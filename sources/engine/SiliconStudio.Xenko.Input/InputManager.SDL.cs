// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_UI_SDL
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

            GamePadFactories.Add(new SdlInputGamePadFactory());
        }

        public override void Initialize(GameContext<Window> context)
        {
            switch (context.ContextType)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
                case AppContextType.Desktop:
                case AppContextType.DesktopOpenTK:
                case AppContextType.DesktopSDL:
                    InitializeFromContext(context, true);
                    break;
#else
                case AppContextType.Desktop:
                case AppContextType.DesktopSDL:
                    InitializeFromContext(context, false);
                    break;
                case AppContextType.DesktopOpenTK:
                    InitializeFromContext(context, true);
                    break;
#endif
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
                _relativeCapturedPosition = UiControl.RelativeCursorPosition;
                IsMousePositionLocked = true;
            }
        }

        public override void UnlockMousePosition()
        {
            if (IsMousePositionLocked)
            {
                IsMousePositionLocked = false;
                _relativeCapturedPosition = Point.Zero;
                Game.IsMouseVisible = _wasMouseVisibleBeforeCapture;
            }
        }

        // FIXME: SDL seems to be always enabled for multitouch.
        public override bool MultiTouchEnabled { get; set; }

        protected override void SetMousePosition(Vector2 normalizedPosition)
        {
            Cursor.Position = new Point(
                (int)(UiControl.ClientRectangle.Width * normalizedPosition.X),
                (int)(UiControl.ClientRectangle.Height * normalizedPosition.Y));
        }

        private void InitializeFromContext(GameContext<Window> context, bool isOpenGL)
        {
            UiControl = context.Control;

            _pointerClock.Restart();

            UiControl.KeyDownActions += e => OnKeyEvent(e, false);
            UiControl.KeyUpActions += e => OnKeyEvent(e, true);
            UiControl.FocusGainedActions += e => OnUiControlGotFocus();
            UiControl.FocusLostActions += e => OnUiControlLostFocus();
            UiControl.MouseMoveActions += OnMouseMoveEvent;
            UiControl.PointerButtonPressActions += e => { OnMouseInputEvent(new Vector2(e.x, e.y), ConvertMouseButton(e.button), InputEventType.Down); };
            UiControl.PointerButtonReleaseActions += e => OnMouseInputEvent(new Vector2(e.x, e.y), ConvertMouseButton(e.button), InputEventType.Up);
            UiControl.MouseWheelActions += e =>
            {
                Point pos = Cursor.Position;
                // Only use `e.y` on SDL as this will be where the deltas will be.
                OnMouseInputEvent(new Vector2(pos.X, pos.Y), MouseButton.Middle, InputEventType.Wheel, e.y);
            };
            UiControl.ResizeEndActions += UiWindowOnSizeChanged;

            ControlWidth = UiControl.ClientSize.Width;
            ControlHeight = UiControl.ClientSize.Height;
        }

        private void OnKeyEvent(SDL.SDL_KeyboardEvent e, bool isKeyUp)
        {
            lock (KeyboardInputEvents)
            {
                Keys key;
                if (SDLKeys.mapKeys.TryGetValue(e.keysym.sym, out key) && key != Keys.None)
                {
                    var type = isKeyUp ? InputEventType.Up : InputEventType.Down;
                    KeyboardInputEvents.Add(new KeyboardInputEvent { Key = key, Type = type });
                }
            }
        }

        private void UiWindowOnSizeChanged(SDL.SDL_WindowEvent eventArgs)
        {
            ControlWidth = UiControl.ClientSize.Width;
            ControlHeight = UiControl.ClientSize.Height;
        }

        private void OnMouseInputEvent(Vector2 pixelPosition, MouseButton button, InputEventType type, float value = 0)
        {
            // The mouse wheel event are still received even when the mouse cursor is out of the Window boundaries. Discard the event in this case.
            if (type == InputEventType.Wheel && !UiControl.ClientRectangle.Contains(UiControl.RelativeCursorPosition))
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
            if (IsMousePositionLocked && (e.x == _relativeCapturedPosition.X && e.y == _relativeCapturedPosition.Y))
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
                // Restore position to prevent mouse from going out of the window where we would not get
                // mouse move event.
                UiControl.RelativeCursorPosition = _relativeCapturedPosition;
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
                        if (SDLKeys.mapKeys.TryGetValue(keyCode, out key) && key != Keys.None)
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
        /// <summary>
        /// Location of mouse in Window coordinate when mouse is captured
        /// </summary>
        private Point _relativeCapturedPosition;
        private bool _wasMouseVisibleBeforeCapture;

    }

    /// <summary>
    /// Mapping between <see cref="SDL.SDL_Keycode"/> and <see cref="SiliconStudio.Xenko.Input.Keys"/> needed for
    /// translating SDL key events into Xenko ones.
    /// </summary>
    static class SDLKeys
    {
        /// <summary>
        /// Map between SDL keys and Xenko keys.
        /// </summary>
        internal static readonly Dictionary<SDL.SDL_Keycode, Keys> mapKeys = NewMapKeys();

        /// <summary>
        /// Create a mapping between <see cref="SDL.SDL_Keycode"/> and <see cref="SiliconStudio.Xenko.Input.Keys"/>
        /// </summary>
        /// <remarks>Not all <see cref="SiliconStudio.Xenko.Input.Keys"/> have a corresponding SDL entries. For the moment they are commented out in the code below.</remarks>
        /// <returns>A new map.</returns>
        private static Dictionary<SDL.SDL_Keycode, Keys> NewMapKeys()
        {
            var map = new Dictionary<SDL.SDL_Keycode, Keys>(200); 
            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.None;
            map [SDL.SDL_Keycode.SDLK_CANCEL] = Keys.Cancel;
            map [SDL.SDL_Keycode.SDLK_BACKSPACE] = Keys.Back;
            map [SDL.SDL_Keycode.SDLK_TAB] = Keys.Tab;
            map [SDL.SDL_Keycode.SDLK_KP_TAB] = Keys.Tab;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.LineFeed;
            map [SDL.SDL_Keycode.SDLK_CLEAR] = Keys.Clear;
            map [SDL.SDL_Keycode.SDLK_CLEARAGAIN] = Keys.Clear;
            map [SDL.SDL_Keycode.SDLK_KP_CLEAR] = Keys.Clear;
            map [SDL.SDL_Keycode.SDLK_KP_CLEARENTRY] = Keys.Clear;
            map [SDL.SDL_Keycode.SDLK_KP_ENTER] = Keys.Enter;
            map [SDL.SDL_Keycode.SDLK_RETURN] = Keys.Return;
            map [SDL.SDL_Keycode.SDLK_RETURN2] = Keys.Return;
            map [SDL.SDL_Keycode.SDLK_PAUSE] = Keys.Pause;
            map [SDL.SDL_Keycode.SDLK_CAPSLOCK] = Keys.Capital;
//            map [SDL.SDL_Keycode.SDLK_CAPSLOCK] = Keys.CapsLock;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.HangulMode;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.KanaMode;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.JunjaMode;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.FinalMode;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.HanjaMode;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.KanjiMode;
            map [SDL.SDL_Keycode.SDLK_ESCAPE] = Keys.Escape;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.ImeConvert;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.ImeNonConvert;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.ImeAccept;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.ImeModeChange;
            map [SDL.SDL_Keycode.SDLK_SPACE] = Keys.Space;
            map [SDL.SDL_Keycode.SDLK_KP_SPACE] = Keys.Space;
            map [SDL.SDL_Keycode.SDLK_PAGEUP] = Keys.PageUp;
            map [SDL.SDL_Keycode.SDLK_PRIOR] = Keys.Prior;
//            map [SDL.SDL_Keycode.SDLK_PAGEDOWN] = Keys.Next); // Next is the same as PageDo;
            map [SDL.SDL_Keycode.SDLK_PAGEDOWN] = Keys.PageDown;
            map [SDL.SDL_Keycode.SDLK_END] = Keys.End;
            map [SDL.SDL_Keycode.SDLK_HOME] = Keys.Home;
            map [SDL.SDL_Keycode.SDLK_AC_HOME] = Keys.Home;
            map [SDL.SDL_Keycode.SDLK_LEFT] = Keys.Left;
            map [SDL.SDL_Keycode.SDLK_UP] = Keys.Up;
            map [SDL.SDL_Keycode.SDLK_RIGHT] = Keys.Right;
            map [SDL.SDL_Keycode.SDLK_DOWN] = Keys.Down;
            map [SDL.SDL_Keycode.SDLK_SELECT] = Keys.Select;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Print;
            map [SDL.SDL_Keycode.SDLK_EXECUTE] = Keys.Execute;
            map [SDL.SDL_Keycode.SDLK_PRINTSCREEN] = Keys.PrintScreen;
//            map [SDL.SDL_Keycode.SDLK_PRINTSCREEN] = Keys.Snapshot); // Snapshot is the same as PageDo;
            map [SDL.SDL_Keycode.SDLK_INSERT] = Keys.Insert;
            map [SDL.SDL_Keycode.SDLK_DELETE] = Keys.Delete;
            map [SDL.SDL_Keycode.SDLK_HELP] = Keys.Help;
            map [SDL.SDL_Keycode.SDLK_1] = Keys.D0;
            map [SDL.SDL_Keycode.SDLK_2] = Keys.D2;
            map [SDL.SDL_Keycode.SDLK_3] = Keys.D3;
            map [SDL.SDL_Keycode.SDLK_4] = Keys.D4;
            map [SDL.SDL_Keycode.SDLK_5] = Keys.D5;
            map [SDL.SDL_Keycode.SDLK_6] = Keys.D6;
            map [SDL.SDL_Keycode.SDLK_7] = Keys.D7;
            map [SDL.SDL_Keycode.SDLK_8] = Keys.D8;
            map [SDL.SDL_Keycode.SDLK_9] = Keys.D9;
            map [SDL.SDL_Keycode.SDLK_a] = Keys.A;
            map [SDL.SDL_Keycode.SDLK_b] = Keys.B;
            map [SDL.SDL_Keycode.SDLK_c] = Keys.C;
            map [SDL.SDL_Keycode.SDLK_d] = Keys.D;
            map [SDL.SDL_Keycode.SDLK_e] = Keys.E;
            map [SDL.SDL_Keycode.SDLK_f] = Keys.F;
            map [SDL.SDL_Keycode.SDLK_g] = Keys.G;
            map [SDL.SDL_Keycode.SDLK_h] = Keys.H;
            map [SDL.SDL_Keycode.SDLK_i] = Keys.I;
            map [SDL.SDL_Keycode.SDLK_j] = Keys.J;
            map [SDL.SDL_Keycode.SDLK_k] = Keys.K;
            map [SDL.SDL_Keycode.SDLK_l] = Keys.L;
            map [SDL.SDL_Keycode.SDLK_m] = Keys.M;
            map [SDL.SDL_Keycode.SDLK_n] = Keys.N;
            map [SDL.SDL_Keycode.SDLK_o] = Keys.O;
            map [SDL.SDL_Keycode.SDLK_p] = Keys.P;
            map [SDL.SDL_Keycode.SDLK_q] = Keys.Q;
            map [SDL.SDL_Keycode.SDLK_r] = Keys.R;
            map [SDL.SDL_Keycode.SDLK_s] = Keys.S;
            map [SDL.SDL_Keycode.SDLK_t] = Keys.T;
            map [SDL.SDL_Keycode.SDLK_u] = Keys.U;
            map [SDL.SDL_Keycode.SDLK_v] = Keys.V;
            map [SDL.SDL_Keycode.SDLK_w] = Keys.W;
            map [SDL.SDL_Keycode.SDLK_x] = Keys.X;
            map [SDL.SDL_Keycode.SDLK_y] = Keys.Y;
            map [SDL.SDL_Keycode.SDLK_z] = Keys.Z;
            map [SDL.SDL_Keycode.SDLK_LGUI] = Keys.LeftWin;
            map [SDL.SDL_Keycode.SDLK_RGUI] = Keys.RightWin;
            map [SDL.SDL_Keycode.SDLK_APPLICATION] = Keys.Apps; // TODO: Verify value
            map [SDL.SDL_Keycode.SDLK_SLEEP] = Keys.Sleep;
            map [SDL.SDL_Keycode.SDLK_KP_0] = Keys.NumPad0;
            map [SDL.SDL_Keycode.SDLK_KP_1] = Keys.NumPad1;
            map [SDL.SDL_Keycode.SDLK_KP_2] = Keys.NumPad2;
            map [SDL.SDL_Keycode.SDLK_KP_3] = Keys.NumPad3;
            map [SDL.SDL_Keycode.SDLK_KP_4] = Keys.NumPad4;
            map [SDL.SDL_Keycode.SDLK_KP_5] = Keys.NumPad5;
            map [SDL.SDL_Keycode.SDLK_KP_6] = Keys.NumPad6;
            map [SDL.SDL_Keycode.SDLK_KP_7] = Keys.NumPad7;
            map [SDL.SDL_Keycode.SDLK_KP_8] = Keys.NumPad8;
            map [SDL.SDL_Keycode.SDLK_KP_9] = Keys.NumPad9;
            map [SDL.SDL_Keycode.SDLK_KP_MULTIPLY] = Keys.Multiply;
            map [SDL.SDL_Keycode.SDLK_PLUS] = Keys.Add;
            map [SDL.SDL_Keycode.SDLK_KP_PLUS] = Keys.Add;
            map [SDL.SDL_Keycode.SDLK_SEPARATOR] = Keys.Separator;
            map [SDL.SDL_Keycode.SDLK_MINUS] = Keys.Subtract;
            map [SDL.SDL_Keycode.SDLK_KP_MINUS] = Keys.Subtract;
            map [SDL.SDL_Keycode.SDLK_DECIMALSEPARATOR] = Keys.Decimal;
            map [SDL.SDL_Keycode.SDLK_KP_DECIMAL] = Keys.Decimal;
            map [SDL.SDL_Keycode.SDLK_KP_DIVIDE] = Keys.Divide;
            map [SDL.SDL_Keycode.SDLK_F1] = Keys.F1;
            map [SDL.SDL_Keycode.SDLK_F2] = Keys.F2;
            map [SDL.SDL_Keycode.SDLK_F3] = Keys.F3;
            map [SDL.SDL_Keycode.SDLK_F4] = Keys.F4;
            map [SDL.SDL_Keycode.SDLK_F5] = Keys.F5;
            map [SDL.SDL_Keycode.SDLK_F6] = Keys.F6;
            map [SDL.SDL_Keycode.SDLK_F7] = Keys.F7;
            map [SDL.SDL_Keycode.SDLK_F8] = Keys.F8;
            map [SDL.SDL_Keycode.SDLK_F9] = Keys.F9;
            map [SDL.SDL_Keycode.SDLK_F10] = Keys.F10;
            map [SDL.SDL_Keycode.SDLK_F11] = Keys.F11;
            map [SDL.SDL_Keycode.SDLK_F12] = Keys.F12;
            map [SDL.SDL_Keycode.SDLK_F13] = Keys.F13;
            map [SDL.SDL_Keycode.SDLK_F14] = Keys.F14;
            map [SDL.SDL_Keycode.SDLK_F15] = Keys.F15;
            map [SDL.SDL_Keycode.SDLK_F16] = Keys.F16;
            map [SDL.SDL_Keycode.SDLK_F17] = Keys.F17;
            map [SDL.SDL_Keycode.SDLK_F18] = Keys.F18;
            map [SDL.SDL_Keycode.SDLK_F19] = Keys.F19;
            map [SDL.SDL_Keycode.SDLK_F20] = Keys.F20;
            map [SDL.SDL_Keycode.SDLK_F21] = Keys.F21;
            map [SDL.SDL_Keycode.SDLK_F22] = Keys.F22;
            map [SDL.SDL_Keycode.SDLK_F23] = Keys.F23;
            map [SDL.SDL_Keycode.SDLK_F24] = Keys.F24;
            map [SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR] = Keys.NumLock;
            map [SDL.SDL_Keycode.SDLK_SCROLLLOCK] = Keys.Scroll;
            map [SDL.SDL_Keycode.SDLK_LSHIFT] = Keys.LeftShift;
            map [SDL.SDL_Keycode.SDLK_RSHIFT] = Keys.RightShift;
            map [SDL.SDL_Keycode.SDLK_LCTRL] = Keys.LeftCtrl;
            map [SDL.SDL_Keycode.SDLK_RCTRL] = Keys.RightCtrl;
            map [SDL.SDL_Keycode.SDLK_LALT] = Keys.LeftAlt;
            map [SDL.SDL_Keycode.SDLK_RALT] = Keys.RightAlt;
            map [SDL.SDL_Keycode.SDLK_AC_BACK] = Keys.BrowserBack;
            map [SDL.SDL_Keycode.SDLK_AC_FORWARD] = Keys.BrowserForward;
            map [SDL.SDL_Keycode.SDLK_AC_REFRESH] = Keys.BrowserRefresh;
            map [SDL.SDL_Keycode.SDLK_AC_STOP] = Keys.BrowserStop;
            map [SDL.SDL_Keycode.SDLK_AC_SEARCH] = Keys.BrowserSearch;
            map [SDL.SDL_Keycode.SDLK_AC_BOOKMARKS] = Keys.BrowserFavorites;
            map [SDL.SDL_Keycode.SDLK_AC_HOME] = Keys.BrowserHome;
            map [SDL.SDL_Keycode.SDLK_AUDIOMUTE] = Keys.VolumeMute;
            map [SDL.SDL_Keycode.SDLK_VOLUMEDOWN] = Keys.VolumeDown;
            map [SDL.SDL_Keycode.SDLK_VOLUMEUP] = Keys.VolumeUp;
            map [SDL.SDL_Keycode.SDLK_AUDIONEXT] = Keys.MediaNextTrack;
            map [SDL.SDL_Keycode.SDLK_AUDIOPREV] = Keys.MediaPreviousTrack;
            map [SDL.SDL_Keycode.SDLK_AUDIOSTOP] = Keys.MediaStop;
            map [SDL.SDL_Keycode.SDLK_AUDIOPLAY] = Keys.MediaPlayPause;
            map [SDL.SDL_Keycode.SDLK_MAIL] = Keys.LaunchMail;
            map [SDL.SDL_Keycode.SDLK_MEDIASELECT] = Keys.SelectMedia;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.LaunchApplication1;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.LaunchApplication2;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem1;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemSemicolon;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemPlus;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemComma;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemMinus;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemPeriod;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem2;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemQuestion;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem3;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemTilde;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem4;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemOpenBrackets;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem5;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemPipe;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem6;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemCloseBrackets;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem7;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemQuotes;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem8;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Oem102;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemBackslash;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Attn;
            map [SDL.SDL_Keycode.SDLK_CRSEL] = Keys.CrSel;
            map [SDL.SDL_Keycode.SDLK_EXSEL] = Keys.ExSel;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.EraseEof;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Play;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Zoom;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.NoName;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.Pa1;
//            map [SDL.SDL_Keycode.SDLK_UNKNOWN] = Keys.OemClear;
            return map;
        }
    }
}
#endif
