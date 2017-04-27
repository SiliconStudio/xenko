// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SiliconStudio.Xenko.VisualStudio.BuildEngine
{
    [ContentType("output")]
    [Export(typeof(IClassifierProvider))]
    public class OutputClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;

        private static OutputClassifier outputClassifier;

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return outputClassifier ?? (outputClassifier = new OutputClassifier(ClassificationRegistry));
        }
    }
}
