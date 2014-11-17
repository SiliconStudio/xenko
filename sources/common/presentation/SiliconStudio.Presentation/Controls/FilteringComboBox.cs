// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    [TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
    public class FilteringComboBox : Selector
    {
        public class FilteringComboBoxSort : IComparer
        {
            private string token;
            private string tokenLowercase;

            public string Token { get { return token; } set { token = value; tokenLowercase = (value ?? "").ToLowerInvariant(); } }

            public int Compare(object x, object y)
            {
                var a = x.ToString();
                var b = y.ToString();

                if (string.IsNullOrWhiteSpace(token))
                    return string.Compare(a, b, StringComparison.InvariantCultureIgnoreCase);

                var indexA = a.IndexOf(tokenLowercase, StringComparison.InvariantCultureIgnoreCase);
                var indexB = b.IndexOf(tokenLowercase, StringComparison.InvariantCultureIgnoreCase);

                if (indexA == 0 && indexB > 0)
                    return -1;
                if (indexB == 0 && indexA > 0)
                    return 1;

                return string.Compare(a, b, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// The instance of <see cref="FilteringComboBoxSort"/> used for filtering and sorting items.
        /// </summary>
        private readonly FilteringComboBoxSort sort;

        /// <summary>
        /// The input text box.
        /// </summary>
        private TextBox editableTextBox;

        /// <summary>
        /// The filtered list box.
        /// </summary>
        private ListBox listBox;
        /// <summary>
        /// Indicates that the selection is being internally cleared and that the drop down should not be opened nor refreshed.
        /// </summary>
        /// 
        private bool clearing;
        /// <summary>
        /// Indicates that the selection is being internally updated and that the text should not be cleared.
        /// </summary>
        private bool updatingSelection;

        /// <summary>
        /// Indicates that the text box is being validated and that the update of the text should not impact the selected item.
        /// </summary>
        private bool validating;

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(FilteringComboBox));

        public static readonly DependencyProperty ClearTextAfterValidationProperty = DependencyProperty.Register("ClearTextAfterValidation", typeof(bool), typeof(FilteringComboBox));

        /// <summary>
        /// Identifies the <see cref="WatermarkContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkContentProperty = DependencyProperty.Register("WatermarkContent", typeof(object), typeof(FilteringComboBox), new PropertyMetadata(null));

        public static readonly DependencyProperty ItemsToExcludeProperty = DependencyProperty.Register("ItemsToExclude", typeof(IEnumerable), typeof(FilteringComboBox));

        /// <summary>
        /// Raised just before the TextBox changes are validated. This event is cancellable
        /// </summary>
        public static readonly RoutedEvent ValidatingEvent = EventManager.RegisterRoutedEvent("Validating", RoutingStrategy.Bubble, typeof(CancelRoutedEventHandler), typeof(FilteringComboBox));

        /// <summary>
        /// Raised when TextBox changes have been validated.
        /// </summary>
        public static readonly RoutedEvent ValidatedEvent = EventManager.RegisterRoutedEvent("Validated", RoutingStrategy.Bubble, typeof(ValidationRoutedEventHandler<string>), typeof(FilteringComboBox));

        public FilteringComboBox()
        {
            sort = new FilteringComboBoxSort();
            IsTextSearchEnabled = false;
        }

        public bool IsDropDownOpen { get { return (bool)GetValue(IsDropDownOpenProperty); } set { SetValue(IsDropDownOpenProperty, value); } }

        public bool ClearTextAfterValidation { get { return (bool)GetValue(ClearTextAfterValidationProperty); } set { SetValue(ClearTextAfterValidationProperty, value); } }

        /// <summary>
        /// Gets or sets the content to display when the TextBox is empty.
        /// </summary>
        public object WatermarkContent { get { return GetValue(WatermarkContentProperty); } set { SetValue(WatermarkContentProperty, value); } }

        public IEnumerable ItemsToExclude { get { return (IEnumerable)GetValue(ItemsToExcludeProperty); } set { SetValue(ItemsToExcludeProperty, value); } }

        /// <summary>
        /// Raised just before the TextBox changes are validated. This event is cancellable
        /// </summary>
        public event CancelRoutedEventHandler Validating { add { AddHandler(ValidatingEvent, value); } remove { RemoveHandler(ValidatingEvent, value); } }

        /// <summary>
        /// Raised when TextBox changes have been validated.
        /// </summary>
        public event ValidationRoutedEventHandler<string> Validated { add { AddHandler(ValidatedEvent, value); } remove { RemoveHandler(ValidatedEvent, value); } }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            if (newValue != null)
            {
                var cvs = (CollectionView)CollectionViewSource.GetDefaultView(newValue);
                cvs.Filter = Filter;
                var listCollectionView = cvs as ListCollectionView;
                if (listCollectionView != null)
                {
                    listCollectionView.CustomSort = sort;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            editableTextBox = GetTemplateChild("PART_EditableTextBox") as TextBox;
            if (editableTextBox == null)
                throw new InvalidOperationException("A part named 'PART_EditableTextBox' must be present in the ControlTemplate, and must be of type 'SiliconStudio.Presentation.Controls.Input.TextBox'.");

            listBox = GetTemplateChild("PART_ListBox") as ListBox;
            if (listBox == null)
                throw new InvalidOperationException("A part named 'PART_ListBox' must be present in the ControlTemplate, and must be of type 'ListBox'.");

            editableTextBox.TextChanged += EditableTextBoxTextChanged;
            editableTextBox.PreviewKeyDown += EditableTextBoxPreviewKeyDown;
            editableTextBox.Validating += EditableTextBoxValidating;
            editableTextBox.Validated += EditableTextBoxValidated;
            editableTextBox.Cancelled += EditableTextBoxCancelled;
            editableTextBox.LostFocus += EditableTextBoxLostFocus;
            listBox.PreviewMouseUp += ListBoxMouseUp;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (SelectedItem == null && !updatingSelection)
            {
                clearing = true;
                editableTextBox.Clear();
                clearing = false;
            }
        }

        private void UpdateText()
        {
            if (listBox.SelectedItem != null)
            {
                editableTextBox.Text = listBox.SelectedItem.ToString();
                IsDropDownOpen = false;
            }
        }

        private void EditableTextBoxValidating(object sender, CancelRoutedEventArgs e)
        {
            // This may happens somehow when the template is refreshed.
            if (!ReferenceEquals(sender, editableTextBox))
                return;

            validating = true;
            UpdateText();
            validating = false;

            var cancelRoutedEventArgs = new CancelRoutedEventArgs(ValidatingEvent);
            RaiseEvent(cancelRoutedEventArgs);
            if (cancelRoutedEventArgs.Cancel)
                e.Cancel = true;
        }

        private void EditableTextBoxValidated(object sender, ValidationRoutedEventArgs<string> e)
        {
            // This may happens somehow when the template is refreshed.
            if (!ReferenceEquals(sender, editableTextBox))
                return;

            var validatedArgs = new RoutedEventArgs(ValidatedEvent);
            RaiseEvent(validatedArgs);

            if (ClearTextAfterValidation)
            {
                clearing = true;
                editableTextBox.Text = string.Empty;
                clearing = false;
            }
        }

        private async void EditableTextBoxCancelled(object sender, RoutedEventArgs e)
        {
            // This may happens somehow when the template is refreshed.
            if (!ReferenceEquals(sender, editableTextBox))
                return;
            
            clearing = true;
            editableTextBox.Text = string.Empty;
            // Defer closing the popup in case we lost the focus because of a click in the list box - so it can still raise the correct event
            // This is a very hackish, we should find a better way to do it!
            await Task.Delay(100);
            IsDropDownOpen = false;
            clearing = false;
        }

        private async void EditableTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            // This may happens somehow when the template is refreshed.
            if (!ReferenceEquals(sender, editableTextBox))
                return;
            
            clearing = true;
            // Defer closing the popup in case we lost the focus because of a click in the list box - so it can still raise the correct event
            // This is a very hackish, we should find a better way to do it!
            await Task.Delay(100);
            IsDropDownOpen = false;
            clearing = false;
        }

        private void ListBoxMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && listBox.SelectedIndex > -1)
            {
                UpdateText();
                editableTextBox.Validate();
            }
        }

        private void EditableTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ItemsSource == null)
                return;

            updatingSelection = true;
            if (!IsDropDownOpen && !clearing)
            {
                // Setting IsDropDownOpen to true will select all the text. We don't want this behavior, so let's save and restore the caret index.
                var index = editableTextBox.CaretIndex;
                IsDropDownOpen = true;
                editableTextBox.CaretIndex = index;
            }
            sort.Token = editableTextBox.Text;
            var cvs = CollectionViewSource.GetDefaultView(ItemsSource);
            cvs.Refresh();
            if (listBox.Items.Count > 0 && !validating)
            {
                listBox.SelectedIndex = 0;
            }
            updatingSelection = false;
        }

        private void EditableTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (listBox.Items.Count > 0)
            {
                updatingSelection = true;
                if (e.Key == Key.Escape)
                {
                    IsDropDownOpen = false;
                }
                if (e.Key == Key.Up)
                {
                    listBox.SelectedIndex = Math.Max(listBox.SelectedIndex - 1, 0);
                    if (listBox.SelectedItem != null)
                        listBox.ScrollIntoView(listBox.SelectedItem);
                }
                if (e.Key == Key.Down)
                {
                    listBox.SelectedIndex = Math.Min(listBox.SelectedIndex + 1, listBox.Items.Count - 1);
                    if (listBox.SelectedItem != null)
                        listBox.ScrollIntoView(listBox.SelectedItem);
                }
                if (e.Key == Key.PageUp)
                {
                    var stackPanel = listBox.FindVisualChildOfType<VirtualizingStackPanel>();
                    if (stackPanel != null)
                    {
                        var count = stackPanel.Children.Count;
                        listBox.SelectedIndex = Math.Max(listBox.SelectedIndex - count, 0);
                    }
                    else
                    {
                        listBox.SelectedIndex = 0;
                    }
                    if (listBox.SelectedItem != null)
                        listBox.ScrollIntoView(listBox.SelectedItem);
                }
                if (e.Key == Key.PageDown)
                {
                    var stackPanel = listBox.FindVisualChildOfType<VirtualizingStackPanel>();
                    if (stackPanel != null)
                    {
                        var count = stackPanel.Children.Count;
                        listBox.SelectedIndex = Math.Min(listBox.SelectedIndex + count, listBox.Items.Count - 1);
                    }
                    else
                    {
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                    }
                    if (listBox.SelectedItem != null)
                        listBox.ScrollIntoView(listBox.SelectedItem);
                }
                if (e.Key == Key.Home)
                {
                    listBox.SelectedIndex = 0;
                }
                if (e.Key == Key.End)
                {
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                }
                updatingSelection = false;
            }
        }

        private bool Filter(object obj)
        {
            if (editableTextBox == null)
                return true;

            var filter = editableTextBox.Text;
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            if (obj == null)
                return false;

            if (ItemsToExclude != null && ItemsToExclude.Cast<object>().Contains(obj))
                return false;

            var text = obj.ToString();
            return text.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) > -1 || MatchCamelCase(text);
        }

        private bool MatchCamelCase(string text)
        {
            var camelCaseSplit = text.CamelCaseSplit();
            var filter = editableTextBox.Text.ToLowerInvariant();
            int currentFilterChar = 0;

            foreach (var word in camelCaseSplit)
            {
                int currentWordChar = 0;
                while (currentFilterChar > 0)
                {
                    if (char.ToLower(word[currentWordChar]) == filter[currentFilterChar])
                        break;
                    --currentFilterChar;
                }

                while (char.ToLower(word[currentWordChar]) == filter[currentFilterChar])
                {
                    ++currentWordChar;
                    ++currentFilterChar;
                    if (currentFilterChar == filter.Length)
                        return true;

                    if (currentWordChar == word.Length)
                        break;
                }
            }
            return currentFilterChar == filter.Length;
        }
    }
}
