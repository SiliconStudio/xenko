// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Particles
{
    internal struct ParticleField
    {
        /// <summary>
        /// Offset of the field from the particle pool's head
        /// </summary>
        public IntPtr Offset;

        /// <summary>
        /// Size of one data unit. Depends of how you group the fields together (AoS or SoA)
        /// </summary>
        public int Size;

        /// <summary>
        /// Field size is strictly the size of one unit in this field, regardless of how it is grouped with other fields.
        /// </summary>
        public readonly int FieldSize;

        public ParticleField(int fieldSize, IntPtr offset, int totalUnitSize = 0)
        {
            Offset = offset;
            Size = totalUnitSize;
            FieldSize = fieldSize;
        }
    }
}
