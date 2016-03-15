// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if (SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_LINUX) && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && SILICONSTUDIO_XENKO_UI_OPENTK

using System;
using OpenTK.Input;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;
using GameWindow = OpenTK.GameWindow;
using Vector2 = SiliconStudio.Core.Mathematics.Vector2;

namespace SiliconStudio.Xenko.Input
{
    internal class InputManagerOpenTK : InputManagerWindows<OpenTK.GameWindow>
    {
        private GameWindow gameWindow;

        public InputManagerOpenTK(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = true;
            HasPointer = true;

#if !SILICONSTUDIO_PLATFORM_LINUX
            GamePadFactories.Add(new XInputGamePadFactory());
#endif
        }

        public override void Initialize(GameContext<OpenTK.GameWindow> context)
        {
            switch (context.ContextType)
            {
                case AppContextType.DesktopOpenTK:
                    InitializeFromOpenTK(context);
                    break;

                default:
                    throw new ArgumentException(string.Format("WindowContext [{0}] not supported", Game.Context.ContextType));
            }

            // Scan all registered inputs
            Scan();
        }

        public void InitializeFromOpenTK(GameContext<OpenTK.GameWindow> gameContext)
        {
            gameWindow = gameContext.Control;

            gameWindow.KeyDown += Keyboard_KeyDown;
            gameWindow.KeyUp += Keyboard_KeyUp;
            gameWindow.MouseDown += Mouse_ButtonDown;
            gameWindow.MouseUp += Mouse_ButtonUp;
            gameWindow.MouseMove += Mouse_Move;
            gameWindow.Resize += GameWindowOnResize;

            GameWindowOnResize(null, EventArgs.Empty);
        }

        private void GameWindowOnResize(object sender, EventArgs eventArgs)
        {
            ControlHeight = gameWindow.Height;
            ControlWidth = gameWindow.Width;
        }

        void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            var previousMousePosition = CurrentMousePosition;
            CurrentMousePosition = new Vector2(e.X / ControlWidth, e.Y / ControlHeight);

            CurrentMouseDelta += CurrentMousePosition - previousMousePosition;

