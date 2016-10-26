// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in a gamepad point of view controller
    /// </summary>
    public class GamePadPovControllerEvent : EventArgs
    {
        /// <summary>
        /// The index of the pov controller
        /// </summary>
        public int Index;

        /// <summary>
        /// <c>true</c> if the controller is enabled, <c>false</c> if the controller is in a neutral position (disabled)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// The new value of the pov controller
        /// </summary>
        /// <remarks>Goes from 0 to 1 where 0 is up</remarks>
        public float Value;
    }
}