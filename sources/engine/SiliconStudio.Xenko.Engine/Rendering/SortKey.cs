// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Sort key used 
    /// </summary>
    public struct SortKey : IComparable<SortKey>
    {
        public ulong Value;
        public int Index;
        public int StableIndex;

        public int CompareTo(SortKey other)
        {
            var result = Value.CompareTo(other.Value);
            return result != 0 ? result : StableIndex.CompareTo(other.StableIndex);
        }
    }
}
