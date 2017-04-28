// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using Android.Views;
using SiliconStudio.Xenko.Games.Android;
using Keycode = Android.Views.Keycode;

namespace SiliconStudio.Xenko.Input
{
    internal class KeyboardAndroid : KeyboardDeviceBase, IDisposable
    {
        private readonly AndroidXenkoGameView gameView;

        public KeyboardAndroid(InputSourceAndroid source, AndroidXenkoGameView gameView)
        {
            Source = source;
            this.gameView = gameView;
            var listener = new Listener(this);
            gameView.SetOnKeyListener(listener);
        }

        public override string Name => "Android Keyboard";

        public override Guid Id => new Guid("98468e4a-2895-4f87-b750-5ffe2dd943ae");

        public override IInputSource Source { get; }

        public void Dispose()
        {
            gameView.SetOnKeyListener(null);
        }

        protected class Listener : Java.Lang.Object, View.IOnKeyListener
        {
            private readonly KeyboardAndroid keyboard;

            public Listener(KeyboardAndroid keyboard)
            {
                this.keyboard = keyboard;
            }

            public bool OnKey(View v, Keycode keyCode, Android.Views.KeyEvent e)
            {
                var xenkoKey = ConvertKeyFromAndroid(keyCode);

                if (e.Action == KeyEventActions.Down)
                {
                    keyboard.HandleKeyDown(xenkoKey);
                }
                else
                {
                    keyboard.HandleKeyUp(xenkoKey);
                }

                return true;
            }

            private Keys ConvertKeyFromAndroid(Keycode key)
            {
                switch (key)
                {
                    case Keycode.Num0:
                        return Keys.D0;
                    case Keycode.Num1:
                        return Keys.D1;
                    case Keycode.Num2:
                        return Keys.D2;
                    case Keycode.Num3:
                        return Keys.D3;
                    case Keycode.Num4:
                        return Keys.D4;
                    case Keycode.Num5:
                        return Keys.D5;
                    case Keycode.Num6:
                        return Keys.D6;
                    case Keycode.Num7:
                        return Keys.D7;
                    case Keycode.Num8:
                        return Keys.D8;
                    case Keycode.Num9:
                        return Keys.D9;
                    case Keycode.A:
                        return Keys.A;
                    case Keycode.B:
                        return Keys.B;
                    case Keycode.C:
                        return Keys.C;
                    case Keycode.D:
                        return Keys.D;
                    case Keycode.E:
                        return Keys.E;
                    case Keycode.F:
                        return Keys.F;
                    case Keycode.G:
                        return Keys.G;
                    case Keycode.H:
                        return Keys.H;
                    case Keycode.I:
                        return Keys.I;
                    case Keycode.J:
                        return Keys.J;
                    case Keycode.K:
                        return Keys.K;
                    case Keycode.L:
                        return Keys.L;
                    case Keycode.M:
                        return Keys.M;
                    case Keycode.N:
                        return Keys.N;
                    case Keycode.O:
                        return Keys.O;
                    case Keycode.P:
                        return Keys.P;
                    case Keycode.Q:
                        return Keys.Q;
                    case Keycode.R:
                        return Keys.R;
                    case Keycode.S:
                        return Keys.S;
                    case Keycode.T:
                        return Keys.T;
                    case Keycode.U:
                        return Keys.U;
                    case Keycode.V:
                        return Keys.V;
                    case Keycode.W:
                        return Keys.W;
                    case Keycode.X:
                        return Keys.X;
                    case Keycode.Y:
                        return Keys.Y;
                    case Keycode.Z:
                        return Keys.Z;
                    case Keycode.AltLeft:
                        return Keys.LeftAlt;
                    case Keycode.AltRight:
                        return Keys.RightAlt;
                    case Keycode.ShiftLeft:
                        return Keys.LeftShift;
                    case Keycode.ShiftRight:
                        return Keys.RightShift;
                    case Keycode.Enter:
                        return Keys.Enter;
                    case Keycode.Back:
                        return Keys.Back;
                    case Keycode.Tab:
                        return Keys.Tab;
                    case Keycode.Del:
                        return Keys.Delete;
                    case Keycode.PageUp:
                        return Keys.PageUp;
                    case Keycode.PageDown:
                        return Keys.PageDown;
                    case Keycode.DpadUp:
                        return Keys.Up;
                    case Keycode.DpadDown:
                        return Keys.Down;
                    case Keycode.DpadLeft:
                        return Keys.Right;
                    case Keycode.DpadRight:
                        return Keys.Right;
                    default:
                        return (Keys)(-1);
                }
            }
        }
    }
}

#endif