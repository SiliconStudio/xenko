// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input.Gestures
{
    public class ButtonGestureEventArgs : GestureEventArgs
    {
        public ButtonGestureEventArgs(IInputDevice device, ButtonState state) : base(device)
        {
            this.State = state;
        }
        
        public ButtonState State { get; internal set; }
    }
}