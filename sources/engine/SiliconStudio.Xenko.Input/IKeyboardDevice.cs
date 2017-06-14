// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A keyboard device
    /// </summary>
    public interface IKeyboardDevice : IInputDevice
    {
        /// <summary>
        /// The keys that have been pressed since the last frame
        /// </summary>
        IReadOnlySet<Keys> PressedKeys { get; }

        /// <summary>
        /// The keys that have been released since the last frame
        /// </summary>
        IReadOnlySet<Keys> ReleasedKeys { get; }

        /// <summary>
        /// List of keys that are currently down on this keyboard
        /// </summary>
        IReadOnlySet<Keys> DownKeys { get; }
    }
}