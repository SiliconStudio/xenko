// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Graphics.Data
{
    public partial class VertexBufferBindingData
    {
        public VertexBufferBindingData()
        {
        }

        public VertexBufferBindingData(ContentReference<BufferData> buffer, VertexDeclaration declaration, int count, int stride = 0, int offset = 0)
        {
            Buffer = buffer;
            Declaration = declaration;
            Count = count;
            Stride = stride;
            Offset = offset;
        }
    }

    public partial class IndexBufferBindingData
    {
        public IndexBufferBindingData()
        {
        }

        public IndexBufferBindingData(ContentReference<BufferData> buffer, bool is32Bit, int count, int offset = 0)
        {
            Buffer = buffer;
            Is32Bit = is32Bit;
            Count = count;
            Offset = offset;
        }
    }

    public partial class VertexBufferBindingDataConverter
    {
        public override void ConvertFromData(ConverterContext converterContext, VertexBufferBindingData data, ref VertexBufferBinding source)
        {
            Buffer buffer = null;
            converterContext.ConvertFromData(data.Buffer, ref buffer);
            source = new VertexBufferBinding(buffer, data.Declaration, data.Count, data.Stride, data.Offset);
        }
    }

    public partial class IndexBufferBindingDataConverter
    {
        public override void ConvertFromData(ConverterContext converterContext, IndexBufferBindingData data, ref IndexBufferBinding source)
        {
            Buffer buffer = null;
            converterContext.ConvertFromData(data.Buffer, ref buffer);
            source = new IndexBufferBinding(buffer, data.Is32Bit, data.Count, data.Offset);
        }
    }
}