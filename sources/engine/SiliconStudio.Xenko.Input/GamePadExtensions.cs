// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    public static class GamePadExtensions
    {
        /// <summary>
        /// Gets the number of buttons on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of buttons</returns>
        public static int GetNumButtons(this IGamePadDevice device)
        {
            return device.ButtonInfos.Count;
        }

        /// <summary>
        /// Gets the number of axes on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of axes</returns>
        public static int GetNumAxes(this IGamePadDevice device)
        {
            return device.AxisInfos.Count;
        }

        /// <summary>
        /// Gets the number of pov controllers on this gamepad
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <returns>The number of pov controllers</returns>
        public static int GetNumPovControllers(this IGamePadDevice device)
        {
            return device.PovControllerInfos.Count;
        }

        /// <summary>
        /// Returns the value of a point of view controller converted to a <see cref="GamePadButton"/> which has the appropriate DPad flags set
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <param name="index">The index of the point of view controller</param>
        /// <returns></returns>
        public static GamePadButton GetDPad(this IGamePadDevice device, int index)
        {
            if (device.GetPovControllerEnabled(index))
            {
                return GamePadConversions.PovControllerToButton(device.GetPovController(index));
            }
            return 0;
        }

        /// <summary>
        /// Returns the value of an axis as a trigger (going from 0 to 1)
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <param name="index">the index of the axis</param>
        /// <returns></returns>
        public static float GetTrigger(this IGamePadDevice device, int index)
        {
            return device.GetAxis(index)*0.5f + 0.5f;
        }
    }
}