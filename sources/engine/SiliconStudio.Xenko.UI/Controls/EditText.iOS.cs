// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using System;
using System.Drawing;
using Foundation;
using UIKit;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.UI.Events;
using System.Diagnostics;

namespace SiliconStudio.Xenko.UI.Controls
{
    public partial class EditText
    {
        private UITextField attachedTextField;

        private static UIButton doneButton;
        private static UITextField textField;
        private static EditText currentActiveEditText;
        private static UIView barView;
        private static UIView overlayView;
        private static GameContextiOS gameContext;

        private static void InitializeStaticImpl()
        {
            doneButton = UIButton.FromType(UIButtonType.RoundedRect);
            doneButton.SetTitle(NSBundle.MainBundle.LocalizedString("UIDoneButton", null), UIControlState.Normal);
            doneButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            doneButton.TouchDown += DoneButtonOnTouchDown;

            textField = new UITextField
            {
                KeyboardType = UIKeyboardType.Default,
                BorderStyle = UITextBorderStyle.RoundedRect,
            };
            textField.EditingDidEnd += TextFieldOnEditingDidEnd;
            textField.EditingDidBegin += TextFieldOnEditingDidBegin;
            
            barView = new UIView { Hidden = true };
            barView.AddSubview(textField);
            barView.AddSubview(doneButton);
            barView.BackgroundColor = UIColor.Gray;

            overlayView = new UIView { Hidden = true };
            overlayView.AddSubview(barView);
            overlayView.BackgroundColor = new UIColor(0,0,0,0.4f);
        }

        internal GameBase GetGame()
        {
            if (UIElementServices.Services == null)
                throw new InvalidOperationException("services");

            var game = UIElementServices.Services.GetService(typeof(IGame)) as GameBase;
            if (game == null)
                throw new ArgumentException("Provided services need to contain a provider for the IGame interface.");

            return game;
        }

        private static void TextFieldOnEditingDidBegin(object sender, EventArgs eventArgs)
        {
            overlayView.Hidden = false;
            barView.Hidden = false;

            if (currentActiveEditText != null)
            {
                // we need to skip some draw calls here to let the time to iOS to draw its own keyboard animations... (Thank you iOS)
                // If we don't do this when changing the type of keyboard (split / docked / undocked), the keyboard freeze for about 5/10 seconds before updating.
                // Note: Setting UIView.EnableAnimation to false does not solve the problem. Only animation when the keyboard appear/disappear are skipped.
                currentActiveEditText.GetGame().SlowDownDrawCalls = true;
            }
        }

        private void InitializeImpl()
        {
            if (gameContext == null)
            {
                var game = GetGame();
                if (game == null)
                    throw new ArgumentException("Provided services need to contain a provider for the IGame interface.");

                Debug.Assert(game.Context is GameContextiOS, "There is only one possible descendant of GameContext for iOS.");

                gameContext = (GameContextiOS)game.Context;
                gameContext.Control.GameView.AddSubview(overlayView);

                NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, OnScreenRotated);

                UpdateOverlayAndEditBarLayout();
            }
        }

        private void OnScreenRotated(NSNotification nsNotification)
        {
            if (gameContext == null)
                return;

            UpdateOverlayAndEditBarLayout();
        }

        private static void UpdateOverlayAndEditBarLayout()
        {
            const int spaceX = 10;
            const int spaceY = 5;
            const int buttonWidth = 60;
            const int buttonHeight = 35;
            const int barHeight = buttonHeight + 2*spaceY;

            var viewFrame = gameContext.Control.GameView.Frame;

            barView.Frame = new RectangleF(0, 0, (int)viewFrame.Width, barHeight);
            overlayView.Frame = new RectangleF((int)viewFrame.X, (int)viewFrame.Y, 2 * (int)viewFrame.Width, (int)viewFrame.Height); // if we don't over-set width background can be seen during rotation...
            textField.Frame = new RectangleF(spaceX, spaceY, (int)viewFrame.Width - buttonWidth - 3 * spaceX, buttonHeight);
            doneButton.Frame = new RectangleF((int)viewFrame.Width - buttonWidth - spaceX, spaceY, buttonWidth, buttonHeight);
        }

