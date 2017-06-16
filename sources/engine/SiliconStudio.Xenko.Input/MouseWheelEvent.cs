// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event for a mouse wheel being used
    /// </summary>
    public class MouseWheelEvent : InputEvent
    {
        /// <summary>
        /// The amount the mouse wheel scrolled
        /// </summary>
        public float WheelDelta;

        /// <summary>
        /// The mouse that sent this event
        /// </summary>
        public IMouseDevice Mouse => (IMouseDevice)Device;

        public override string ToString()
        {
            return $"{nameof(WheelDelta)}: {WheelDelta} {nameof(Mouse)}: {Mouse.Name}";
        }
    }
}