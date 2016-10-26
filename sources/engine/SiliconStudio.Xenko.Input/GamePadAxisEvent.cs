// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event to describe a change in a gamepad axis
    /// </summary>
    public class GamePadAxisEvent : EventArgs
    {
        /// <summary>
        /// Index of the axis
        /// </summary>
        public int Index;
        /// <summary>
        /// The new value of the axis
        /// </summary>
        public float Value;
    }
}