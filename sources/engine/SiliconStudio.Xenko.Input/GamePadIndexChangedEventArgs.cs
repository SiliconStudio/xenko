// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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