            // trigger touch move events
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                var buttonId = (int)button;
                if (MouseButtonCurrentlyDown[buttonId])
                    HandlePointerEvents(buttonId, CurrentMousePosition, PointerState.Move, PointerType.Mouse);
            }
        }

        void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            var button = ConvertMouseButtonFromOpenTK(e.Button);
            var buttonId = (int)button;

            // the mouse events series has been interrupted because out of the window.
            if (!MouseButtonCurrentlyDown[buttonId])
                return;
            
            CurrentMousePosition = new Vector2(e.X / ControlWidth, e.Y / ControlHeight);
            var mouseInputEvent = new MouseInputEvent { Type = InputEventType.Up, MouseButton = button };
            lock (MouseInputEvents)
                MouseInputEvents.Add(mouseInputEvent);

            MouseButtonCurrentlyDown[buttonId] = false;
            HandlePointerEvents(buttonId, CurrentMousePosition, PointerState.Up, PointerType.Mouse);
        }

        void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = ConvertMouseButtonFromOpenTK(e.Button);
            var buttonId = (int)button;

            CurrentMousePosition = new Vector2(e.X / ControlWidth, e.Y / ControlHeight);
            var mouseInputEvent = new MouseInputEvent { Type = InputEventType.Down, MouseButton = button };
            lock (MouseInputEvents)
                MouseInputEvents.Add(mouseInputEvent);

            MouseButtonCurrentlyDown[buttonId] = true;
            HandlePointerEvents(buttonId, CurrentMousePosition, PointerState.Down, PointerType.Mouse);
        }

        void Keyboard_KeyUp(object sender, KeyboardKeyEventArgs arg)
        {
            lock (KeyboardInputEvents)
                KeyboardInputEvents.Add(new KeyboardInputEvent { Key = ConvertKeyFromOpenTK(arg.Key), Type = InputEventType.Up });
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs arg)
        {
            lock (KeyboardInputEvents)
                KeyboardInputEvents.Add(new KeyboardInputEvent { Key = ConvertKeyFromOpenTK(arg.Key), Type = InputEventType.Down });
        }

        private Keys ConvertKeyFromOpenTK(Key key)
        {
            switch (key)
            {
                case Key.Number0: return Keys.D0;
                case Key.Number1: return Keys.D1;
                case Key.Number2: return Keys.D2;
                case Key.Number3: return Keys.D3;
                case Key.Number4: return Keys.D4;
                case Key.Number5: return Keys.D5;
                case Key.Number6: return Keys.D6;
                case Key.Number7: return Keys.D7;
                case Key.Number8: return Keys.D8;
                case Key.Number9: return Keys.D9;
                case Key.A: return Keys.A;
                case Key.B: return Keys.B;
                case Key.C: return Keys.C;
                case Key.D: return Keys.D;
                case Key.E: return Keys.E;
                case Key.F: return Keys.F;
                case Key.G: return Keys.G;
                case Key.H: return Keys.H;
                case Key.I: return Keys.I;
                case Key.J: return Keys.J;
                case Key.K: return Keys.K;
                case Key.L: return Keys.L;
                case Key.M: return Keys.M;
                case Key.N: return Keys.N;
                case Key.O: return Keys.O;
                case Key.P: return Keys.P;
                case Key.Q: return Keys.Q;
                case Key.R: return Keys.R;
                case Key.S: return Keys.S;
                case Key.T: return Keys.T;
                case Key.U: return Keys.U;
                case Key.V: return Keys.V;
                case Key.W: return Keys.W;
                case Key.X: return Keys.X;
                case Key.Y: return Keys.Y;
                case Key.Z: return Keys.Z;
                case Key.F1: return Keys.F1;
                case Key.F2: return Keys.F2;
                case Key.F3: return Keys.F3;
                case Key.F4: return Keys.F4;
                case Key.F5: return Keys.F5;
                case Key.F6: return Keys.F6;
                case Key.F7: return Keys.F7;
                case Key.F8: return Keys.F8;
                case Key.F9: return Keys.F9;
                case Key.F10: return Keys.F10;
                case Key.F11: return Keys.F11;
                case Key.F12: return Keys.F12;
                case Key.Space: return Keys.Space;
                case Key.AltLeft: return Keys.LeftAlt;
                case Key.AltRight: return Keys.RightAlt;
                case Key.ShiftLeft: return Keys.LeftShift;
                case Key.ShiftRight: return Keys.RightShift;
                case Key.ControlLeft: return Keys.LeftCtrl;
                case Key.ControlRight: return Keys.RightCtrl;
                case Key.Enter: return Keys.Enter;
                case Key.BackSpace: return Keys.Back;
                case Key.Tab: return Keys.Tab;
                case Key.Insert: return Keys.Insert;
                case Key.Delete: return Keys.Delete;
                case Key.Home: return Keys.Home;
                case Key.End: return Keys.End;
                case Key.PageUp: return Keys.PageUp;
                case Key.PageDown: return Keys.PageDown;
                case Key.Up: return Keys.Up;
                case Key.Down: return Keys.Down;
                case Key.Left: return Keys.Left;
                case Key.Right: return Keys.Right;
                default:
                    return (Keys)(-1);
            }
        }

        private MouseButton ConvertMouseButtonFromOpenTK(OpenTK.Input.MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case OpenTK.Input.MouseButton.Left:
                    return MouseButton.Left;
                case OpenTK.Input.MouseButton.Right:
                    return MouseButton.Right;
                case OpenTK.Input.MouseButton.Middle:
                    return MouseButton.Middle;
                case OpenTK.Input.MouseButton.Button1:
                    return MouseButton.Extended1;
                case OpenTK.Input.MouseButton.Button2:
                    return MouseButton.Extended2;
            }
            return (MouseButton)(-1);
        }

        // There is no multi-touch on windows, so there is nothing specific to do.
        public override bool MultiTouchEnabled { get; set; }
    }
}
#endif