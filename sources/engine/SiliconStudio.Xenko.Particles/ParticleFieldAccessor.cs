// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Particles
{
    public struct ParticleFieldAccessor
    {
        // Not sure about this class. I copied it from the old implementation, but I think it can be removed.
        // Maybe change ParticleFieldDescription and use it directly?
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
    }

    public struct ParticleFieldAccessor<T>
    {
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

        static public ParticleFieldAccessor Invalid() => new ParticleFieldAccessor<T>(IntPtr.Zero, 0);

        public bool IsValid() => (offset != IntPtr.Zero);

        public static implicit operator ParticleFieldAccessor(ParticleFieldAccessor<T> accessor) => new ParticleFieldAccessor(accessor.offset, accessor.unitSize);

        public IntPtr this[int index] => (offset + index*unitSize);
    }
}
