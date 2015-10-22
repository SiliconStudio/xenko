// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SiliconStudio.Xenko.VisualStudio
{
    public partial class OutputClassifier : IClassifier
    {
        private IClassificationTypeRegistryService classificationRegistry;

        private int messageTypeCharPosition;

        public OutputClassifier(IClassificationTypeRegistryService classificationRegistry)
        {
            this.classificationRegistry = classificationRegistry;
            this.messageTypeCharPosition = "[BuildEngine] ".Length;

            InitializeClassifiers();
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var spans = new List<ClassificationSpan>();
            var text = span.GetText();

            if (text.StartsWith("[BuildEngine]"))
            {
                var messageType = text[messageTypeCharPosition];
                string classificationType;
                if (classificationTypes.TryGetValue(messageType, out classificationType))
                {
                    var type = classificationRegistry.GetClassificationType(classificationType);
                    spans.Add(new ClassificationSpan(span, type));
                }
            }

            return spans;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }
    }
}