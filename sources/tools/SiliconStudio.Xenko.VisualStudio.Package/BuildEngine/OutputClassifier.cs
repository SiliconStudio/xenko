// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using SiliconStudio.Xenko.VisualStudio.Commands;

namespace SiliconStudio.Xenko.VisualStudio
{
    public partial class OutputClassifier : IClassifier
    {
        private IClassificationTypeRegistryService classificationRegistry;

        public OutputClassifier(IClassificationTypeRegistryService classificationRegistry)
        {
            this.classificationRegistry = classificationRegistry;

            InitializeClassifiers();
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var spans = new List<ClassificationSpan>();
            var text = span.GetText();

            string messageType;
            if (BuildMonitorCallback.FilterMessage(text, out messageType))
            {
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
