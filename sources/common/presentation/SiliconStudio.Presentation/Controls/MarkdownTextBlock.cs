// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SiliconStudio.Presentation.Controls
{
    [TemplatePart(Name = MessageContainerPartName, Type = typeof(FlowDocumentScrollViewer))]
    public class MarkdownTextBlock : Control
    {
        /// <summary>
        /// The name of the part for the <see cref="FlowDocumentScrollViewer"/> container.
        /// </summary>
        private const string MessageContainerPartName = "PART_MessageContainer";

        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register("Markdown", typeof(XamlMarkdown), typeof(MarkdownTextBlock));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MarkdownTextBlock), new PropertyMetadata(TextChanged));

        public XamlMarkdown Markdown
        {
            get { return (XamlMarkdown)GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// The container in which the message is displayed.
        /// </summary>
        private FlowDocumentScrollViewer messageContainer;

        /// <summary>
        /// Default markdown used if none is supplied.
        /// </summary>
        private readonly Lazy<XamlMarkdown> defaultMarkdown = new Lazy<XamlMarkdown>(() => new XamlMarkdown());

        static MarkdownTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MarkdownTextBlock), new FrameworkPropertyMetadata(typeof(MarkdownTextBlock)));
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            messageContainer = GetTemplateChild(MessageContainerPartName) as FlowDocumentScrollViewer;
            if (messageContainer == null)
                throw new InvalidOperationException($"A part named '{MessageContainerPartName}' must be present in the ControlTemplate, and must be of type '{typeof(FlowDocumentScrollViewer)}'.");

            ResetMessage();
        }

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MarkdownTextBlock;
            control?.ResetMessage();
        }

        private void ResetMessage()
        {
            if (messageContainer != null)
            {
                messageContainer.Document = ProcessText();
            }
        }

        private FlowDocument ProcessText()
        {
            var engine = Markdown ?? defaultMarkdown.Value;
            return engine.Transform(Text ?? "*Nothing to display*");
        }
    }
}
