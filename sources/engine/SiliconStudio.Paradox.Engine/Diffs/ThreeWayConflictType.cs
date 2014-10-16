// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Diffs
{
    internal enum ThreeWayConflictType
    {
        Deleted1And2,
        Modified1And2,
        Insertion1And2,
        Modified1Deleted2,
        Modified2Deleted1,
    }
}