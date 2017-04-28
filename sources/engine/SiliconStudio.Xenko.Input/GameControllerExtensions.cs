// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    public static class GameControllerExtensions
    {
        /// <summary>
        /// Gets the number of buttons on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of buttons</returns>
        public static int GetButtonCount(this IGameControllerDevice device)
        {
            return device.ButtonInfos.Count;
        }

        /// <summary>
        /// Gets the number of axes on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of axes</returns>
        public static int GetAxisCount(this IGameControllerDevice device)
        {
            return device.AxisInfos.Count;
        }

        /// <summary>
        /// Gets the number of pov controllers on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of pov controllers</returns>
        public static int GetPovControllersCount(this IGameControllerDevice device)
        {
            return device.PovControllerInfos.Count;
        }

        /// <summary>
        /// Returns the value of a point of view controller converted to a <see cref="GamePadButton"/> which has the matching Pad flags set
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <param name="index">The index of the point of view controller</param>
        /// <returns></returns>
        public static GamePadButton GetDPad(this IGameControllerDevice device, int index)
        {
            if (device.GetPovControllerEnabled(index))
            {
                return GameControllerUtils.PovControllerToButton(device.GetPovController(index));
            }
            return 0;
        }
    }
}