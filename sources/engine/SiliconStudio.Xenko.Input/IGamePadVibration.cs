// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A gamepad that supports vibration
    /// </summary>
    public interface IGamePadVibration
    {
        /// <summary>
        /// Sets the 4 vibration motors or approximates the effect if less are supported
        /// </summary>
        /// <param name="smallLeft">The small left side motor</param>
        /// <param name="smallRight">The small right side motor</param>
        /// <param name="largeLeft">The large left side motor</param>
        /// <param name="largeRight">The large right side motor</param>
        void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight);
    }
}