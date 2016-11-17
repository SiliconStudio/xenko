// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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