// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A device such as a keyboard that supports text input. This can be a windows keyboard with IME support or a touch keyboard on a smartphone device
    /// </summary>
    public interface ITextInputDevice : IInputDevice
    {
        /// <summary>
        /// Allows input to be entered, the input device will then send text events through the input manager
        /// </summary>
        void EnabledTextInput();
        
        /// <summary>
        /// Disallows text input to be entered, will close any IME active and stop sending text events
        /// </summary>
        void DisableTextInput();
    }
}