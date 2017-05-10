// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A keyboard device
    /// </summary>
    public interface IKeyboardDevice : IInputDevice
    {
        /// <summary>
        /// List of keys that are currently down on this keyboard
        /// </summary>
        IReadOnlyList<Keys> DownKeys { get; }

        /// <summary>
        /// Determines whether the specified key is being pressed down
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        bool IsKeyDown(Keys key);
    }
}