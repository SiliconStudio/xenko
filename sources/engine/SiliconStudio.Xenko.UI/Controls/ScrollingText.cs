// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// A text viewer that scrolls automatically the text from right to left.
    /// </summary>
    [DebuggerDisplay("ScrollingText - Name={Name}")]
    public class ScrollingText : TextBlock
    {
        /// <summary>
        /// The key to the ScrollingSpeed dependency property.
        /// </summary>
        protected readonly static PropertyKey<float> ScrollingSpeedPropertyKey = new PropertyKey<float>("ScrollingSpeedKey", typeof(ScrollingText), DefaultValueMetadata.Static(40f), ValidateValueMetadata.New<float>(ValidateScrollingSpeedCallback));

        /// <summary>
        /// The key to the TextWrapped dependency property.
        /// </summary>
        protected readonly static PropertyKey<bool> RepeatTextPropertyKey = new PropertyKey<bool>("TextWrappedKey", typeof(ScrollingText), DefaultValueMetadata.Static(true), ObjectInvalidationMetadata.New<bool>(RepeatTextInvalidationCallback));

        /// <summary>
        /// The key to the DesiredCharacterNumber dependency property.
        /// </summary>
        protected readonly static PropertyKey<uint> DesiredCharacterNumberPropertyKey = new PropertyKey<uint>("DesiredCharacterNumberKey", typeof(ScrollingText), DefaultValueMetadata.Static((uint)10), ObjectInvalidationMetadata.New<uint>(InvalidateCharacterNumber));

        private static void InvalidateCharacterNumber(object propertyOwner, PropertyKey<uint> propertyKey, uint propertyOldValue)
        {
            var element = (UIElement)propertyOwner;
            element.InvalidateMeasure();
        }

        private string textToDisplay = "";

        private float elementWidth;

        /// <summary>
        /// The index in <see cref="Controls.TextBlock.Text"/> defining the position of the next letter to add to <see cref="TextToDisplay"/>.
        /// </summary>
        private int nextLetterIndex;

        private bool textHasBeenAppended;

        /// <summary>
        /// The current offset of the text in the Ox axis.
        /// </summary>
        public float ScrollingOffset { get; private set; }

        /// <summary>
        /// The total accumulated width of the scrolling text since the last call the <see cref="ResetDisplayingText"/>
        /// </summary>
        public float AccumulatedWidth { get; private set; }

        public ScrollingText()
        {
            ResetDisplayingText();
            DrawLayerNumber += 3; // (1: clipping border, 2: Text, 3: clipping border undraw)
        }

        /// <summary>
        /// Gets or sets the scrolling speed of the text. The unit is in virtual pixels.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The provided speed must be positive or null.</exception>
        public float ScrollingSpeed
        {
            get { return DependencyProperties.Get(ScrollingSpeedPropertyKey); }
            set { DependencyProperties.Set(ScrollingSpeedPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the desired number of character in average to display at a given time. This value is taken in account during the measurement stage of the element.
        /// </summary>
        public uint DesiredCharacterNumber
        {
            get { return DependencyProperties.Get(DesiredCharacterNumberPropertyKey); }
            set { DependencyProperties.Set(DesiredCharacterNumberPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the a value indicating if the text message must be repeated (wrapped) or not.
        /// </summary>
        public bool RepeatText
        {
            get { return DependencyProperties.Get(RepeatTextPropertyKey); }
            set { DependencyProperties.Set(RepeatTextPropertyKey, value); }
        }

        private static void ValidateScrollingSpeedCallback(ref float value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("value");
        }

        private static void RepeatTextInvalidationCallback(object propertyOwner, PropertyKey<bool> propertyKey, bool propertyOldValue)
        {
            var scrollingText = (ScrollingText)propertyOwner;
            scrollingText.ResetDisplayingText();
        }

        /// <summary>
        /// Append the provided text to the end of the current <see cref="TextBlock.Text"/> without restarting the display to the begin of the <see cref="TextBlock.Text"/>.
        /// </summary>
        /// <param name="text">The text to append</param>
        public void AppendText(string text)
        {
            if (text == null) throw new ArgumentNullException("text");

            textHasBeenAppended = true;
            Text += text;
        }

        /// <summary>
        /// Clear the currently scrolling text.
        /// </summary>
        public void ClearText()
        {
            Text = "";
        }

        protected override void OnTextChanged()
        {
            if (!textHasBeenAppended) // Text has been modified by the user -> reset scrolling offsets
                ResetDisplayingText();

            textHasBeenAppended = false;
        }

        private void ResetDisplayingText()
        {
            textToDisplay = "";
            nextLetterIndex = 0;
            ScrollingOffset = IsArrangeValid? ActualWidth: float.PositiveInfinity;
            AccumulatedWidth = 0;
        }

        public override string TextToDisplay
        {
            get { return textToDisplay; }
        }
        
        /// <summary>
        /// Calculate the width of the text to display in virtual pixels size.
        /// </summary>
        /// <returns>The size of the text in virtual pixels</returns>
        private float CalculateTextToDisplayWidth()
        {
            return CalculateTextSize(TextToDisplay).X;
        }

        protected override void Update(GameTime time)
        {
            base.Update(time);

            if (!IsEnabled)
                return;

            UpdateAndAdjustDisplayText(time);
        }

        private void UpdateAndAdjustDisplayText(GameTime time = null)
        {
            if (string.IsNullOrEmpty(Text))
                return;

            var elapsedSeconds = time != null ? (float)time.Elapsed.TotalSeconds : 0f;

            // calculate the shift offset
            var nextOffsetShift = elapsedSeconds * ScrollingSpeed - ScrollingOffset;

            // calculate the size of the next TextToDisplay required
            var sizeNextTextToDisplay = nextOffsetShift + elementWidth;

            // append characters to TextToDisplay so that it measures more than 'sizeNextTextToDisplay'
            var textToDisplayWidth = CalculateTextToDisplayWidth();
            while (textToDisplayWidth < sizeNextTextToDisplay && nextLetterIndex < Text.Length)
            {
                textToDisplay += Text[nextLetterIndex++];

                var addedCharacterWidth = CalculateTextToDisplayWidth() - textToDisplayWidth;
                AccumulatedWidth += addedCharacterWidth;

                if (RepeatText && nextLetterIndex >= Text.Length)
                    nextLetterIndex = 0;

                textToDisplayWidth += addedCharacterWidth;
            }

            // Check if all the string has finished to scroll, if clear the message
            if (CalculateTextSize(textToDisplay).X < nextOffsetShift)
                textToDisplay = "";

            // remove characters at the beginning of TextToDisplay as long as possible
            var fontSize = new Vector2(TextSize, TextSize);
            while (textToDisplay.Length > 1 && Font.MeasureString(textToDisplay, ref fontSize, 1).X < nextOffsetShift)
            {
                nextOffsetShift -= Font.MeasureString(textToDisplay, ref fontSize, 1).X;
                textToDisplay = textToDisplay.Substring(1);
            }

            // Update the scroll offset of the viewer
            ScrollingOffset = -nextOffsetShift;
        }

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            return MeasureSize();
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            elementWidth = finalSizeWithoutMargins.X;

            ScrollingOffset = Math.Min(elementWidth, ScrollingOffset);

            UpdateAndAdjustDisplayText();

            return base.ArrangeOverride(finalSizeWithoutMargins);
        }

        /// <summary>
        /// Measure the size of the <see cref="ScrollingText"/> element.
        /// </summary>
        /// <returns>The size of the element</returns>
        public Vector3 MeasureSize()
        {
            if (Font == null)
                return Vector3.Zero;

            return new Vector3(Font.MeasureString(new string('A', (int)DesiredCharacterNumber)), 0);
        }
    }
}
