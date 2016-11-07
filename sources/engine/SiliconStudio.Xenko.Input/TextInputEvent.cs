// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Input
{
    public enum TextInputEventType
    {
        /// <summary>
        /// When new text is entered
        /// </summary>
        Input,
        /// <summary>
        /// When the text composition is changed (in the IME)
        /// </summary>
        Composition,
    }

    /// <summary>
    /// Input event used for text input and IME related events
    /// </summary>
    public class TextInputEvent : InputEvent
    {
        /// <summary>
        /// The text that was entered
        /// </summary>
        public string Text;

        /// <summary>
        /// The type of text input event
        /// </summary>
        public TextInputEventType Type;

        /// <summary>
        /// Start of the current composition being edited
        /// </summary>
        public int CompositionStart;

        /// <summary>
        /// Length of the current part of the composition being edited
        /// </summary>
        public int CompositionLength;

        public override string ToString()
        {
            return $"{nameof(Text)}: {Text}, {nameof(Type)}: {Type}, {nameof(CompositionStart)}: {CompositionStart}, {nameof(CompositionLength)}: {CompositionLength}";
        }
    }
}