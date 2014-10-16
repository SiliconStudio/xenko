// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Particles
{
    /// <summary>
    /// Specifies how to access a <see cref="ParticleFieldDescription"/> in a given <see cref="ParticleSystem"/> instance.
    /// </summary>
    public struct ParticleFieldAccessor
    {
        private readonly int offset;

        internal ParticleFieldAccessor(ParticleField field)
        {
            this.offset = field.Offset;
        }
        
        public ParticleFieldAccessor(int offset)
        {
            this.offset = offset;
        }

        public bool IsValid()
        {
            return this.offset != -1;
        }

        public static implicit operator int(ParticleFieldAccessor accessor)
        {
            return accessor.offset;
        }
    }

    /// <summary>
    /// Specifies how to access a <see cref="ParticleFieldDescription{T}"/> in a given <see cref="ParticleSystem"/> instance.
    /// </summary>
    /// <typeparam name="T">Type of the field.</typeparam>
    public struct ParticleFieldAccessor<T>
    {
        private readonly int offset;

        internal ParticleFieldAccessor(ParticleField field)
        {
            this.offset = field.Offset;
        }

        public ParticleFieldAccessor(int offset)
        {
            this.offset = offset;
        }

        public bool IsValid()
        {
            return this.offset != -1;
        }

        public static implicit operator ParticleFieldAccessor(ParticleFieldAccessor<T> field)
        {
            return new ParticleFieldAccessor(field.offset);
        }

        public static implicit operator int(ParticleFieldAccessor<T> accessor)
        {
            return accessor.offset;
        }
    }
}