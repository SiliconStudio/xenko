// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides an interface for gamepads that support vibration
    /// </summary>
    public interface IGamePadVibration
    {
        /// <summary>
        /// Sets 4 vibration motors one the device or approximates the effect if less are supported
        /// </summary>
        /// <param name="smallLeft">The small left side motor</param>
        /// <param name="smallRight">The small right side motor</param>
        /// <param name="largeLeft">The large left side motor</param>
        /// <param name="largeRight">The large right side motor</param>
        /// <remarks>Values range from 0 to 1, where 0 is off and 1 is maximum vibration</remarks>
        void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight);
    }
}