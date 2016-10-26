// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// The types of gamepad axes
    /// </summary>
    public enum GamePadAxisType
    {
        /// <summary>
        /// An axis that describes a relative position from it's standard position
        /// </summary>
        RelativeAxis,
        /// <summary>
        /// An axis that describes the running total by continuously applying it's relative position
        /// </summary>
        AbsoluteAxis
    }
}