// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles.Updaters
{
    public struct ParticleCollisionAttribute
    {
        public static ParticleCollisionAttribute Empty = new ParticleCollisionAttribute { flags = 0 };

        private uint flags;

        public ParticleCollisionAttribute(ParticleCollisionAttribute other)
        {
            flags = other.flags;
        }

        private const uint FlagsHasColided = 0x1 << 0;

        public bool HasColided
        {
            get { return (flags & FlagsHasColided) > 0; }
            set
            {
                if (value)
                    flags |= FlagsHasColided;
                else
                    flags &= ~FlagsHasColided;
            }
        }
    }
}
