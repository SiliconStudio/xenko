// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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