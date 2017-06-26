// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    public static class KeyboardDeviceExtensions
    {
        /// <summary>
        /// Determines whether the specified key is pressed since the previous update.
        /// </summary>
        /// <param name="keyboardDevice">The keyboard</param>
        /// <param name="key">The key</param>
        /// <returns><c>true</c> if the specified key is pressed; otherwise, <c>false</c>.</returns>
        public static bool IsKeyPressed(this IKeyboardDevice keyboardDevice, Keys key)
        {
            return keyboardDevice.PressedKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is released since the previous update.
        /// </summary>
        /// <param name="keyboardDevice">The keyboard</param>
        /// <param name="key">The key</param>
        /// <returns><c>true</c> if the specified key is released; otherwise, <c>false</c>.</returns>
        public static bool IsKeyReleased(this IKeyboardDevice keyboardDevice, Keys key)
        {
            return keyboardDevice.ReleasedKeys.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is being pressed down
        /// </summary>
        /// <param name="keyboardDevice">The keyboard</param>
        /// <param name="key">The key</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        public static bool IsKeyDown(this IKeyboardDevice keyboardDevice, Keys key)
        {
            return keyboardDevice.DownKeys.Contains(key);
        }
    }
}