// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using Android.Views;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games.Android;

namespace SiliconStudio.Xenko.Input
{
    internal class PointerAndroid : PointerDeviceBase, IDisposable
    {
        private readonly AndroidXenkoGameView uiControl;

        public PointerAndroid(InputSourceAndroid source, AndroidXenkoGameView uiControl)
        {
            Source = source;
            this.uiControl = uiControl;
            var listener = new Listener(this);
            uiControl.Resize += OnResize;
            uiControl.SetOnTouchListener(listener);

            OnResize(this, null);
        }

        public override string Name => "Android Pointer";

        public override Guid Id => new Guid("21370b00-aaf9-4ecf-afb2-575dde6c6c56");

        public override PointerType Type => PointerType.Touch;

        public override IInputSource Source { get; }

        public void Dispose()
        {
            uiControl.Resize -= OnResize;
            uiControl.SetOnTouchListener(null);
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            SetSurfaceSize(new Vector2(uiControl.Size.Width, uiControl.Size.Height));
        }

        protected class Listener : Java.Lang.Object, View.IOnTouchListener
        {
            private readonly PointerAndroid pointer;

            public Listener(PointerAndroid pointer)
            {
                this.pointer = pointer;
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                // Choose action type
                PointerEventType actionType;
                switch (e.ActionMasked)
                {
                    case MotionEventActions.Down:
                    case MotionEventActions.Pointer1Down:
                        actionType = PointerEventType.Pressed;
                        break;

                    case MotionEventActions.Outside:
                    case MotionEventActions.Cancel:
                        actionType = PointerEventType.Canceled;
                        break;

                    case MotionEventActions.Up:
                    case MotionEventActions.Pointer1Up:
                        actionType = PointerEventType.Released;
                        break;

                    default:
                        actionType = PointerEventType.Moved;
                        break;
                }

                // Generate events for each pointer
                for (int i = 0; i < e.PointerCount; i++)
                {
                    var evt = new PointerInputEvent
                    {
                        Id = e.GetPointerId(i),
                        Position = new Vector2(e.GetX(i), e.GetY(i)) * pointer.InverseSurfaceSize,
                        Type = actionType
                    };

                    // Store pointer ID
                    // Normalize
                    pointer.PointerInputEvents.Add(evt);
                }
                
                return true;
            }
        }
    }
}
#endif