// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    public static class GameControllerExtensions
    {
        /// <summary>
        /// Gets the number of buttons on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of buttons</returns>
        public static int GetNumButtons(this IGameControllerDevice device)
        {
            return device.ButtonInfos.Count;
        }

        /// <summary>
        /// Gets the number of axes on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of axes</returns>
        public static int GetNumAxes(this IGameControllerDevice device)
        {
            return device.AxisInfos.Count;
        }

        /// <summary>
        /// Gets the number of pov controllers on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of pov controllers</returns>
        public static int GetNumPovControllers(this IGameControllerDevice device)
        {
            return device.PovControllerInfos.Count;
        }

        /// <summary>
        /// Returns the value of a point of view controller converted to a <see cref="GamePadButton"/> which has the appropriate DPad flags set
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <param name="index">The index of the point of view controller</param>
        /// <returns></returns>
        public static GamePadButton GetDPad(this IGameControllerDevice device, int index)
        {
            if (device.GetPovControllerEnabled(index))
            {
                return GamePadUtils.PovControllerToButton(device.GetPovController(index));
            }
            return 0;
        }

        /// <summary>
        /// Returns the gamepad layout, mapping the gamepad to generic <see cref="GamePadState"/>, or null if there is no mapping
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>A gamepad layout or null</returns>
        public static GamePadLayout GetLayout(this IGameControllerDevice device)
        {
            var pad = device as GameControllerDeviceBase;
            return pad?.Layout;
        }
    }
}