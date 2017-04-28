// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event for a keyboard button changing state
    /// </summary>
    public class KeyEvent : ButtonEvent
    {
        /// <summary>
        /// The key that is being pressed or released.
        /// </summary>
        public Keys Key;

        /// <summary>
        /// The repeat count for this key. If it is 0 this is the initial press of the key
        /// </summary>
        public int RepeatCount;

        /// <summary>
        /// The keyboard that sent this event
        /// </summary>
        public IKeyboardDevice Keyboard => (IKeyboardDevice)Device;

        public override string ToString()
        {
            return $"{nameof(Key)}: {Key}, {nameof(State)}: {State}, {nameof(RepeatCount)}: {RepeatCount}, {nameof(Keyboard)}: {Keyboard}";
        }
    }
}