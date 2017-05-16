// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.IO;

namespace SiliconStudio.Core.Storage
{
    public class DigestStream : OdbStreamWriter
    {
        private ObjectIdBuilder builder = new ObjectIdBuilder();

        public override ObjectId CurrentHash
        {
            get
            {
                return builder.ComputeHash();
            }
        }

        public DigestStream(Stream stream) : base(stream, null)
        {
        }

        internal DigestStream(Stream stream, string temporaryName) : base(stream, temporaryName)
        {
        }

        public void Reset()
        {
            Position = 0;
            builder.Reset();
        }

        public override void WriteByte(byte value)
        {
            builder.WriteByte(value);
            stream.WriteByte(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            builder.Write(buffer, offset, count);
            stream.Write(buffer, offset, count);
        }
    }
}
