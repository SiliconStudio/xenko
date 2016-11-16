// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides easier ways to set vibration levels on a controller, rather than setting 4 motors
    /// </summary>
    public static class GamePadExtensions
    {
        /// <summary>
        /// Sets all the gamepad vibration motors to the same amount
        /// </summary>
        /// <param name="pad">The gamepad</param>
        /// <param name="amount">The amount of vibration</param>
        public static void SetVibration(this IGamePadDevice pad, float amount)
        {
            pad.SetVibration(amount, amount, amount, amount);
        }

        /// <summary>
        /// Sets all the gamepad left and right motors to the given amounts
        /// </summary>
        /// <param name="pad">The gamepad</param>
        /// <param name="leftMotors">The amount of vibration for the left side</param>
        /// <param name="rightMotors">The amount of vibration for the right side</param>
        public static void SetVibration(this IGamePadDevice pad, float leftMotors, float rightMotors)
        {
            pad.SetVibration(leftMotors, rightMotors, leftMotors, rightMotors);
        }
    }
}