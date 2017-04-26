// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Assets.TextAccessors
{
    [DataContract]
    public class FileTextAccessor : ISerializableTextAccessor
    {
        [DataMember]
        public string FilePath { get; set; }

        public ITextAccessor Create()
        {
            return new DefaultTextAccessor { FilePath = FilePath };
        }
    }
}
