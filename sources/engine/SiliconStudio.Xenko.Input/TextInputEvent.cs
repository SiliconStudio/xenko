// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Input event used for text input and IME related events
    /// </summary>
    public class TextInputEvent : InputEvent
    {
        public TextInputEvent(IKeyboardDevice device) : base(device)
        {
        }

        /// <summary>
        /// The character that was entered
        /// </summary>
        public char Character;

        public override string ToString()
        {
            return $"{nameof(Character)}: {Character}";
        }
    }
}