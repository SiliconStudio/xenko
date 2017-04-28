// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Gesture event arguments
    /// </summary>
    public class GestureEventArgs : EventArgs
    {
        public GestureEventArgs(IInputDevice device)
        {
            Device = device;
        }
        
        /// <summary>
        /// The device that triggered this gesture event
        /// </summary>
        public IInputDevice Device { get; internal set; }
    }
}