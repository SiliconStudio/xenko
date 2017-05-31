// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.IO;

namespace SiliconStudio.Translation.Extractor
{
    internal class Message
    {
        public string Comment;
        public string Context;
        public string PluralText;
        public string Text;

        public long LineNumber;
        public UFile Source;
    }
}
