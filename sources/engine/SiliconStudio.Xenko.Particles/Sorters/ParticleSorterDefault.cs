// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    struct SortedParticle : IComparable
    {
        public readonly Particle Particle;
        public readonly float SortIndex;         // TODO Maybe use a Int32 key rather than float?

        public SortedParticle(Particle particle, float sortIndex)
        {
            Particle = particle;
            SortIndex = sortIndex;
        }

        int IComparable.CompareTo(object other)
        {
            return CompareTo((SortedParticle)other);
        }

        int CompareTo(SortedParticle other)
        {
            return (this == other) ? 0 : (this < other) ? -1 : 1;
        }

        public static bool operator <(SortedParticle left, SortedParticle right) => (left.SortIndex < right.SortIndex);

        public static bool operator >(SortedParticle left, SortedParticle right) => (left.SortIndex > right.SortIndex);

        public static bool operator <=(SortedParticle left, SortedParticle right) => (left.SortIndex <= right.SortIndex);

        public static bool operator >=(SortedParticle left, SortedParticle right) => (left.SortIndex >= right.SortIndex);

        public static bool operator ==(SortedParticle left, SortedParticle right) => (left.SortIndex == right.SortIndex);

        public static bool operator !=(SortedParticle left, SortedParticle right) => (left.SortIndex != right.SortIndex);

        public override bool Equals(object obj)
        {
            if (!(obj is SortedParticle))
                return false;

            var other = (SortedParticle)obj;
            return (this == other);
        }

        public override int GetHashCode()
        {
            return SortIndex.GetHashCode();
        }
    }

    public delegate float GetSortIndex<T>(T value) where T : struct;

    /// <summary>
    /// The default sorter doesn not sort the particles, but only passes them directly to the renderer
    /// </summary>
    public class ParticleSorterDefault : ParticleSorter 
    {
        public ParticleSorterDefault(ParticlePool pool) : base(pool) 
        {           
        }

        /// <inheritdoc />
        public override void Sort() { }

        /// <inheritdoc />
        public override IEnumerator<Particle> GetEnumerator()
        {
            return ParticlePool.GetEnumerator();
        }
    }
}
