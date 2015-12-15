using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    struct SortedParticle : IComparable
    {
        public Particle Particle;
        public float SortIndex;         // TODO Maybe use a Int32 key rather than float?

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
    }

    public delegate float GetSortIndex<T>(T value) where T : struct;

    public abstract class ParticleSorter : IEnumerable
    {
        protected ParticlePool ParticlePool;

        public ParticleFieldAccessor<T> GetField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        {
            return ParticlePool.GetField<T>(fieldDesc);
        }

        protected ParticleSorter(ParticlePool pool)
        {
            ParticlePool = pool;
        }

        public abstract void Sort<T>(ParticleFieldDescription<T> fieldDesc, GetSortIndex<T> getIndex) where T : struct;

IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<Particle> GetEnumerator();
    }
}
