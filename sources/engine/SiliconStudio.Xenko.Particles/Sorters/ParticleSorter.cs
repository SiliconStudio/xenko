// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    public interface IParticleSortedList : IEnumerable<Particle>
    {
        /// <summary>
        /// Returns a particle field accessor for the contained <see cref="ParticlePool"/>
        /// </summary>
        /// <typeparam name="T">Type data for the field</typeparam>
        /// <param name="fieldDesc">The field description</param>
        /// <returns></returns>
        ParticleFieldAccessor<T> GetField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct;
    }

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

    /// <summary>
    /// Base enumerator which accesses all particles in a <see cref="ParticlePool"/> in a sorted manner
    /// </summary>
    public abstract class ParticleSorter
    {
        /// <summary>
        /// Target <see cref="ParticlePool"/> to iterate and sort
        /// </summary>
        protected readonly ParticlePool ParticlePool;

        protected ParticleSorter(ParticlePool pool)
        {
            ParticlePool = pool;
        }

        public abstract IParticleSortedList GetSortedList();        
    }
}
