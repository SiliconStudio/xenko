// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event that was generated from an <see cref="IInputDevice"/>
    /// </summary>
    public abstract class InputEvent : EventArgs
    {
        /// <summary>
        /// Creates a new input event
        /// </summary>
        /// <param name="device">The device that produces this event</param>
        protected InputEvent(IInputDevice device)
        {
            Device = device;
        }
        
        /// <summary>
        /// The device that sent this event
        /// </summary>
        public IInputDevice Device { get; }
    }
}