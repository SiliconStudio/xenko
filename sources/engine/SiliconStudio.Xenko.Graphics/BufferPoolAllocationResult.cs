// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Graphics
{
    public struct BufferPoolAllocationResult
    {
        public IntPtr Data;
        public int Size;
        public int Offset;

        public bool Uploaded;
        public Buffer Buffer;
    }
}
