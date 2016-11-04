// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_UNIX || SILICONSTUDIO_PLATFORM_UWP
using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using TextAlignment = SiliconStudio.Xenko.Graphics.TextAlignment;

namespace SiliconStudio.Xenko.UI.Controls
{
    public partial class EditText
    {
        private void OnTouchMoveImpl(TouchEventArgs args)
        {
            var currentPosition = FindNearestCharacterIndex(new Vector2(args.WorldPosition.X - WorldMatrix.M41, args.WorldPosition.Y - WorldMatrix.M42));

            if (caretAtStart)
            {
                if (currentPosition < selectionStop)
                    Select(currentPosition, selectionStop - currentPosition, true);
                else
                    Select(selectionStop, currentPosition - selectionStop);  
            }
            else
            {
                if (currentPosition < SelectionStart)
                    Select(currentPosition, selectionStart - currentPosition, true);
                else
                    Select(selectionStart, currentPosition - selectionStart);  
            }
        }

        private void OnTouchDownImpl(TouchEventArgs args)
        {
            // Find the appropriate position for the caret.
            CaretPosition = FindNearestCharacterIndex(new Vector2(args.WorldPosition.X - WorldMatrix.M41, args.WorldPosition.Y - WorldMatrix.M42));
        }
        
        /// <summary>
        /// Find the index of the nearest character to the provided position.
        /// </summary>
        /// <param name="position">The position in edit text space</param>
        /// <returns>The 0-based index of the nearest character</returns>
        protected virtual int FindNearestCharacterIndex(Vector2 position)
        {
            if (Font == null)
                return 0;

            var textRegionSize = (ActualWidth - Padding.Left - Padding.Right);
            var fontScale = LayoutingContext.RealVirtualResolutionRatio;
            var fontSize = new Vector2(fontScale.Y * ActualTextSize); // we don't want letters non-uniform ratio

            // calculate the offset of the beginning of the text due to text alignment
            var alignmentOffset = -textRegionSize / 2f;
            if (TextAlignment != TextAlignment.Left)
            {
                var textWidth = Font.MeasureString(TextToDisplay, ref fontSize).X;
                if (Font.FontType == SpriteFontType.Dynamic)
                    textWidth /= fontScale.X;

                alignmentOffset = TextAlignment == TextAlignment.Center ? -textWidth / 2 : -textRegionSize / 2f + (textRegionSize - textWidth);
            }
            var touchInText = position.X - alignmentOffset;

            // Find the first character starting after the click
            var characterIndex = 1;
            var previousCharacterOffset = 0f;
            var currentCharacterOffset = Font.MeasureString(TextToDisplay, ref fontSize, characterIndex).X;
            while (currentCharacterOffset < touchInText && characterIndex < textToDisplay.Length)
            {
                ++characterIndex;
                previousCharacterOffset = currentCharacterOffset;
                currentCharacterOffset = Font.MeasureString(TextToDisplay, ref fontSize, characterIndex).X;
                if (Font.FontType == SpriteFontType.Dynamic)
                    currentCharacterOffset /= fontScale.X;
            }

            // determine the caret position.
            if (touchInText < 0) // click before the start of the text
            {
                return 0;
            }
            if (currentCharacterOffset < touchInText) // click after the end of the text
            {
                return textToDisplay.Length;
            }

            const float Alpha = 0.66f;
            var previousElementRatio = Math.Abs(touchInText - previousCharacterOffset) / Alpha;
            var currentElementRation = Math.Abs(currentCharacterOffset - touchInText) / (1 - Alpha);
            return previousElementRatio < currentElementRation ? characterIndex - 1 : characterIndex;
        }

        internal override void OnKeyPressed(KeyEventArgs args)
        {
            InterpretKey(args.Key, args.Input);
        }

        internal override void OnTextInput(TextEventArgs args)
        {
            // Backspace is already handled by regular keys
            // Also ignore return and tab characters
            if (args.Character == '\b' || args.Character == '\r' || args.Character == '\t')
                return; 

            SelectedText = new string(args.Character, 1);
        }

        private void InterpretKey(Keys key, InputManager input)
        {
            // delete and back space have same behavior when there is a selection 
            if (SelectionLength > 0 && (key == Keys.Delete || key == Keys.Back))
            {
                SelectedText = "";
                return;
            }

            // backspace with caret 
            if (key == Keys.Back)
            {
                selectionStart = Math.Max(0, selectionStart - 1);
                SelectedText = "";
                return;
            }

            // delete with caret
            if (key == Keys.Delete)
            {
                SelectionLength = 1;
                SelectedText = "";
                return;
            }

            // select until home
            if (key == Keys.Home && (input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift)))
            {
                Select(0, selectionStart + SelectionLength, true);
                return;
            }

            // select until end
            if (key == Keys.End && (input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift)))
            {
                Select(selectionStart, Text.Length-selectionStart, false);
                return;
            }

            // move to home
            if (key == Keys.Home)
            {
                CaretPosition = 0;
                return;
            }

            // move to end
            if (key == Keys.End)
            {
                CaretPosition = Text.Length;
                return;
            }

            // select backward 
            if (key == Keys.Left && (input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift)))
            {
                if (caretAtStart || selectionStart == selectionStop)
                    Select(selectionStart - 1, SelectionLength + 1, true);
                else
                    Select(selectionStart, SelectionLength - 1);

                return;
            }

            // select forward
            if (key == Keys.Right && (input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift)))
            {
                if (caretAtStart && selectionStart != selectionStop)
                    Select(selectionStart + 1, SelectionLength - 1, true);
                else
                    Select(selectionStart, SelectionLength + 1);

                return;
            }

            // move backward
            if (key == Keys.Left)
            {
                CaretPosition = CaretPosition - 1;
                return;
            }

            // move forward
            if (key == Keys.Right)
            {
                CaretPosition = CaretPosition + 1;
                return;
            }

            // validate the text with "enter" or "escape"
            if (key == Keys.Enter || key == Keys.Escape)
            {
                IsSelectionActive = false;
                return;
            }

            // try to convert the key to character and insert it at the caret position or replace the current selection
            //var character = '\0';
            //if (TryConvertKeyToCharacter(key, input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift), ref character))
            //    SelectedText = new string(character, 1);
        }
    }
}

#endif
