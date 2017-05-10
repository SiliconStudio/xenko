// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
        /// Sets the gamepad's large and small motors to the given amounts
        /// </summary>
        /// <param name="pad">The gamepad</param>
        /// <param name="largeMotors">The amount of vibration for the large motors</param>
        /// <param name="smallMotors">The amount of vibration for the small motors</param>
        public static void SetVibration(this IGamePadDevice pad, float largeMotors, float smallMotors)
        {
            pad.SetVibration(smallMotors, smallMotors, largeMotors, largeMotors);
        }
    }
}