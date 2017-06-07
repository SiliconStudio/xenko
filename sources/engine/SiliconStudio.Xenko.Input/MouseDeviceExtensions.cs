// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    public static class MouseDeviceExtensions
    {
        /// <summary>
        /// Determines whether the specified mouse button is pressed since the previous update.
        /// </summary>
        /// <param name="mouseDevice">The mnouse</param>
        /// <param name="mouseButton">The mouse button</param>
        /// <returns><c>true</c> if the specified mouse button is pressed since the previous update; otherwise, <c>false</c>.</returns>
        public static bool IsButtonPressed(this IMouseDevice mouseDevice, MouseButton mouseButton)
        {
            return mouseDevice.PressedButtons.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is released.
        /// </summary>
        /// <param name="mouseDevice">The mnouse</param>
        /// <param name="mouseButton">The mouse button</param>
        /// <returns><c>true</c> if the specified mouse button is released; otherwise, <c>false</c>.</returns>
        public static bool IsButtonReleased(this IMouseDevice mouseDevice, MouseButton mouseButton)
        {
            return mouseDevice.ReleasedButtons.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified button is being pressed down
        /// </summary>
        /// <param name="mouseDevice">The mnouse</param>
        /// <param name="mouseButton">The mouse button</param>
        /// <returns><c>true</c> if the specified button is being pressed down; otherwise, <c>false</c>.</returns>
        public static bool IsButtonDown(this IMouseDevice mouseDevice, MouseButton mouseButton)
        {
            return mouseDevice.DownButtons.Contains(mouseButton);
        }
    }
}
