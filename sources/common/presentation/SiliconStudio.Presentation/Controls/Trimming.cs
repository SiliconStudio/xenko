// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Controls
{
    public static class Trimming
    {
        /// <summary>
        /// The string used as ellipsis for trimming.
        /// </summary>
        public const string Ellipsis = "…";

        /// <summary>
        /// Identifies the <see cref="TextTrimming"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.RegisterAttached("TextTrimming", typeof(TextTrimming), typeof(Trimming), new PropertyMetadata(TextTrimming.None));

        /// <summary>
        /// Identifies the <see cref="TrimmingSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TrimmingSourceProperty = DependencyProperty.RegisterAttached("TrimmingSource", typeof(TrimmingSource), typeof(Trimming), new PropertyMetadata(TrimmingSource.End));

        /// <summary>
        /// Identifies the <see cref="WordSeparators"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WordSeparatorsProperty = DependencyProperty.RegisterAttached("WordSeparators", typeof(string), typeof(Trimming), new PropertyMetadata(" \t"));

        /// <summary>
        /// Gets the current value of the <see cref="TextTrimming"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <returns>The value of the <see cref="TextTrimming"/> dependency property.</returns>
        public static TextTrimming GetTextTrimming(DependencyObject target)
        {
            return (TextTrimming)target.GetValue(TextTrimmingProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="TextTrimming"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="value">The value to set.</param>
        public static void SetTextTrimming(DependencyObject target, TextTrimming value)
        {
            target.SetValue(TextTrimmingProperty, value);
        }

        /// <summary>
        /// Gets the current value of the <see cref="TrimmingSource"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <returns>The value of the <see cref="TrimmingSource"/> dependency property.</returns>
        public static TrimmingSource GetTrimmingSource(DependencyObject target)
        {
            return (TrimmingSource)target.GetValue(TrimmingSourceProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="TrimmingSource"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="value">The value to set.</param>
        public static void SetTrimmingSource(DependencyObject target, TrimmingSource value)
        {
            target.SetValue(TrimmingSourceProperty, value);
        }

        /// <summary>
        /// Gets the current value of the <see cref="WordSeparators"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <returns>The value of the <see cref="WordSeparators"/> dependency property.</returns>
        public static string GetWordSeparators(DependencyObject target)
        {
            return (string)target.GetValue(WordSeparatorsProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="WordSeparators"/> dependency property attached to the given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="value">The value to set.</param>
        public static void SetWordSeparators(DependencyObject target, string value)
        {
            target.SetValue(WordSeparatorsProperty, value);
        }

        public static string ProcessTrimming(Control control, string text, double availableWidth)
        {
            var trimming = GetTextTrimming(control);
            var source = GetTrimmingSource(control);
            var wordSeparators = GetWordSeparators(control);

            if (trimming == TextTrimming.None)
            {
                return text;
            }
            // TODO: optim
            //// Don't process the text if the current available width is the same that the current trimmed text width
            //double epsilon = GetTextWidth(".");
            //if (Math.Abs(availableWidth - trimmedTextWidth) < epsilon)
            //    return arrangedSize;

            double[] sizes;
            var textWidth = GetTextWidth(control, text, trimming, out sizes);
            if (availableWidth >= textWidth)
            {
                return text;
            }

            List<string> words;

            switch (trimming)
            {
                case TextTrimming.CharacterEllipsis:
                    words = text.ToCharArray().Select(c => c.ToString(CultureInfo.InvariantCulture)).ToList();
                    break;
                case TextTrimming.WordEllipsis:
                    words = SplitWords(text, wordSeparators);
                    break;
                default:
                    throw new ArgumentException("Invalid 'TextTrimming' argument.");
            }

            switch (source)
            {
                case TrimmingSource.Begin:
                {
                    var n = words.Count - 1;
                    var sb = new StringBuilder();

                    //for (var i = 0; i < Offset; i++)
                    //    sb.Append(text[i]);
                    sb.Append(Ellipsis);

                    var starting = sb.ToString();
                    sb.Clear();
                    var currentWidth = GetTextWidth(control, starting, trimming);

                    for (; ; )
                    {
                        var test = currentWidth + sizes[n];

                        if (test > availableWidth)
                            break;

                        sb.Insert(0, words[n--]);
                        currentWidth = test;
                    }

                    return string.Format("{0}{1}", starting, sb);
                }
                case TrimmingSource.Middle:
                {
                    var currentWidth = GetTextWidth(control, Ellipsis, trimming);
                    var n1 = 0;
                    var n2 = words.Count - 1;

                    var begin = new StringBuilder();
                    var end = new StringBuilder();

                    while (true)
                    {
                        var test = currentWidth + sizes[n1] + sizes[n2];

                        if (test > availableWidth)
                            break;

                        begin.Append(words[n1++]);
                        end.Insert(0, words[n2--]);

                        currentWidth = test;
                    }

                    return string.Format("{0}{2}{1}", begin, end, Ellipsis);
                }
                case TrimmingSource.End:
                {
                    var n = 0;
                    var sb = new StringBuilder();
                    // TODO: allow to skip some characters before trimming (ie. keep the extension at the end for instance)
                    //for (var i = 0; i < Offset; i++)
                    //    sb.Insert(0, text[text.Length - i - 1]);
                    sb.Insert(0, Ellipsis);
                    var ending = sb.ToString();
                    sb.Clear();
                    var currentWidth = GetTextWidth(control, ending, trimming);

                    while (true)
                    {
                        var test = currentWidth + sizes[n];

                        if (test > availableWidth)
                            break;

                        sb.Append(words[n++]);
                        currentWidth = test;
                    }

                    return string.Format("{0}{1}", sb, ending);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static double GetTextWidth(Control control, string text, TextTrimming trimming)
        {
            double[] dummy;
            return GetTextWidth(control, text, trimming, out dummy);
        }

        private static double GetTextWidth(Control control, string text, TextTrimming trimming, out double[] sizes)
        {
            var wordSeparators = GetWordSeparators(control);

            var typeface = new Typeface(control.FontFamily, control.FontStyle, control.FontWeight, control.FontStretch);
            var totalWidth = 0.0;
            // We use a period to ensure space characters will have their actual size used.
            var period = new FormattedText(".", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, control.FontSize, Brushes.Black);
            double periodWidth = period.Width;

            switch (trimming)
            {
                case TextTrimming.CharacterEllipsis:
                    sizes = new double[text.Length];
                    for (var i = 0; i < text.Length; i++)
                    {
                        string token = text[i].ToString(CultureInfo.CurrentUICulture) + ".";
                        var formattedText = new FormattedText(token, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, control.FontSize, Brushes.Black);
                        double width = formattedText.Width - periodWidth;
                        sizes[i] = width;
                        totalWidth += width;
                    }
                    return totalWidth;
                case TextTrimming.WordEllipsis:
                    var words = SplitWords(text, wordSeparators);
                    sizes = new double[words.Count];
                    for (var i = 0; i < words.Count; i++)
                    {
                        string token = words[i] + ".";
                        var formattedText = new FormattedText(token, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, control.FontSize, Brushes.Black);
                        double width = formattedText.Width - periodWidth;
                        sizes[i] = width;
                        totalWidth += width;
                    }
                    return totalWidth;
                default:
                    throw new ArgumentOutOfRangeException("trimming");
            }
        }

        private static List<string> SplitWords(string text, string wordSeparators)
        {
            var words = new List<string>();

            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (wordSeparators.Contains(c))
                {
                    // Ignore empty entries (ie. double consecutive separators)
                    if (sb.Length > 0)
                    {
                        // Add the current word to the list
                        words.Add(sb.ToString());
                    }

                    // Reset the string builder for the next word
                    sb.Clear();

                    // Add the separator itself, it is still needed in the list
                    words.Add(c.ToString(CultureInfo.CurrentUICulture));
                }
                else
                {
                    // If the current character in not a separator, simply append it to the current word
                    sb.Append(c);
                }
            }

            // We reached the end of the text, add the current word to the list.
            if (sb.Length > 0)
                words.Add(sb.ToString());

            return words;
        }
    }
}