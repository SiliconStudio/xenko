// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Graphics
{
    [DataConverter(AutoGenerate = true, CustomConvertFromData = true)]
    public class IndexBufferBinding
    {
        public IndexBufferBinding(Buffer indexBuffer, bool is32Bit, int count, int indexOffset = 0)
        {
            if (indexBuffer == null) throw new ArgumentNullException("indexBuffer");
            Buffer = indexBuffer;
            Is32Bit = is32Bit;
            Offset = indexOffset;
            Count = count;
        }

        [DataMemberConvert]
        public Buffer Buffer { get; private set; }
        [DataMemberConvert]
        public bool Is32Bit { get; private set; }
        [DataMemberConvert]
        public int Offset { get; private set; }

        [DataMemberConvert]
        public int Count { get; private set; }
    }
}