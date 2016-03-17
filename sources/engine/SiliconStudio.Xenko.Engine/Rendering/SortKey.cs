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

        public int CompareTo(SortKey other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}