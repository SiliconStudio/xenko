// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event for a keyboard button changing state
    /// </summary>
    public class KeyEvent : ButtonEvent
    {
        /// <summary>
        /// Creates a new key event
        /// </summary>
        /// <param name="keyboard">The keyboard that produces this event</param>
        public KeyEvent(IKeyboardDevice keyboard) : base(keyboard)
        {
        }

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
        public IKeyboardDevice Keyboard => Device as IKeyboardDevice;

        public override string ToString()
        {
            return $"{nameof(Key)}: {Key}, {nameof(RepeatCount)}: {RepeatCount}, {nameof(Keyboard)}: {Keyboard}";
        }
    }
}