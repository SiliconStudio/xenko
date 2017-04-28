// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Common interface for gestures that respond to <see cref="IGamePadDevice"/> input
    /// </summary>
    public interface IGamePadGesture
    {
        /// <summary>
        /// The index of the gamepad to watch
        /// </summary>
        int GamePadIndex { get; set; }
    }
}