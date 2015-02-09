// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Groups entity based on a flag.
    /// </summary>
    [Flags]
    [DataContract]
    public enum EntityGroup : uint
    {
        None = 0,

        Default = Group1,
        Group1 = 1 << 0,
        Group2 = 1 << 1,
        Group3 = 1 << 2,
        Group4 = 1 << 3,
        Group5 = 1 << 4,
        Group6 = 1 << 5,
        Group7 = 1 << 6,
        Group8 = 1 << 7,
        Group9 = 1 << 8,
        Group10 = 1 << 9,
        Group11 = 1 << 10,
        Group12 = 1 << 11,
        Group13 = 1 << 12,
        Group14 = 1 << 13,
        Group15 = 1 << 14,
        Group16 = 1 << 15,
        Group17 = 1 << 16,
        Group18 = 1 << 17,
        Group19 = 1 << 18,
        Group20 = 1 << 19,
        Group21 = 1 << 20,
        Group22 = 1 << 21,
        Group23 = 1 << 22,
        Group24 = 1 << 23,
        Group25 = 1 << 24,
        Group26 = 1 << 25,
        Group27 = 1 << 26,
        Group28 = 1 << 27,
        Group29 = 1 << 28,
        Group30 = 1 << 29,
        Group31 = 1 << 30,

        All = 0xffffffff
    }
}
