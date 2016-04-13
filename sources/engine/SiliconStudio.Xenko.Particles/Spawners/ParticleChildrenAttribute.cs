// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    public struct ParticleChildrenAttribute
    {
        public static ParticleChildrenAttribute Empty = new ParticleChildrenAttribute { flags = 0 };

        private uint flags;

        // Maybe encode it in 0-255 value and put it in flags?
        private float carryOver;

        public ParticleChildrenAttribute(ParticleChildrenAttribute other)
        {
            flags = other.flags;
            carryOver = other.carryOver;
        }

        private const uint MaskParticlesToEmit = 0xFF << 0;

        public uint ParticlesToEmit
        {
            get { return (flags & MaskParticlesToEmit); }
            set
            {
                flags = (flags & ~MaskParticlesToEmit) + Math.Min(value, MaskParticlesToEmit);
            }
        }

        public float CarryOver
        {
            get { return carryOver; }
            set { carryOver = value; }
        }
    }
}
