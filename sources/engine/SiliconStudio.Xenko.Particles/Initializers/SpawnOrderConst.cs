// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles.Initializers
{
    /// <summary>
    /// Spawn order can be additionally subdivided in groups
    /// </summary>
    public static class SpawnOrderConst
    {
        public const int GroupBitOffset = 16;
        public const uint GroupBitMask = 0xFFFF0000;
        public const uint AuxiliaryBitMask = 0x0000FFFF;
        public const int LargeGroupBitOffset = 20;
        public const uint LargeGroupBitMask = 0xFFF00000;
        public const uint LargeAuxiliaryBitMask = 0x000FFFFF;
    }
}