        private static void TextFieldOnEditingDidEnd(object sender, EventArgs eventArgs)
        {
            currentActiveEditText.IsSelectionActive = false;
            barView.Hidden = true;
            overlayView.Hidden = true;
            FocusedElement = null;

            if (currentActiveEditText != null)
            {
                // Editing finished, we can now draw back to normal frame rate.
                currentActiveEditText.GetGame().SlowDownDrawCalls = false;
            }
        }

        private static void DoneButtonOnTouchDown(object sender, EventArgs eventArgs)
        {
            currentActiveEditText.IsSelectionActive = false;
        }

        private void TextFieldOnValueChanged(object sender, EventArgs eventArgs)
        {
            if (attachedTextField == null)
                return;

            // early exit if text did not changed 
            if (text == attachedTextField.Text)
                return;

            text = attachedTextField.Text;
            UpdateTextToDisplay();

            RaiseEvent(new RoutedEventArgs(TextChangedEvent));
            InvalidateMeasure();
        }

        private int GetLineCountImpl()
        {
            return 1;
        }

        private void OnMaxLinesChangedImpl()
        {
        }

        private void OnMinLinesChangedImpl()
        {
        }

        private void ActivateEditTextImpl()
        {
            currentActiveEditText = this;
            attachedTextField = textField;

            UpdateInputTypeImpl();
            attachedTextField.Text = text;
            attachedTextField.EditingChanged += TextFieldOnValueChanged;
            attachedTextField.ShouldChangeCharacters += ShouldChangeCharacters;
            attachedTextField.BecomeFirstResponder();
        }

        private bool ShouldChangeCharacters(UITextField theTextField, NSRange range, string replacementString)
        {
            // check that new characters are correct.
            var predicate = CharacterFilterPredicate;
            foreach (var character in replacementString)
            {
                if (predicate != null && !predicate(character))
                    return false;
            }

            var replacementSize = replacementString.Length - range.Length;
            return replacementSize < 0 || theTextField.Text.Length + replacementSize <= MaxLength;
        }

        private void DeactivateEditTextImpl()
        {
            attachedTextField.EditingChanged -= TextFieldOnValueChanged;
            attachedTextField.ShouldChangeCharacters -= ShouldChangeCharacters;
            attachedTextField.SecureTextEntry = false;
            attachedTextField.ResignFirstResponder();
            attachedTextField = null;
            currentActiveEditText = null;
        }

        private void OnTouchMoveImpl(TouchEventArgs args)
        {
        }

        private void OnTouchDownImpl(TouchEventArgs args)
        {
        }

        private void UpdateInputTypeImpl()
        {
            if (attachedTextField == null)
                return;

            attachedTextField.SecureTextEntry = ShouldHideText;
        }

        private void UpdateSelectionToEditImpl()
        {
            if (attachedTextField == null)
                return;

            attachedTextField.SelectedTextRange = attachedTextField.GetTextRange(
                attachedTextField.GetPosition(attachedTextField.BeginningOfDocument, selectionStart),
                attachedTextField.GetPosition(attachedTextField.BeginningOfDocument, selectionStop));
        }

        private void UpdateSelectionFromEditImpl()
        {
            if (attachedTextField == null)
                return;

            selectionStart = (int)attachedTextField.GetOffsetFromPosition(attachedTextField.BeginningOfDocument, attachedTextField.SelectedTextRange.Start);
            selectionStop = (int)attachedTextField.GetOffsetFromPosition(attachedTextField.BeginningOfDocument, attachedTextField.SelectedTextRange.End);
        }

        private void UpdateTextToEditImpl()
        {
            if (attachedTextField == null)
                return;
            
            // update the iOS text edit only the text changed to avoid re-triggering events.
            if (Text != attachedTextField.Text)
                attachedTextField.Text = text;
        }
    }
}

#endif
