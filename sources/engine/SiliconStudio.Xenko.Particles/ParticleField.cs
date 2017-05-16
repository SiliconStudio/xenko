// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Particles
{
    internal struct ParticleField
    {
#if PARTICLES_SOA
        /// <summary>
        /// Offset of the field from the particle pool's head
        /// </summary>
        public IntPtr Offset;

        /// <summary>
        /// Size of one data unit. Depends of how you group the fields together (AoS or SoA)
        /// </summary>
        public readonly int Size;

        public ParticleField(int fieldSize, IntPtr offset)
        {
            Offset = offset;
            Size = fieldSize;
        }
#else
        /// <summary>
        /// Offset of the field from the particle's position
        /// </summary>
        public int Offset;

        /// <summary>
        /// Size of the field
        /// </summary>
        public int Size;
#endif
    }
}
