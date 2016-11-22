// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using Android.Views;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games.Android;

namespace SiliconStudio.Xenko.Input
{
    internal class PointerAndroid : PointerDeviceBase, IDisposable
    {
        private AndroidXenkoGameView uiControl;
        private Listener listener;

        public PointerAndroid(AndroidXenkoGameView uiControl)
        {
            this.uiControl = uiControl;
            listener = new Listener(this);
            uiControl.Resize += OnResize;
            uiControl.SetOnTouchListener(listener);

            OnResize(this, null);
        }

        public void Dispose()
        {
            uiControl.Resize -= OnResize;
            uiControl.SetOnTouchListener(null);
        }

        public override string DeviceName => "Android Pointer";
        public override Guid Id => new Guid("21370b00-aaf9-4ecf-afb2-575dde6c6c56");
        public override PointerType Type => PointerType.Touch;

        private void OnResize(object sender, EventArgs eventArgs)
        {
            SetSurfaceSize(new Vector2(uiControl.Size.Width, uiControl.Size.Height));
        }

        protected class Listener : Java.Lang.Object, View.IOnTouchListener
        {
            private PointerAndroid pointer;
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
                    case MotionEventActions.Canceled:
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
                    var evt = new PointerInputEvent();
                    // Store pointer ID
                    evt.Id = e.GetPointerId(i);
                    evt.Position = new Vector2(e.GetX(i), e.GetY(i)) * pointer.InverseSurfaceSize; // Normalize
                    evt.Type = actionType;
                    pointer.PointerInputEvents.Add(evt);
                }
                
                return true;
            }
        }
    }
}
#endif