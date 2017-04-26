// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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