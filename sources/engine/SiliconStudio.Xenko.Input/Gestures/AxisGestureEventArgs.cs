// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input.Gestures
{
    public class AxisGestureEventArgs : GestureEventArgs
    {
        public AxisGestureEventArgs(IInputDevice device, float state) : base(device)
        {
            this.State = state;
        }

        public float State { get; internal set; }
    }
}