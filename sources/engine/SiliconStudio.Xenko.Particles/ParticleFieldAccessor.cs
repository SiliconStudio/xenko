// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles
{
    public struct ParticleFieldAccessor
    {
#if PARTICLES_SOA
        private readonly int unitSize;
        private readonly IntPtr offset;

        internal ParticleFieldAccessor(ParticleField field)
        {
            offset = field.Offset;
            unitSize = field.Size;
        }

        public ParticleFieldAccessor(IntPtr offset, int unitSize)
        {
            this.offset = offset;
            this.unitSize = unitSize;
        }

        static public ParticleFieldAccessor Invalid() => new ParticleFieldAccessor(IntPtr.Zero, 0);

        public bool IsValid() => (offset != IntPtr.Zero);

        public IntPtr this[int index] => (offset + index * unitSize);
#else
        private readonly int offset;

        internal ParticleFieldAccessor(ParticleField field)
        {
            offset = field.Offset;
        }

        public ParticleFieldAccessor(int offset)
        {
            this.offset = offset;
        }

        static public ParticleFieldAccessor Invalid() => new ParticleFieldAccessor(-1);

        public bool IsValid() => (offset != -1);

        public static implicit operator int (ParticleFieldAccessor accessor) => accessor.offset;
#endif

    }

    public struct ParticleFieldAccessor<T>
    {
#if PARTICLES_SOA

        private readonly int unitSize;
        private readonly IntPtr offset;

        internal ParticleFieldAccessor(ParticleField field)
        {
            offset = field.Offset;
            unitSize = field.Size;
        }

        public ParticleFieldAccessor(IntPtr offset, int unitSize)
        {
            this.offset = offset;
            this.unitSize = unitSize;
        }

        static public ParticleFieldAccessor<T> Invalid() => new ParticleFieldAccessor<T>(IntPtr.Zero, 0);

        public bool IsValid() => (offset != IntPtr.Zero);

        public static implicit operator ParticleFieldAccessor(ParticleFieldAccessor<T> accessor) => new ParticleFieldAccessor(accessor.offset, accessor.unitSize);

        public IntPtr this[int index] => (offset + index*unitSize);

#else
        private readonly int offset;

        internal ParticleFieldAccessor(ParticleField field)
        {
            offset = field.Offset;
        }

        public ParticleFieldAccessor(int offset)
        {
            this.offset = offset;
        }

        static public ParticleFieldAccessor<T> Invalid() => new ParticleFieldAccessor<T>(-1);

        public bool IsValid() => (offset != -1);

        public static implicit operator ParticleFieldAccessor(ParticleFieldAccessor<T> accessor) => new ParticleFieldAccessor(accessor.offset);

        public static implicit operator int (ParticleFieldAccessor<T> accessor) => accessor.offset;
#endif
    }
}
