// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event for a button changing state on a device
    /// </summary>
    public abstract class ButtonEvent : InputEvent
    {
        /// <summary>
        /// The new state of the button
        /// </summary>
        public bool IsDown;
    }
}