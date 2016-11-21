// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event for when a <see cref="IGamePadDevice"/>'s index changed
    /// </summary>
    public class GamePadIndexChangedEventArgs : EventArgs
    {
        /// <summary>
        /// New device index
        /// </summary>
        public int Index;

        /// <summary>
        /// if <c>true</c>, this change was initiate by the device
        /// </summary>
        public bool IsDeviceSideChange;
    }
}