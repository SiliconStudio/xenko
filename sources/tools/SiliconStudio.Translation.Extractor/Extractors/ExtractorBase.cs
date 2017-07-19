// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Translation.Extractor
{
    internal abstract class ExtractorBase
    {
        private readonly string[] supportedExtensions;

        protected ExtractorBase([NotNull] ICollection<UFile> inputFiles, params string[] supportedExtensions)
        {
            this.supportedExtensions = supportedExtensions;
            InputFiles = inputFiles ?? throw new ArgumentNullException(nameof(inputFiles));
        }

        protected ICollection<UFile> InputFiles { get; }

        [NotNull]
        public IReadOnlyList<Message> ExtractMessages()
        {
            return InputFiles.Where(f => supportedExtensions.Contains(f.GetFileExtension())).SelectMany(ExtractMessagesFromFile).ToList();
        }

        [ItemNotNull]
        protected abstract IEnumerable<Message> ExtractMessagesFromFile([NotNull] UFile file);
    }
}
