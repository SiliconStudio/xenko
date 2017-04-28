// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A gesture that acts as a button, having a true/false state
    /// </summary>
    public interface IButtonGesture : IInputGesture
    {
        /// <summary>
        /// Raised when the button state has changed
        /// </summary>
        event EventHandler<ButtonGestureEventArgs> Changed;
    }
}