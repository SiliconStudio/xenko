// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GNU.Gettext;
using GNU.Gettext.Utils;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Translation.Extractor
{
    // ReSharper disable once InconsistentNaming
    internal class POExporter
    {
        public POExporter([NotNull] Options options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Catalog = new Catalog();
            if (!Options.Overwrite && File.Exists(Options.OutputFile))
            {
                Catalog.Load(Options.OutputFile);
                Catalog.ForEach(e => e.ClearReferences());
            }
        }

        public Catalog Catalog { get; }

        public Options Options { get; }

        public void Merge([ItemNotNull, NotNull]  IEnumerable<Message> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            messages.ForEach(MergeMessage);
        }

        public void Save()
        {
            if (File.Exists(Options.OutputFile))
            {
                var bakFileName = Options.OutputFile + ".bak";
                File.Copy(Options.OutputFile, bakFileName, true);
                File.Delete(Options.OutputFile);
            }
            Catalog.Save(Options.OutputFile);
        }

        private void MergeMessage([NotNull] Message message)
        {
            var entry = Catalog.FindItem(message.Text, message.Context);
            var newEntry = entry == null;
            if (newEntry)
            {
                entry = new CatalogEntry(Catalog, message.Text, message.PluralText);
            }

            // Source reference
            if (message.Source != null)
            {
                var filePath = FileUtils.GetRelativeUri(message.Source.FullPath, Path.GetFullPath(Options.OutputFile));
                entry.AddReference($"{filePath}:{message.LineNumber}");
            }
            // Plural
            if (!string.IsNullOrEmpty(message.PluralText))
            {
                entry.SetTranslations(Enumerable.Repeat("", Catalog.PluralFormsCount).ToArray());
                entry.SetPluralString(message.PluralText);
            }
            // Context
            if (!string.IsNullOrEmpty(message.Context))
            {
                entry.Context = message.Context;
            }
            // Comments
            if (!string.IsNullOrEmpty(message.Comment))
            {
                entry.Comment = message.Comment;
            }
            // Add entry if it did not exist
            if (newEntry)
                Catalog.AddItem(entry);
        }
    }
}
