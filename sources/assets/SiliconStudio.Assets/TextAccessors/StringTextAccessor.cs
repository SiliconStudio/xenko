// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
