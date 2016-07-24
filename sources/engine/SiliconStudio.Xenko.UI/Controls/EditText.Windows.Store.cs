// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using System;
using System.Diagnostics;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.UI.Controls
{
    // Note: this completes EditText.Direct.cs
    public partial class EditText
    {
        private static EditText activeEditText;
        private Windows.UI.Xaml.Controls.TextBox editText;
        private GameContextWindowsRuntime gameContext;

        private static void InitializeStaticImpl()
        {
        }

        private void InitializeImpl()
        {
        }

        private int GetLineCountImpl()
        {
            if (Font == null)
                return 1;

            return text.Split('\n').Length;
        }

        private void OnMaxLinesChangedImpl()
        {
        }

        private void OnMinLinesChangedImpl()
        {
        }

        private void ActivateEditTextImpl()
        {
            // Windows Store don't have this method, so let's always use TextBox
#if !SILICONSTUDIO_PLATFORM_WINDOWS_STORE
            var pane = Windows.UI.ViewManagement.InputPane.GetForCurrentView();
            if (pane.TryShow())
#endif
            {
                var game = GetGame();
                if (game == null)
                    throw new ArgumentException("Provided services need to contain a provider for the IGame interface.");

                Debug.Assert(game.Context is GameContextWindowsRuntime, "There is only one possible descendant of GameContext for Windows Store.");

                gameContext = (GameContextWindowsRuntime)game.Context;
                var swapChainPanel = gameContext.Control;

                // Detach previous EditText (if any)
                if (activeEditText != null)
                    activeEditText.IsSelectionActive = false;
                activeEditText = this;

                // Make sure it doesn't have a parent (another text box being edited)
                editText = gameContext.EditTextBox;
                editText.Text = text;
                swapChainPanel.Children.Add(new Windows.UI.Xaml.Controls.StackPanel { Children = { editText } });

                editText.TextChanged += EditText_TextChanged;
                editText.KeyDown += EditText_KeyDown;

                // Focus
                editText.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            }
        }

        private void EditText_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            // validate the text with "enter" or "escape"
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Escape)
            {
                IsSelectionActive = false;
                e.Handled = true;
            }
        }

        private void EditText_TextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            if (editText == null)
                return;

            // early exit if text did not changed 
            if (text == editText.Text)
                return;

            // Make sure selection is not reset
            var editSelectionStart = editText.SelectionStart;

            SetTextInternal(editText.Text, false);
            UpdateTextToEditImpl();

            if (editSelectionStart >= text.Length)
                editSelectionStart = text.Length;
            editText.SelectionStart = editSelectionStart;

            UpdateSelectionFromEditImpl();
        }

        private void DeactivateEditTextImpl()
        {
            if (editText != null)
            {
                // Remove text box
                editText.TextChanged -= EditText_TextChanged;
                editText.KeyDown -= EditText_KeyDown;
                var stackPanel = (Windows.UI.Xaml.Controls.Panel)editText.Parent;
                stackPanel.Children.Remove(editText);
                gameContext.Control.Children.Remove(stackPanel);

                editText = null;
                activeEditText = null;
            }

            FocusedElement = null;
        }

        private void UpdateTextToEditImpl()
        {
            if (editText == null)
                return;

            if (editText.Text != Text) // avoid infinite text changed triggering loop.
            {
                editText.Text = text;
            }
        }

        private void UpdateInputTypeImpl()
        {
        }

        private void UpdateSelectionFromEditImpl()
        {
            if (editText == null)
                return;

            selectionStart = editText.SelectionStart;
            selectionStop = editText.SelectionStart + editText.SelectionLength;
        }

        private void UpdateSelectionToEditImpl()
        {
            if (editText == null)
                return;

            editText.Select(selectionStart, selectionStop - selectionStart);
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
    }
}
#endif