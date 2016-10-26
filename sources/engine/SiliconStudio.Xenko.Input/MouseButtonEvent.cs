// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes a button on a mouse changing state
    /// </summary>
    public class MouseButtonEvent : ButtonEvent
    {
        /// <summary>
        /// Creates a new mouse button event
        /// </summary>
        /// <param name="device">The mouse</param>
        public MouseButtonEvent(IMouseDevice device) : base(device)
        {
        }

        /// <summary>
        /// The button that changed state
        /// </summary>
        public MouseButton Button;

        /// <summary>
        /// The mouse that sent this event
        /// </summary>
        public IMouseDevice Mouse => Device as IMouseDevice;

        public override string ToString()
        {
            return $"{nameof(Button)}: {Button}, {nameof(State)}: {State}, {nameof(Mouse)}: {Mouse.DeviceName}";
        }
    }
}