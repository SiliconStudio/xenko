// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    public class AppendLogMessagesBehavior : Behavior<RichTextBox>
    {
        public static readonly DependencyProperty LogMessagesProperty = DependencyProperty.Register("LogMessages", typeof(IEnumerable<SerializableTimestampLogMessage>), typeof(AppendLogMessagesBehavior), new PropertyMetadata(null, LogMessagesChanged));

        public IEnumerable<SerializableTimestampLogMessage> LogMessages
        {
            get { return (IEnumerable<SerializableTimestampLogMessage>)GetValue(LogMessagesProperty); }
            set { SetValue(LogMessagesProperty, value); }
        }

        public static readonly DependencyProperty LogIdProperty = DependencyProperty.Register("LogId", typeof(long), typeof(AppendLogMessagesBehavior), new PropertyMetadata(-1L, RefreshDocument));

        public long LogId
        {
            get { return (long)GetValue(LogIdProperty); }
            set { SetValue(LogIdProperty, value); }
        }

        private void AppendLine()
        {
            if (AssociatedObject != null)
            {
                Paragraph paragraph = GetDocumentRoot(AssociatedObject);
                int currentLineCount = paragraph.Inlines.Count;

                var shouldScroll = AssociatedObject.VerticalOffset + AssociatedObject.ViewportHeight >= AssociatedObject.ExtentHeight - 1.0;
                foreach (SerializableTimestampLogMessage message in LogMessages.Skip(currentLineCount))
                {
                    paragraph.Inlines.Add(new Run(FormatLog(message)) { Foreground = GetLogColor(message) });
                }
                if (shouldScroll)
                    AssociatedObject.ScrollToEnd();
            }
        }

        private static void LogMessagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (AppendLogMessagesBehavior)d;

            var observable = e.OldValue as ObservableCollection<SerializableTimestampLogMessage>;

            if (observable != null)
            {
                observable.CollectionChanged += behavior.ObservableOnCollectionChanged;
            }

            observable = e.NewValue as ObservableCollection<SerializableTimestampLogMessage>;
            if (observable != null)
            {
                observable.CollectionChanged += behavior.ObservableOnCollectionChanged;
            }

            behavior.AppendLine();
        }

        private void ObservableOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            AppendLine();
        }

        private static void RefreshDocument(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (AppendLogMessagesBehavior)d;
            if (behavior.AssociatedObject != null)
            {
                Paragraph paragraph = GetDocumentRoot(behavior.AssociatedObject);
                paragraph.Inlines.Clear();
            }
        }

        private static Paragraph GetDocumentRoot(RichTextBox richTextBox)
        {
            if (richTextBox.Document == null)
            {
                richTextBox.Document = new FlowDocument(new Paragraph());
            }
            return (Paragraph)richTextBox.Document.Blocks.AsEnumerable().First();
        }

        private static string FormatLog(SerializableTimestampLogMessage message)
        {
            var builder = new StringBuilder();
            builder.Append((TimeSpan.FromTicks(message.Timestamp).TotalMilliseconds * 0.001).ToString("0.000 "));
            builder.Append(message.Module);
            builder.Append(": ");
            builder.Append(message.Text);
            builder.Append(Environment.NewLine);
            if (message.ExceptionInfo != null)
            {
                builder.AppendLine(string.Format("    Exception: {0}", message.ExceptionInfo));
            }
            return builder.ToString();
        }

        private static Brush GetLogColor(SerializableTimestampLogMessage message)
        {
            switch (message.Type)
            {
                case LogMessageType.Debug:
                    return Brushes.DarkGray;
                case LogMessageType.Verbose:
                    return Brushes.White;
                case LogMessageType.Info:
                    return Brushes.LawnGreen;
                case LogMessageType.Warning:
                    return Brushes.Yellow;
                case LogMessageType.Error:
                case LogMessageType.Fatal:
                    return Brushes.Red;
                default:
                    return Brushes.White;
            }
        }
    }
}
