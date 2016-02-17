// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
