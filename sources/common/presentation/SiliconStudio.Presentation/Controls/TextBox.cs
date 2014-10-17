// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using SiliconStudio.Presentation.Core;

namespace SiliconStudio.Presentation.Controls
{
    public enum TrimmingSource
    {
        Begin,
        Middle,
        End
    }

    /// <summary>
    /// An implementation of the <see cref="System.Windows.Controls.TextBox"/> controls that provides additional features such as a proper
    /// validation/cancellation workflow, and a watermark to display when the text is empty.
    /// </summary>
    [TemplatePart(Name = "PART_TrimmedText", Type = typeof(TextBlock))]
    public class TextBox : System.Windows.Controls.TextBox
    {
        /// <summary>
        /// Identifies the <see cref="HasText"/> dependency property.
        /// </summary>
        private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly("HasText", typeof(bool), typeof(TextBox), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="TrimmedText"/> dependency property.
        /// </summary>
        public static readonly DependencyPropertyKey TrimmedTextPropertyKey = DependencyProperty.RegisterReadOnly("TrimmedText", typeof(string), typeof(TextBox), new PropertyMetadata(""));
        
        private TextBlock trimmedTextBlock; 
        private bool validating;

        /// <summary>
        /// Identifies the <see cref="WatermarkContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkContentProperty = DependencyProperty.Register("WatermarkContent", typeof(object), typeof(TextBox), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="WatermarkContentTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkContentTemplateProperty = DependencyProperty.Register("WatermarkContentTemplate", typeof(DataTemplate), typeof(TextBox), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="TextTrimming"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(TextBox), new PropertyMetadata(TextTrimming.None));

        /// <summary>
        /// Identifies the <see cref="TrimmingSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TrimmingSourceProperty = DependencyProperty.Register("TrimmingSource", typeof(TrimmingSource), typeof(TextBox), new PropertyMetadata(TrimmingSource.End));

        /// <summary>
        /// Identifies the <see cref="TrimmedText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TrimmedTextProperty = TrimmedTextPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the <see cref="GetFocusOnLoad"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GetFocusOnLoadProperty = DependencyProperty.Register("GetFocusOnLoad", typeof(bool), typeof(TextBox), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="SelectAllOnFocus"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.Register("SelectAllOnFocus", typeof(bool), typeof(TextBox), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="ValidateWithEnter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateWithEnterProperty = DependencyProperty.Register("ValidateWithEnter", typeof(bool), typeof(TextBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="ValidateOnTextChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateOnTextChangeProperty = DependencyProperty.Register("ValidateOnTextChange", typeof(bool), typeof(TextBox), new PropertyMetadata(false));
        
        /// <summary>
        /// Identifies the <see cref="ValidateOnLostFocus"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateOnLostFocusProperty = DependencyProperty.Register("ValidateOnLostFocus", typeof(bool), typeof(TextBox), new PropertyMetadata(true, OnLostFocusActionChanged));

        /// <summary>
        /// Identifies the <see cref="CancelWithEscape"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelWithEscapeProperty = DependencyProperty.Register("CancelWithEscape", typeof(bool), typeof(TextBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="CancelOnLostFocus"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelOnLostFocusProperty = DependencyProperty.Register("CancelOnLostFocus", typeof(bool), typeof(TextBox), new PropertyMetadata(false, OnLostFocusActionChanged));

        /// <summary>
        /// Identifies the <see cref="ValidateCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateCommandProperty = DependencyProperty.Register("ValidateCommand", typeof(ICommand), typeof(TextBox));

        /// <summary>
        /// Identifies the <see cref="ValidateCommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateCommandParameterProprty = DependencyProperty.Register("ValidateCommandParameter", typeof(object), typeof(TextBox));

        /// <summary>
        /// Identifies the <see cref="CancelCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(TextBox));

        /// <summary>
        /// Identifies the <see cref="CancelCommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelCommandParameterProprty = DependencyProperty.Register("CancelCommandParameter", typeof(object), typeof(TextBox));

        /// <summary>
        /// Raised just before the TextBox changes are validated. This event is cancellable
        /// </summary>
        public static readonly RoutedEvent ValidatingEvent = EventManager.RegisterRoutedEvent("Validating", RoutingStrategy.Bubble, typeof(CancelRoutedEventHandler), typeof(TextBox));

        /// <summary>
        /// Raised when TextBox changes have been validated.
        /// </summary>
        public static readonly RoutedEvent ValidatedEvent = EventManager.RegisterRoutedEvent("Validated", RoutingStrategy.Bubble, typeof(ValidationRoutedEventHandler<string>), typeof(TextBox));

        /// <summary>
        /// Raised when the TextBox changes are cancelled.
        /// </summary>
        public static readonly RoutedEvent CancelledEvent = EventManager.RegisterRoutedEvent("Cancelled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBox));

        /// <summary>
        /// Clears the current <see cref="System.Windows.Controls.TextBox.Text"/> of a text box.
        /// </summary>
        public static RoutedCommand ClearTextCommand { get; private set; }

        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
            TextProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnTextChanged, null, true, UpdateSourceTrigger.Explicit));
            ClearTextCommand = new RoutedCommand("ClearTextCommand", typeof(System.Windows.Controls.TextBox));
            CommandManager.RegisterClassCommandBinding(typeof(System.Windows.Controls.TextBox), new CommandBinding(ClearTextCommand, OnClearTextCommand));
        }

