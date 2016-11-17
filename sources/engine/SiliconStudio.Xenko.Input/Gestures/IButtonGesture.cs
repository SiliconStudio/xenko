// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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