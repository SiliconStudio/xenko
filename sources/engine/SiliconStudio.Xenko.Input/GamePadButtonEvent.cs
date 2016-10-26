// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in gamepad button state
    /// </summary>
    public class GamePadButtonEvent : EventArgs
    {
        /// <summary>
        /// The index of the button
        /// </summary>
        public int Index;

        /// <summary>
        /// The new state of the button
        /// </summary>
        public GamePadButtonState State;
    }
}