        public TextBox()
        {
            WordSeparators = " \t";
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Gets whether this TextBox contains a non-empty text.
        /// </summary>
        public bool HasText { get { return (bool)GetValue(HasTextPropertyKey.DependencyProperty); } private set { SetValue(HasTextPropertyKey, value); } }

        /// <summary>
        /// Gets the trimmed text to display when the control does not have the focus, depending of the value of the <see cref="TextTrimming"/> property.
        /// </summary>
        public string TrimmedText { get { return (string)GetValue(TrimmedTextPropertyKey.DependencyProperty); } private set { SetValue(TrimmedTextPropertyKey, value); } }
        
        /// <summary>
        /// Gets or sets the content to display when the TextBox is empty.
        /// </summary>
        public object WatermarkContent { get { return GetValue(WatermarkContentProperty); } set { SetValue(WatermarkContentProperty, value); } }

        /// <summary>
        /// Gets or sets the template of the content to display when the TextBox is empty.
        /// </summary>
        public DataTemplate WatermarkContentTemplate { get { return (DataTemplate)GetValue(WatermarkContentTemplateProperty); } set { SetValue(WatermarkContentTemplateProperty, value); } }

        /// <summary>
        /// Gets or sets how to trim the text when the control does not have the focus.
        /// </summary>
        public TextTrimming TextTrimming { get { return (TextTrimming)GetValue(TextTrimmingProperty); } set { SetValue(TextTrimmingProperty, value); } }

        /// <summary>
        /// Gets or sets the source position of the text trimming when the control does not have the focus.
        /// </summary>
        public TrimmingSource TrimmingSource { get { return (TrimmingSource)GetValue(TrimmingSourceProperty); } set { SetValue(TrimmingSourceProperty, value); } }
 
        /// <summary>
        /// Gets or sets whether the associated text box should get keyboard focus when this behavior is attached.
        /// </summary>
        public bool GetFocusOnLoad { get { return (bool)GetValue(GetFocusOnLoadProperty); } set { SetValue(GetFocusOnLoadProperty, value); } }
        
        /// <summary>
        /// Gets or sets whether the text of the TextBox must be selected when the control gets focus.
        /// </summary>
        public bool SelectAllOnFocus { get { return (bool)GetValue(SelectAllOnFocusProperty); } set { SetValue(SelectAllOnFocusProperty, value); } }

        /// <summary>
        /// Gets or sets whether the validation should happen when the user press <b>Enter</b>.
        /// </summary>
        public bool ValidateWithEnter { get { return (bool)GetValue(ValidateWithEnterProperty); } set { SetValue(ValidateWithEnterProperty, value); } }

        /// <summary>
        /// Gets or sets whether the validation should happen as soon as the <see cref="TextBox.Text"/> is changed.
        /// </summary>
        public bool ValidateOnTextChange { get { return (bool)GetValue(ValidateOnTextChangeProperty); } set { SetValue(ValidateOnTextChangeProperty, value); } }

        /// <summary>
        /// Gets or sets whether the validation should happen when the control losts focus.
        /// </summary>
        public bool ValidateOnLostFocus { get { return (bool)GetValue(ValidateOnLostFocusProperty); } set { SetValue(ValidateOnLostFocusProperty, value); } }

        /// <summary>
        /// Gets or sets whether the cancellation should happen when the user press <b>Escape</b>.
        /// </summary>
        public bool CancelWithEscape { get { return (bool)GetValue(CancelWithEscapeProperty); } set { SetValue(CancelWithEscapeProperty, value); } }

        /// <summary>
        /// Gets or sets whether the cancellation should happen when the control losts focus.
        /// </summary>
        public bool CancelOnLostFocus { get { return (bool)GetValue(CancelOnLostFocusProperty); } set { SetValue(CancelOnLostFocusProperty, value); } }

        /// <summary>
        /// Gets or sets the command to execute when the validation occurs.
        /// </summary>
        public ICommand ValidateCommand { get { return (ICommand)GetValue(ValidateCommandProperty); } set { SetValue(ValidateCommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to execute when the validation occurs.
        /// </summary>
        public object ValidateCommandParameter { get { return GetValue(ValidateCommandParameterProprty); } set { SetValue(ValidateCommandParameterProprty, value); } }

        /// <summary>
        /// Gets or sets the command to execute when the cancellation occurs.
        /// </summary>
        public ICommand CancelCommand { get { return (ICommand)GetValue(CancelCommandProperty); } set { SetValue(CancelCommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to execute when the cancellation occurs.
        /// </summary>
        public object CancelCommandParameter { get { return GetValue(CancelCommandParameterProprty); } set { SetValue(CancelCommandParameterProprty, value); } }

        /// <summary>
        /// Raised just before the TextBox changes are validated. This event is cancellable
        /// </summary>
        public event CancelRoutedEventHandler Validating { add { AddHandler(ValidatingEvent, value); } remove { RemoveHandler(ValidatingEvent, value); } }

        /// <summary>
        /// Raised when TextBox changes have been validated.
        /// </summary>
        public event ValidationRoutedEventHandler<string> Validated { add { AddHandler(ValidatedEvent, value); } remove { RemoveHandler(ValidatedEvent, value); } }

        /// <summary>
        /// Raised when the TextBox changes are cancelled.
        /// </summary>
        public event RoutedEventHandler Cancelled { add { AddHandler(CancelledEvent, value); } remove { RemoveHandler(CancelledEvent, value); } }

        /// <summary>
        /// Gets or sets the charcters used to separate word when computing trimming using <see cref="System.Windows.TextTrimming.WordEllipsis"/>.
        /// </summary>
        public string WordSeparators { get; set; }

        /// <summary>
        /// Validates the current changes in the TextBox.
        /// </summary>
        public void Validate()
        {
            var cancelRoutedEventArgs = new CancelRoutedEventArgs(ValidatingEvent);
            OnValidating(cancelRoutedEventArgs);
            if (cancelRoutedEventArgs.Cancel)
                return;

            RaiseEvent(cancelRoutedEventArgs);
            if (cancelRoutedEventArgs.Cancel)
                return;

            validating = true;
            var coercedText = CoerceTextForValidation(Text);
            SetCurrentValue(TextProperty, coercedText);

            BindingExpression expression = GetBindingExpression(TextProperty);
            if (expression != null)
                expression.UpdateSource();

            ClearUndoStack();

            var validatedArgs = new ValidationRoutedEventArgs<string>(ValidatedEvent, coercedText);
            OnValidated();
            
            RaiseEvent(validatedArgs);
            if (ValidateCommand != null && ValidateCommand.CanExecute(ValidateCommandParameter))
                ValidateCommand.Execute(ValidateCommandParameter);
            validating = false;
        }

        /// <summary>
        /// Cancels the current changes in the TextBox.
        /// </summary>
        public void Cancel()
        {
            BindingExpression expression = GetBindingExpression(TextProperty);
            if (expression != null)
                expression.UpdateTarget();

            ClearUndoStack();

            var cancelledArgs = new RoutedEventArgs(CancelledEvent);
            OnCancelled();
            RaiseEvent(cancelledArgs);

            if (CancelCommand != null && CancelCommand.CanExecute(CancelCommandParameter))
                CancelCommand.Execute(CancelCommandParameter);
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            trimmedTextBlock = GetTemplateChild("PART_TrimmedText") as TextBlock;
            if (trimmedTextBlock == null)
                throw new InvalidOperationException("A part named 'PART_TrimmedText' must be present in the ControlTemplate, and must be of type 'TextBlock'.");
        }

        /// <summary>
        /// Raised when the text of the TextBox changes.
        /// </summary>
        /// <param name="oldValue">The old value of the <see cref="TextBox.Text"/> property.</param>
        /// <param name="newValue">The new value of the <see cref="TextBox.Text"/> property.</param>
        protected virtual void OnTextChanged(string oldValue, string newValue)
        {
        }

        /// <summary>
        /// Raised when the text of the TextBox is being validated.
        /// </summary>
        /// <param name="e">The event argument.</param>
        protected virtual void OnValidating(CancelRoutedEventArgs e)
        {
        }

        /// <summary>
        /// Raised when the current changes have has been validated.
        /// </summary>
        protected virtual void OnValidated()
        {
        }

        /// <summary>
        /// Raised when the current changes have been cancelled.
        /// </summary>
        protected virtual void OnCancelled()
        {
        }

        /// <summary>
        /// Coerces the text during the validation process. This method is invoked by <see cref="Validate"/>.
        /// </summary>
        /// <param name="baseValue">The value to coerce.</param>
        /// <returns>The coerced value.</returns>
        protected virtual string CoerceTextForValidation(string baseValue)
        {
            return MaxLength > 0 && baseValue.Length > MaxLength ? baseValue.Substring(0, MaxLength) : baseValue;
        }

        /// <inheritdoc/>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter && ValidateWithEnter)
            {
                Validate();
            }
            if (e.Key == Key.Escape && CancelWithEscape)
            {
                Cancel();
            }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            if (SelectAllOnFocus)
            {
                SelectAll();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                if (SelectAllOnFocus)
                {
                    // We handle the event only when the SelectAllOnFocus property is active. If we don't handle it, base.OnMouseDown will clear the selection
                    // we're just about to do. But if we handle it, the caret won't be moved to the cursor position, which is the behavior we expect when SelectAllOnFocus is inactive.
                    e.Handled = true;
                }
                Focus();
            }
            base.OnMouseDown(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (ValidateOnLostFocus)
            {
                Validate();
            }
            if (CancelOnLostFocus)
            {
                Cancel();
            }

            base.OnLostKeyboardFocus(e);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var arrangedSize = base.ArrangeOverride(arrangeBounds);
            var availableWidth = arrangeBounds.Width;
            if (trimmedTextBlock != null)
                availableWidth -= trimmedTextBlock.Margin.Left + trimmedTextBlock.Margin.Right;

            PerformEllipsis(availableWidth);
            return arrangedSize;
        }

        private void ClearUndoStack()
        {
            var limit = UndoLimit;
            UndoLimit = 0;
            UndoLimit = limit;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (GetFocusOnLoad)
            {
                Keyboard.Focus(this);
            }
        }

        private void PerformEllipsis(double availableWidth)
        {
            if (TextTrimming == TextTrimming.None)
            {
                TrimmedText = Text;
                return;
            }
            // TODO: optim
            //// Don't process the text if the current available width is the same that the current trimmed text width
            //double epsilon = GetTextWidth(".");
            //if (Math.Abs(availableWidth - trimmedTextWidth) < epsilon)
            //    return arrangedSize;

            double[] sizes;
            var textWidth = GetTextWidth(Text, out sizes);
            if (availableWidth >= textWidth)
            {
                TrimmedText = Text;
                return;
            }

            string[] text;

            switch (TextTrimming)
            {
                case TextTrimming.CharacterEllipsis:
                    text = Text.ToCharArray().Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray();
                    break;
                case TextTrimming.WordEllipsis:
                    text = SplitWords(Text);
                    break;
                default:
                    throw new ArgumentException("Invalid 'TextTrimming' argument.");
            }

            const string Ellipsis = "...";
            if (TrimmingSource == TrimmingSource.Begin)
            {
                var n = text.Length - 1;
                var sb = new StringBuilder();

                //for (var i = 0; i < Offset; i++)
                //    sb.Append(text[i]);
                sb.Append(Ellipsis);

                var starting = sb.ToString();
                sb.Clear();
                var currentWidth = GetTextWidth(starting);

                while (true)
                {
                    var test = currentWidth + sizes[n];

                    if (test > availableWidth)
                        break;

                    sb.Insert(0, text[n--]);
                    currentWidth = test;
                }

                TrimmedText = string.Format("{0}{1}", starting, sb);
            }
            else if (TrimmingSource == TrimmingSource.Middle)
            {
                var currentWidth = GetTextWidth(Ellipsis);
                var n1 = 0;
                var n2 = text.Length - 1;

                var begin = new StringBuilder();
                var end = new StringBuilder();

                while (true)
                {
                    var test = currentWidth + sizes[n1] + sizes[n2];

                    if (test > availableWidth)
                        break;

                    begin.Append(text[n1++]);
                    end.Insert(0, text[n2--]);

                    currentWidth = test;
                }

                TrimmedText = string.Format("{0}{2}{1}", begin, end, Ellipsis);
            }
            else if (TrimmingSource == TrimmingSource.End)
            {
                var n = 0;
                var sb = new StringBuilder();
                // TODO: allow to skip some characters before trimming (ie. keep the extension at the end for instance)
                //for (var i = 0; i < Offset; i++)
                //    sb.Insert(0, text[text.Length - i - 1]);
                sb.Insert(0, Ellipsis);
                var ending = sb.ToString();
                sb.Clear();
                var currentWidth = GetTextWidth(ending);

                while (true)
                {
                    var test = currentWidth + sizes[n];

                    if (test > availableWidth)
                        break;

                    sb.Append(text[n++]);
                    currentWidth = test;
                }

                TrimmedText = string.Format("{0}{1}", sb, ending);
            }
        }

        private double GetTextWidth(string text)
        {
            double[] dummy;
            return GetTextWidth(text, out dummy);
        }

        private double GetTextWidth(string text, out double[] sizes)
        {
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var totalWidth = 0.0;
            // We use a period to ensure space characters will have their actual size used.
            var period = new FormattedText(".", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, FontSize, Brushes.Black);
            double periodWidth = period.Width;

            if (TextTrimming == TextTrimming.CharacterEllipsis)
            {
                sizes = new double[text.Length];

                for (var i = 0; i < text.Length; i++)
                {
                    string token = text[i].ToString(CultureInfo.CurrentUICulture) + ".";
                    var formattedText = new FormattedText(token, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, FontSize, Brushes.Black);
                    double width = formattedText.Width - periodWidth;

                    sizes[i] = width;
                    totalWidth += width;
                }
            }
            else if (TextTrimming == TextTrimming.WordEllipsis)
            {
                var words = SplitWords(text);
                sizes = new double[words.Length];

                for (var i = 0; i < words.Length; i++)
                {
                    string token = words[i] + ".";
                    var formattedText = new FormattedText(token, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, FontSize, Brushes.Black);
                    double width = formattedText.Width - periodWidth;
                    sizes[i] = width;
                    totalWidth += width;
                }
            }
            else
                throw new ArgumentException("Invalid 'TextTrimming' argument.");

            return totalWidth;
        }

        private string[] SplitWords(string text)
        {
            var words = new List<string>();

            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (WordSeparators.Contains(c))
                {
                    if (sb.Length > 0)
                        words.Add(sb.ToString());

                    sb.Clear();

                    words.Add(c.ToString(CultureInfo.InvariantCulture));
                    continue;
                }

                sb.Append(c);
            }

            if (sb.Length > 0)
                words.Add(sb.ToString());

            return words.ToArray();
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var input = (TextBox)d;
            input.HasText = e.NewValue != null && ((string)e.NewValue).Length > 0;
            input.OnTextChanged((string)e.OldValue, (string)e.NewValue);
            if (input.ValidateOnTextChange && !input.validating)
                input.Validate();

            var availableWidth = input.ActualWidth;
            if (input.trimmedTextBlock != null)
                availableWidth -= input.trimmedTextBlock.Margin.Left + input.trimmedTextBlock.Margin.Right;

            input.PerformEllipsis(availableWidth);
        }

        private static void OnLostFocusActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var input = (TextBox)d;
            if (e.Property == ValidateOnLostFocusProperty && (bool)e.NewValue)
            {
                input.SetCurrentValue(CancelOnLostFocusProperty, false);
            }
            if (e.Property == CancelOnLostFocusProperty && (bool)e.NewValue)
            {
                input.SetCurrentValue(ValidateOnLostFocusProperty, false);
            }
        }

        private static void OnClearTextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Clear();
            }
        }

    }
}