// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_LINUX || SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
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
            var fontSize = new Vector2(fontScale.Y * TextSize); // we don't want letters non-uniform ratio

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
            base.OnKeyPressed(args);

            InterpretKey(args.Key, args.Input);
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
            var character = '\0';
            if (TryConvertKeyToCharacter(key, input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift), ref character))
                SelectedText = new string(character, 1);
        }

        private bool TryConvertKeyToCharacter(Keys key, bool isMajuscule, ref Char character)
        {
            switch (key)
            {
                case Keys.None:
                case Keys.Cancel:
                case Keys.Back:
                case Keys.Tab:
                case Keys.LineFeed:
                case Keys.Clear:
                case Keys.Enter:
                case Keys.Pause:
                case Keys.Capital:
                case Keys.HangulMode:
                case Keys.JunjaMode:
                case Keys.FinalMode:
                case Keys.HanjaMode:
                case Keys.Escape:
                case Keys.ImeConvert:
                case Keys.ImeNonConvert:
                case Keys.ImeAccept:
                case Keys.ImeModeChange:
                case Keys.PageUp:
                case Keys.Next:
                case Keys.Home:
                case Keys.End:
                case Keys.Left:
                case Keys.Up:
                case Keys.Right:
                case Keys.Down:
                case Keys.Select:
                case Keys.Print:
                case Keys.Execute:
                case Keys.PrintScreen:
                case Keys.Insert:
                case Keys.Delete:
                case Keys.Help:
                case Keys.LeftWin:
                case Keys.RightWin:
                case Keys.Apps:
                case Keys.Sleep:
                case Keys.Separator:
                case Keys.Decimal:
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                case Keys.F13:
                case Keys.F14:
                case Keys.F15:
                case Keys.F16:
                case Keys.F17:
                case Keys.F18:
                case Keys.F19:
                case Keys.F20:
                case Keys.F21:
                case Keys.F22:
                case Keys.F23:
                case Keys.F24:
                case Keys.NumLock:
                case Keys.Scroll:
                case Keys.LeftShift:
                case Keys.RightShift:
                case Keys.LeftCtrl:
                case Keys.RightCtrl:
                case Keys.LeftAlt:
                case Keys.RightAlt:
                case Keys.BrowserBack:
                case Keys.BrowserForward:
                case Keys.BrowserRefresh:
                case Keys.BrowserStop:
                case Keys.BrowserSearch:
                case Keys.BrowserFavorites:
                case Keys.BrowserHome:
                case Keys.VolumeMute:
                case Keys.VolumeDown:
                case Keys.VolumeUp:
                case Keys.MediaNextTrack:
                case Keys.MediaPreviousTrack:
                case Keys.MediaStop:
                case Keys.MediaPlayPause:
                case Keys.LaunchMail:
                case Keys.SelectMedia:
                case Keys.LaunchApplication1:
                case Keys.LaunchApplication2:
                case Keys.Oem1:
                case Keys.OemPlus:
                case Keys.OemComma:
                case Keys.OemMinus:
                case Keys.OemPeriod:
                case Keys.Oem2:
                case Keys.Oem3:
                case Keys.Oem4:
                case Keys.Oem5:
                case Keys.Oem6:
                case Keys.Oem7:
                case Keys.Oem8:
                case Keys.Oem102:
                case Keys.Attn:
                case Keys.CrSel:
                case Keys.ExSel:
                case Keys.EraseEof:
                case Keys.Play:
                case Keys.Zoom:
                case Keys.NoName:
                case Keys.Pa1:
                case Keys.OemClear:
                case Keys.NumPadEnter:
                case Keys.NumPadDecimal:
                    return false;
                case Keys.Space:
                    character = ' ';
                    break;
                case Keys.D0:
                    character = '0';
                    break;
                case Keys.D1:
                    character = '1';
                    break;
                case Keys.D2:
                    character = '2';
                    break;
                case Keys.D3:
                    character = '3';
                    break;
                case Keys.D4:
                    character = '4';
                    break;
                case Keys.D5:
                    character = '5';
                    break;
                case Keys.D6:
                    character = '6';
                    break;
                case Keys.D7:
                    character = '7';
                    break;
                case Keys.D8:
                    character = '8';
                    break;
                case Keys.D9:
                    character = '9';
                    break;
                case Keys.A:
                    character = isMajuscule? 'A': 'a';
                    break;
                case Keys.B:
                    character = isMajuscule ? 'B' : 'b';
                    break;
                case Keys.C:
                    character = isMajuscule ? 'C' : 'c';
                    break;
                case Keys.D:
                    character = isMajuscule ? 'D' : 'd';
                    break;
                case Keys.E:
                    character = isMajuscule ? 'E' : 'e';
                    break;
                case Keys.F:
                    character = isMajuscule ? 'F' : 'f';
                    break;
                case Keys.G:
                    character = isMajuscule ? 'G' : 'g';
                    break;
                case Keys.H:
                    character = isMajuscule ? 'H' : 'h';
                    break;
                case Keys.I:
                    character = isMajuscule ? 'I' : 'i';
                    break;
                case Keys.J:
                    character = isMajuscule ? 'J' : 'j';
                    break;
                case Keys.K:
                    character = isMajuscule ? 'K' : 'k';
                    break;
                case Keys.L:
                    character = isMajuscule ? 'L' : 'l';
                    break;
                case Keys.M:
                    character = isMajuscule ? 'M' : 'm';
                    break;
                case Keys.N:
                    character = isMajuscule ? 'N' : 'n';
                    break;
                case Keys.O:
                    character = isMajuscule ? 'O' : 'o';
                    break;
                case Keys.P:
                    character = isMajuscule ? 'P' : 'p';
                    break;
                case Keys.Q:
                    character = isMajuscule ? 'Q' : 'q';
                    break;
                case Keys.R:
                    character = isMajuscule ? 'R' : 'r';
                    break;
                case Keys.S:
                    character = isMajuscule ? 'S' : 's';
                    break;
                case Keys.T:
                    character = isMajuscule ? 'T' : 't';
                    break;
                case Keys.U:
                    character = isMajuscule ? 'U' : 'u';
                    break;
                case Keys.V:
                    character = isMajuscule ? 'V' : 'v';
                    break;
                case Keys.W:
                    character = isMajuscule ? 'W' : 'w';
                    break;
                case Keys.X:
                    character = isMajuscule ? 'X' : 'x';
                    break;
                case Keys.Y:
                    character = isMajuscule ? 'Y' : 'y';
                    break;
                case Keys.Z:
                    character = isMajuscule ? 'Z' : 'z';
                    break;
                case Keys.NumPad0:
                    character = '0';
                    break;
                case Keys.NumPad1:
                    character = '1';
                    break;
                case Keys.NumPad2:
                    character = '2';
                    break;
                case Keys.NumPad3:
                    character = '3';
                    break;
                case Keys.NumPad4:
                    character = '4';
                    break;
                case Keys.NumPad5:
                    character = '5';
                    break;
                case Keys.NumPad6:
                    character = '6';
                    break;
                case Keys.NumPad7:
                    character = '7';
                    break;
                case Keys.NumPad8:
                    character = '8';
                    break;
                case Keys.NumPad9:
                    character = '9';
                    break;
                case Keys.Multiply:
                    character = '*';
                    break;
                case Keys.Add:
                    character = '+';
                    break;
                case Keys.Subtract:
                    character = '-';
                    break;
                case Keys.Divide:
                    character = '/';
                    break;
                default:
                    throw new ArgumentOutOfRangeException("key");
            }

            return true;
        }
    }
}

#endif