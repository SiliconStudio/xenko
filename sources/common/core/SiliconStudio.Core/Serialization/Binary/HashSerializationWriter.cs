// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;

namespace SiliconStudio.Core.Serialization
{
    public class HashSerializationWriter : BinarySerializationWriter
    {
        public HashSerializationWriter(Stream outputStream) : base(outputStream)
        {
        }

        /// <inheritdoc/>
        public unsafe override void Serialize(ref string value)
        {
            fixed (char* bufferStart = value)
            {
                Serialize((IntPtr)bufferStart, sizeof(char) * value.Length);
            }
        }
    }
}