// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Common interface for gestures that respond to <see cref="IGameControllerDevice"/> input
    /// </summary>
    public interface IGameControllerGesture
    {
        /// <summary>
        /// The index of the gamepad to watch
        /// </summary>
        Guid ControllerId { get; set; }
    }
}