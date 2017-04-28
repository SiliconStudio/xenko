// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input.Gestures
{
    public class ButtonGestureEventArgs : GestureEventArgs
    {
        public ButtonGestureEventArgs(IInputDevice device, bool state) : base(device)
        {
            this.State = state;
        }
        
        /// <summary>
        /// The new state of the button gesture
        /// </summary>
        public bool State { get; internal set; }
    }
}