// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Arguments for input pre update event
    /// </summary>
    public class InputPreUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// The game time passed to <see cref="InputManager.Update"/>
        /// </summary>
        public GameTime GameTime;
    }
}