// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Controls
{
    public enum TrimmingSource
    {
        Begin,
        Middle,
        End
    }

    /// <summary>
    /// An implementation of the <see cref="TextBoxBase"/> control that provides additional features such as a proper
    /// validation/cancellation workflow, and a watermark to display when the text is empty.
    /// </summary>
    [TemplatePart(Name = "PART_TrimmedText", Type = typeof(TextBlock))]
    public class TextBox : TextBoxBase
    {
        private TextBlock trimmedTextBlock;

        /// <summary>
        /// Identifies the <see cref="TrimmedText"/> dependency property.
        /// </summary>
        public static readonly DependencyPropertyKey TrimmedTextPropertyKey = DependencyProperty.RegisterReadOnly("TrimmedText", typeof(string), typeof(TextBox), new PropertyMetadata(""));

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
        /// Clears the current <see cref="System.Windows.Controls.TextBox.Text"/> of a text box.
        /// </summary>
        public static RoutedCommand ClearTextCommand { get; private set; }
        
        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
            ClearTextCommand = new RoutedCommand("ClearTextCommand", typeof(System.Windows.Controls.TextBox));
            CommandManager.RegisterClassCommandBinding(typeof(System.Windows.Controls.TextBox), new CommandBinding(ClearTextCommand, OnClearTextCommand));
        }

        public TextBox()
        {
            WordSeparators = " \t";
        }

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
        /// Gets or sets the charcters used to separate word when computing trimming using <see cref="System.Windows.TextTrimming.WordEllipsis"/>.
        /// </summary>
        public string WordSeparators { get; set; }

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
        protected override void OnTextChanged(string oldValue, string newValue)
        {
            var availableWidth = ActualWidth;
            if (trimmedTextBlock != null)
                availableWidth -= trimmedTextBlock.Margin.Left + trimmedTextBlock.Margin.Right;

            PerformEllipsis(availableWidth);
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

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var arrangedSize = base.ArrangeOverride(arrangeBounds);
            var availableWidth = arrangeBounds.Width;
            if (trimmedTextBlock != null)
                availableWidth -= trimmedTextBlock.Margin.Left + trimmedTextBlock.Margin.Right;

            PerformEllipsis(availableWidth);
            return arrangedSize;
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