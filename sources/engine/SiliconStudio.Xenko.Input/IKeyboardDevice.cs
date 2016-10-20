// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A keyboard device
    /// </summary>
    public interface IKeyboardDevice : IInputDevice
    {
        /// <summary>
        /// Raised when a key is pressed/released on this keyboard
        /// </summary>
        EventHandler<KeyEvent> OnKey { get; set; }

        /// <summary>
        /// Determines whether the specified key is being pressed down
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        bool IsKeyDown(Keys key);

        // TODO: IME, text input (shift key combinations, etc.), smartphone keyboard visiblity toggle
    }
}