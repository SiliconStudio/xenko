// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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