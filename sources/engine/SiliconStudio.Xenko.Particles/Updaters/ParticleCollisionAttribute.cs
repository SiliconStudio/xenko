// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
