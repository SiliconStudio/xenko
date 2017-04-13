// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Assets.TextAccessors
{
    [DataContract]
    public class StringTextAccessor : ISerializableTextAccessor
    {
        [DataMember]
        public string Text { get; set; }

        public ITextAccessor Create()
        {
            var result = new DefaultTextAccessor();
            result.Set(Text);
            return result;
        }
    }
}
