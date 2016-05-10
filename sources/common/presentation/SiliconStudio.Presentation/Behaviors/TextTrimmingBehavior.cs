// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using SiliconStudio.Presentation.Controls;

namespace SiliconStudio.Presentation.Behaviors
{
    [Obsolete("This behavior is obsolete. Use the TextBox control instead.")]
    public class TextTrimmingBehavior : DeferredBehaviorBase<TextBlock>
    {
        private FrameworkElement parent;
        private string originalText;

        public int Offset { get; set; }

        private string wordSeparators;
        private string processedWordSeparators;
        public string WordSeparators
        {
            get { return wordSeparators; }
            set
            {
                wordSeparators = value;
                ProcessWordSeparators();
            }
        }

        private void ProcessWordSeparators()
        {
            processedWordSeparators = Regex.Unescape(wordSeparators);
        }

        public TrimmingSource TrimmingSource { get; set; }
        public TextTrimming TextTrimming { get; set; }

        public double ContentMargin { get; set; }

        private double trimmedTextWidth = double.MinValue;

        public TextTrimmingBehavior()
        {
            WordSeparators = " ";
        }

        protected override void OnAttachedAndLoaded()
        {
            parent = VisualTreeHelper.GetParent(AssociatedObject) as FrameworkElement;
            if (parent == null)
            {
                parent = LogicalTreeHelper.GetParent(AssociatedObject) as FrameworkElement;
                if (parent == null)
                    return;
            }

            originalText = AssociatedObject.Text;
            parent.SizeChanged += OnLogicalParentSizeChanged;

            ProcessText();
        }

        protected override void OnDetachingAndUnloaded()
        {
            if (parent != null)
                parent.SizeChanged -= OnLogicalParentSizeChanged;
        }

        private void OnLogicalParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProcessText();
        }

        private void ProcessText()
        {
            if (TextTrimming == TextTrimming.None)
                return;

            double[] sizes;

            var textWidth = GetTextWidth(AssociatedObject, originalText, out sizes);
            var availableWidth = parent.ActualWidth - ContentMargin;

            // Don't process the text if the current available width is the same that the current trimmed text width
            double epsilon = GetTextWidth(AssociatedObject, ".");
            if (Math.Abs(availableWidth - trimmedTextWidth) < epsilon)
                return;

            if (availableWidth >= textWidth)
                AssociatedObject.Text = originalText;
            else
            {
                if (TextTrimming == TextTrimming.CharacterEllipsis)
                    PerformEllipsis(sizes, originalText.ToCharArray().Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray(), availableWidth);
                else if (TextTrimming == TextTrimming.WordEllipsis)
                    PerformEllipsis(sizes, Split(originalText), availableWidth);
                else
                    throw new ArgumentException("Invalid 'TextTrimming' argument.");
            }
        }

        private void PerformEllipsis(double[] sizes, string[] text, double availableWidth)
        {
            if (TrimmingSource == TrimmingSource.Begin)
            {
                var n = text.Length - 1;
                var sb = new StringBuilder();

                for (var i = 0; i < Offset; i++)
                    sb.Append(text[i]);
                sb.Append("...");

                var starting = sb.ToString();
                sb.Clear();
                var currentWidth = GetTextWidth(AssociatedObject, starting);

                while (true)
                {
                    var test = currentWidth + sizes[n];

                    if (test > availableWidth)
                        break;

                    sb.Insert(0, text[n--]);
                    currentWidth = test;
                }

                trimmedTextWidth = currentWidth;
                AssociatedObject.Text = string.Format("{0}{1}", starting, sb);
            }
            else if (TrimmingSource == TrimmingSource.Middle)
            {
                var currentWidth = GetTextWidth(AssociatedObject, "...");
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

                AssociatedObject.Text = string.Format("{0}...{1}", begin, end);
            }
            else if (TrimmingSource == TrimmingSource.End)
            {
                var n = 0;
                var sb = new StringBuilder();
                for (var i = 0; i < Offset; i++)
                    sb.Insert(0, text[text.Length - i - 1]);
                sb.Insert(0, "...");

                var ending = sb.ToString();
                sb.Clear();
                var currentWidth = GetTextWidth(AssociatedObject, ending);

                while (true)
                {
                    var test = currentWidth + sizes[n];

                    if (test > availableWidth)
                        break;

                    sb.Append(text[n++]);
                    currentWidth = test;
                }

                AssociatedObject.Text = string.Format("{0}{1}", sb, ending);
            }
        }

        private double GetTextWidth(TextBlock textBlock, string text)
        {
            double[] dummy;
            return GetTextWidth(textBlock, text, out dummy);
        }

        private double GetTextWidth(TextBlock textBlock, string text, out double[] sizes)
        {
            var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);

            var totalWidth = 0.0;

            if (TextTrimming == TextTrimming.CharacterEllipsis)
            {
                sizes = new double[text.Length];

                for (var i = 0; i < text.Length; i++)
                {
                    string token = text[i].ToString(CultureInfo.CurrentUICulture);
                    var formattedText = new FormattedText(token, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, textBlock.FontSize, Brushes.Black);
                    double width = formattedText.Width;

                    sizes[i] = width;
                    totalWidth += width;
                }
            }
            else if (TextTrimming == TextTrimming.WordEllipsis)
            {
                var words = Split(text);
                sizes = new double[words.Length];

                for (var i = 0; i < words.Length; i++)
                {
                    string token = words[i];
                    var formattedText = new FormattedText(token, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, textBlock.FontSize, Brushes.Black);
                    double width = formattedText.Width;
                    sizes[i] = width;
                    totalWidth += width;
                }
            }
            else
                throw new ArgumentException("Invalid 'TextTrimming' argument.");

            return totalWidth;
        }

        private string[] Split(string text)
        {
            var words = new List<string>();

            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (processedWordSeparators.Contains(c))
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
    }
}
