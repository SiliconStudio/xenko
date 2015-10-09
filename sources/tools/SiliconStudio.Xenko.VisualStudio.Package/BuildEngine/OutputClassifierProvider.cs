// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SiliconStudio.Paradox.VisualStudio.BuildEngine
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