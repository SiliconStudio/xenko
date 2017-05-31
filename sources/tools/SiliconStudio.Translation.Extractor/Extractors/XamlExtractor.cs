// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Xaml;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Translation.Presentation.MarkupExtensions;

namespace SiliconStudio.Translation.Extractor
{
    internal class XamlExtractor : ExtractorBase
    {
        public XamlExtractor([NotNull] ICollection<UFile> inputFiles)
            : base(inputFiles, ".xaml")
        {
        }

        protected override IEnumerable<Message> ExtractMessagesFromFile(UFile file)
        {
            // Read all content
            var reader = new XamlXmlReader(file.ToWindowsPath(), new XamlXmlReaderSettings { ProvideLineInfo = true });
            while (reader.Read())
            {
                if (reader.NodeType != XamlNodeType.StartObject)
                    continue;

                var lineNumber = reader.LineNumber;
                var type = reader.Type.UnderlyingType;
                if (type == typeof(LocalizeExtension))
                {
                    var readSubtree = reader.ReadSubtree();
                    readSubtree.Read(); // read once to position on the first node
                    var message = ParseLocalizeExtension(readSubtree);
                    message.LineNumber = lineNumber;
                    message.Source = file;
                    yield return message;
                }
            }
        }

        [NotNull]
        private Message ParseLocalizeExtension([NotNull] XamlReader reader)
        {
            var message = new Message();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XamlNodeType.StartObject:
                        // Skip this object
                        while (reader.Read() && reader.NodeType != XamlNodeType.EndObject) { }
                        break;

                    case XamlNodeType.StartMember:
                        var member = reader.Member;
                        switch (member.Name)
                        {
                            case nameof(LocalizeExtension.Plural):
                                if (reader.Read() && reader.NodeType == XamlNodeType.Value)
                                {
                                    message.PluralText = reader.Value?.ToString();
                                }
                                break;

                            case nameof(LocalizeExtension.Context):
                                if (reader.Read() && reader.NodeType == XamlNodeType.Value)
                                {
                                    message.Context = reader.Value?.ToString();
                                }
                                break;

                            case nameof(LocalizeExtension.Count):
                            case nameof(LocalizeExtension.Text):
                                // Ignore
                                break;

                            default:
                                // Positional parameter in the constructor
                                var paramIndex = 0;
                                while (reader.Read() && reader.NodeType == XamlNodeType.Value)
                                {
                                    switch (paramIndex)
                                    {
                                        case 0:
                                            message.Text = reader.Value?.ToString();
                                            break;

                                        default:
                                            throw new IndexOutOfRangeException();
                                    }
                                    paramIndex++;
                                }
                                break;
                        }
                        break;
                }
            }
            return message;
        }
    }
}
