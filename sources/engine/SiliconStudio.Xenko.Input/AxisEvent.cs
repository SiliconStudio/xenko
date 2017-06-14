// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event for an axis changing state on a device
    /// </summary>
    public abstract class AxisEvent : InputEvent
    {
        /// <summary>
        /// The new value of the axis
        /// </summary>
        public float Value;
    }
}