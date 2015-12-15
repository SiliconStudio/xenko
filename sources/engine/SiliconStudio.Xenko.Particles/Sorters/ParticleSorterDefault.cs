using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    /// <summary>
    /// The default sorter doesn not sort the particles, but only passes them directly to the renderer
    /// </summary>
    public class ParticleSorterDefault : ParticleSorter 
    {
        public ParticleSorterDefault(ParticlePool pool) : base(pool) 
        {           
        }

        public override void Sort() { }

        public override IEnumerator<Particle> GetEnumerator()
        {
            return ParticlePool.GetEnumerator();
        }
    }
}
