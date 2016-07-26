// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    public struct ParticleSortedListNone : IParticleSortedList
    {
        private readonly ParticlePool pool;

        public ParticleSortedListNone(ParticlePool particlePool)
        {
            pool = particlePool;
        }

        /// <inheritdoc />
        public ParticleFieldAccessor<T> GetField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct => pool.GetField<T>(fieldDesc);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public ParticlePool.Enumerator GetEnumerator() => pool.GetEnumerator();

        public void Free() { }
    }

    /// <summary>
    /// The default sorter doesn not sort the particles, but only passes them directly to the renderer
    /// </summary>
    public class ParticleSorterDefault : ParticleSorter 
    {
        public override IParticleSortedList GetSortedList(Vector3 depth)
        {
            return new ParticleSortedListNone(ParticlePool);
        }

        public ParticleSorterDefault(ParticlePool pool) : base(pool) { }
    }
}
