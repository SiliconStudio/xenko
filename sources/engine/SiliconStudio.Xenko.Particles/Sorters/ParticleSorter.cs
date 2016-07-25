// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

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

    public struct SortedParticle : IComparable<SortedParticle>
    {
        public readonly Particle Particle;
        public readonly float SortIndex;         // TODO Maybe use a Int32 key rather than float?

        public SortedParticle(Particle particle, float sortIndex)
        {
            Particle = particle;
            SortIndex = sortIndex;
        }

        int IComparable<SortedParticle>.CompareTo(SortedParticle other)
        {
            return (SortIndex < other.SortIndex) ? -1 : (SortIndex > other.SortIndex) ? 1 : 0;
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

        public abstract IParticleSortedList GetSortedList(Vector3 depth);        
    }
}
