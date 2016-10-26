// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event for a mouse wheel being used
    /// </summary>
    public class MouseWheelEvent : InputEvent
    {
        /// <summary>
        /// Creates a new mouse wheel event
        /// </summary>
        /// <param name="device">The device that produces this event</param>
        public MouseWheelEvent(IMouseDevice device) : base(device)
        {
        }

        /// <summary>
        /// The amount the mouse wheel scrolled
        /// TODO: Normalize this for all platforms, SDL generates -120/120 and winforms generates -1/1 for a single unit
        /// </summary>
        public int WheelDelta;

        /// <summary>
        /// The mouse that sent this event
        /// </summary>
        public IMouseDevice Mouse => Device as IMouseDevice;

        public override string ToString()
        {
            return $"{nameof(WheelDelta)}: {WheelDelta} {nameof(Mouse)}: {Mouse.DeviceName}";
        }
    }
}