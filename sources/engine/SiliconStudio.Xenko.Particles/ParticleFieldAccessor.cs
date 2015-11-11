// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles
{
    public struct ParticleFieldAccessor
    {
        // Not sure about this class. I copied it from the old implementation, but I think it can be removed.
        // Maybe change ParticleFieldDescription and use it directly?
        private readonly int offset;

        internal ParticleFieldAccessor(ParticleField field)
        {
            offset = field.Offset;
        }

        public ParticleFieldAccessor(int offset)
        {
            this.offset = offset;
        }

        public bool IsValid() => (offset != -1);

        public static implicit operator int(ParticleFieldAccessor accessor) => accessor.offset;
    }

    public struct ParticleFieldAccessor<T>
    {
        private readonly int offset;

        internal ParticleFieldAccessor(ParticleField field)
        {
            offset = field.Offset;
        }

        public ParticleFieldAccessor(int offset)
        {
            this.offset = offset;
        }

        public bool IsValid() => (offset != -1);

        public static implicit operator ParticleFieldAccessor(ParticleFieldAccessor<T> accessor) => new ParticleFieldAccessor(accessor.offset);

        public static implicit operator int(ParticleFieldAccessor<T> accessor) => accessor.offset;
    }
}
