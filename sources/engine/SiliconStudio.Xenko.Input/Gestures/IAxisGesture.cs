// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A gesture that acts as an axis, having a positive or negative float value
    /// </summary>
    public interface IAxisGesture : IInputGesture
    {
        /// <summary>
        /// Raised when the axis state has changed
        /// </summary>
        event EventHandler<AxisGestureEventArgs> Changed;
    }
}