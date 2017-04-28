// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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