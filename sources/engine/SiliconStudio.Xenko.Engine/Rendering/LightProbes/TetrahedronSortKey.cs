// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Rendering.LightProbes
{
    public struct TetrahedronSortKey : IComparable<TetrahedronSortKey>
    {
        public int Index;
        public int SortKey;

        public TetrahedronSortKey(int index, int sortKey)
        {
            Index = index;
            SortKey = sortKey;
        }

        public int CompareTo(TetrahedronSortKey other)
        {
            return SortKey.CompareTo(other.SortKey);
        }

        public override string ToString()
        {
            return $"Tetrahedron Index: {Index}; SortKey: {SortKey}";
        }
    }
}