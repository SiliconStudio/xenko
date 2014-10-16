// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using Android.Views;
using OpenTK.Platform.Android;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    public partial class InputManager
    {
        private AndroidGameView gameView;

        public InputManager(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = false;
            HasPointer = true;
        }

        public override void Initialize()
        {
            var viewListener = new ViewListener(this);
            gameView = Game.Context.Control;
            gameView.SetOnTouchListener(viewListener);
            gameView.SetOnKeyListener(viewListener);
            gameView.Resize += GameViewOnResize;

            GameViewOnResize(null, EventArgs.Empty);
        }

        private void GameViewOnResize(object sender, EventArgs eventArgs)
        {
            ControlWidth = gameView.Size.Width;
            ControlHeight = gameView.Size.Height;
        }

        private bool OnTouch(MotionEvent e)
        {
            PointerState state;
            switch (e.ActionMasked)
            {
                case MotionEventActions.Cancel:
                    state = PointerState.Cancel;
                    break;
                case MotionEventActions.Move:
                    state = PointerState.Move;
                    break;
                case MotionEventActions.Outside:
                    state = PointerState.Out;
                    break;
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    state = PointerState.Down;
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    state = PointerState.Up;
                    break;
                default:
                    // Not handled
                    return false;
            }

            var startIndex = 0;
            var endIndex = e.PointerCount;

            if (state == PointerState.Down || state == PointerState.Up || state == PointerState.Out)
            {
                startIndex = e.ActionIndex;
                endIndex = startIndex + 1;
            }

            for (var i = startIndex; i < endIndex; ++i)
            {
                var pointerId = e.GetPointerId(i);
                var pixelPosition = new Vector2(e.GetX(i), e.GetY(i));

                if(MultiTouchEnabled || pointerId == 0) // manually drop multi-touch events when disabled
                    HandlePointerEvents(pointerId, NormalizeScreenPosition(pixelPosition), state);
            }

            return true;
        }

        private bool OnKey(Keycode keyCode, Android.Views.KeyEvent e)
        {
            lock (KeyboardInputEvents)
            {
                KeyboardInputEvents.Add(new KeyboardInputEvent
                    {
                        Key = ConvertKeyFromAndroid(keyCode),
                        Type = e.Action == KeyEventActions.Down ? InputEventType.Down : InputEventType.Up,
                    });
            }
            return true;
        }

        private Keys ConvertKeyFromAndroid(Keycode key)
        {
            switch (key)
            {
                case Keycode.Num0: return Keys.D0;
                case Keycode.Num1: return Keys.D1;
                case Keycode.Num2: return Keys.D2;
                case Keycode.Num3: return Keys.D3;
                case Keycode.Num4: return Keys.D4;
                case Keycode.Num5: return Keys.D5;
                case Keycode.Num6: return Keys.D6;
                case Keycode.Num7: return Keys.D7;
                case Keycode.Num8: return Keys.D8;
                case Keycode.Num9: return Keys.D9;
                case Keycode.A: return Keys.A;
                case Keycode.B: return Keys.B;
                case Keycode.C: return Keys.C;
                case Keycode.D: return Keys.D;
                case Keycode.E: return Keys.E;
                case Keycode.F: return Keys.F;
                case Keycode.G: return Keys.G;
                case Keycode.H: return Keys.H;
                case Keycode.I: return Keys.I;
                case Keycode.J: return Keys.J;
                case Keycode.K: return Keys.K;
                case Keycode.L: return Keys.L;
                case Keycode.M: return Keys.M;
                case Keycode.N: return Keys.N;
                case Keycode.O: return Keys.O;
                case Keycode.P: return Keys.P;
                case Keycode.Q: return Keys.Q;
                case Keycode.R: return Keys.R;
                case Keycode.S: return Keys.S;
                case Keycode.T: return Keys.T;
                case Keycode.U: return Keys.U;
                case Keycode.V: return Keys.V;
                case Keycode.W: return Keys.W;
                case Keycode.X: return Keys.X;
                case Keycode.Y: return Keys.Y;
                case Keycode.Z: return Keys.Z;
                case Keycode.AltLeft: return Keys.LeftAlt;
                case Keycode.AltRight: return Keys.RightAlt;
                case Keycode.ShiftLeft: return Keys.LeftShift;
                case Keycode.ShiftRight: return Keys.RightShift;
                case Keycode.Enter: return Keys.Enter;
                case Keycode.Back: return Keys.Back;
                case Keycode.Tab: return Keys.Tab;
                case Keycode.Del: return Keys.Delete;
                case Keycode.PageUp: return Keys.PageUp;
                case Keycode.PageDown: return Keys.PageDown;
                case Keycode.DpadUp: return Keys.Up;
                case Keycode.DpadDown: return Keys.Down;
                case Keycode.DpadLeft: return Keys.Right;
                case Keycode.DpadRight: return Keys.Right;
                default:
                    return (Keys)(-1);
            }
        }

        class ViewListener : Java.Lang.Object, View.IOnTouchListener, View.IOnKeyListener
        {
            private readonly InputManager inputManager;

            public ViewListener(InputManager inputManager)
            {
                this.inputManager = inputManager;
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                return inputManager.OnTouch(e);
            }

            public bool OnKey(View v, Keycode keyCode, Android.Views.KeyEvent e)
            {
                return inputManager.OnKey(keyCode, e);
            }
        }

        // No easy way to enable/disable multi-touch on android so we drop them manually in OnTouch function
        public override bool MultiTouchEnabled { get; set; }
    }
}
#endif