// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Base class for button gestures
    /// </summary>
    [DataContract]
    public abstract class ButtonGestureBase : InputGestureBase, IButtonGesture
    {
        private bool lastState;

        public event EventHandler<ButtonGestureEventArgs> Changed;

        protected void UpdateButton(bool newState, IInputDevice sourceDevice)
        {
            if (newState != lastState)
            {
                lastState = newState;
                Changed?.Invoke(this, new ButtonGestureEventArgs(sourceDevice, lastState));
            }
        }
    }
}