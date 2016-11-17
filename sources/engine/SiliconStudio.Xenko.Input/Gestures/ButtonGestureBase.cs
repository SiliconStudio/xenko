// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
        private ButtonState lastState;

        public event EventHandler<ButtonGestureEventArgs> Changed;

        protected void UpdateButton(ButtonState newState, IInputDevice sourceDevice)
        {
            if (newState != lastState)
            {
                lastState = newState;
                Changed?.Invoke(this, new ButtonGestureEventArgs(sourceDevice, lastState));
            }
        }
    }
}