// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event for a button changing state on a device
    /// </summary>
    public abstract class ButtonEvent : InputEvent
    {
        /// <summary>
        /// Creates a new button event
        /// </summary>
        /// <param name="device">The device that produces this event</param>
        protected ButtonEvent(IInputDevice device) : base(device)
        {
        }

        /// <summary>
        /// The state of the button
        /// </summary>
        public ButtonState State;
    }
}