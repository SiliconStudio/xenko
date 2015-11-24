// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    [TemplatePart(Name = EditableTextBoxPartName, Type = typeof(TextBox))]
    [TemplatePart(Name = ListBoxPartName, Type = typeof(ListBox))]
    public class SuggestionComboBox : Selector
    {
        /// <summary>
        /// The name of the part for the <see cref="TextBox"/>.
        /// </summary>
        private const string EditableTextBoxPartName = "PART_EditableTextBox";
        /// <summary>
        /// The name of the part for the <see cref="ListBox"/>.
        /// </summary>
        private const string ListBoxPartName = "PART_ListBox";

        /// <summary>
        /// Identifies the <see cref="ClearTextAfterValidation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ClearTextAfterValidationProperty =
            DependencyProperty.Register("ClearTextAfterValidation", typeof(bool), typeof(SuggestionComboBox));
        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(SuggestionComboBox));
        /// <summary>
        /// Identifies the <see cref="OpenDropDownOnFocus"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OpenDropDownOnFocusProperty =
            DependencyProperty.Register("OpenDropDownOnFocus", typeof(bool), typeof(SuggestionComboBox));
        /// <summary>
        /// Identifies the <see cref="RequireSelectedItemToValidate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RequireSelectedItemToValidateProperty =
            DependencyProperty.Register("RequireSelectedItemToValidate", typeof(bool), typeof(SuggestionComboBox));
        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SuggestionComboBox), new FrameworkPropertyMetadata { /*DefaultUpdateSourceTrigger = UpdateSourceTrigger.Explicit,*/ BindsTwoWayByDefault = true });
        /// <summary>
        /// Identifies the <see cref="WatermarkContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkContentProperty =
            DependencyProperty.Register("WatermarkContent", typeof(object), typeof(SuggestionComboBox));

        /// <summary>
        /// The input text box.
        /// </summary>
        private TextBox editableTextBox;
        /// <summary>
        /// The suggestion list box.
        /// </summary>
        private ListBox listBox;
        /// <summary>
        /// Indicates that the user clicked in the listbox with the mouse and that the drop down should not be opened.
        /// </summary>
        private bool listBoxClicking;
        /// <summary>
        /// Indicates that the selection is being internally updated and that the text should not be cleared.
        /// </summary>
        private bool updatingSelection;
        
        static SuggestionComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SuggestionComboBox), new FrameworkPropertyMetadata(typeof(SuggestionComboBox)));
        }

        public SuggestionComboBox()
        {
            IsTextSearchEnabled = false;
        }

        /// <summary>
        /// Gets or sets whether to clear the text after the validation.
        /// </summary>
        public bool ClearTextAfterValidation { get { return (bool)GetValue(ClearTextAfterValidationProperty); } set { SetValue(ClearTextAfterValidationProperty, value); } }
        /// <summary>
        /// Gets or sets whether to open the dropdown when the control got the focus.
        /// </summary>
        public bool OpenDropDownOnFocus { get { return (bool)GetValue(OpenDropDownOnFocusProperty); } set { SetValue(OpenDropDownOnFocusProperty, value); } }
        /// <summary>
        /// Gets or sets whether the drop down is open.
        /// </summary>
        public bool IsDropDownOpen { get { return (bool)GetValue(IsDropDownOpenProperty); } set { SetValue(IsDropDownOpenProperty, value); } }
        /// <summary>
        /// Gets or sets whether the validation will be cancelled if <see cref="Selector.SelectedItem"/> is null.
        /// </summary>
        public bool RequireSelectedItemToValidate { get { return (bool)GetValue(RequireSelectedItemToValidateProperty); } set { SetValue(RequireSelectedItemToValidateProperty, value); } }
        /// <summary>
        /// Gets or sets the text of this <see cref="SuggestionComboBox"/>
        /// </summary>
        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }
        /// <summary>
        /// Gets or sets the content to display when the TextBox is empty.
        /// </summary>
        public object WatermarkContent { get { return GetValue(WatermarkContentProperty); } set { SetValue(WatermarkContentProperty, value); } }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            editableTextBox = GetTemplateChild(EditableTextBoxPartName) as TextBox;
            if (editableTextBox == null)
                throw new InvalidOperationException($"A part named '{EditableTextBoxPartName}' must be present in the ControlTemplate, and must be of type '{typeof(TextBox).FullName}'.");

            listBox = GetTemplateChild(ListBoxPartName) as ListBox;
            if (listBox == null)
                throw new InvalidOperationException($"A part named '{ListBoxPartName}' must be present in the ControlTemplate, and must be of type '{nameof(ListBox)}'.");
            
            editableTextBox.PreviewKeyDown += EditableTextBoxPreviewKeyDown;
            listBox.MouseUp += ListBoxMouseUp;
        }
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (OpenDropDownOnFocus && !listBoxClicking)
            {
                IsDropDownOpen = true;
            }
            listBoxClicking = false;
        }

        private void EditableTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (listBox.Items.Count <= 0)
                return;

            updatingSelection = true;
            var stackPanel = listBox.FindVisualChildOfType<VirtualizingStackPanel>();
            switch (e.Key)
            {
                case Key.Escape:
                    if (IsDropDownOpen)
                    {
                        IsDropDownOpen = false;
                        if (RequireSelectedItemToValidate)
                            editableTextBox.Cancel();
                    }
                    else
                    {
                        editableTextBox.Cancel();
                    }
                    break;

                case Key.Up:
                    listBox.SelectedIndex = Math.Max(listBox.SelectedIndex - 1, 0);
                    BringSelectedItemIntoView();
                    break;

                case Key.Down:
                    listBox.SelectedIndex = Math.Min(listBox.SelectedIndex + 1, listBox.Items.Count - 1);
                    BringSelectedItemIntoView();
                    break;

                case Key.PageUp:
                    if (stackPanel != null)
                    {
                        var count = stackPanel.Children.Count;
                        listBox.SelectedIndex = Math.Max(listBox.SelectedIndex - count, 0);
                    }
                    else
                    {
                        listBox.SelectedIndex = 0;
                    }
                    BringSelectedItemIntoView();
                    break;

                case Key.PageDown:
                    if (stackPanel != null)
                    {
                        var count = stackPanel.Children.Count;
                        listBox.SelectedIndex = Math.Min(listBox.SelectedIndex + count, listBox.Items.Count - 1);
                    }
                    else
                    {
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                    }
                    BringSelectedItemIntoView();
                    break;

                case Key.Home:
                    listBox.SelectedIndex = 0;
                    BringSelectedItemIntoView();
                    break;

                case Key.End:
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                    BringSelectedItemIntoView();
                    break;
            }
            updatingSelection = false;
        }

        private void ListBoxMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && listBox.SelectedIndex > -1)
            {
                // We need to force the validation here
                // The user might have clicked on the list after the drop down was automatically open (see OpenDropDownOnFocus).
                editableTextBox.ForceValidate();
            }
            listBoxClicking = true;
        }

        private void BringSelectedItemIntoView()
        {
            var selectedItem = listBox.SelectedItem;
            if (selectedItem != null)
            {
                listBox.ScrollIntoView(selectedItem);
            }
        }
    }
}
