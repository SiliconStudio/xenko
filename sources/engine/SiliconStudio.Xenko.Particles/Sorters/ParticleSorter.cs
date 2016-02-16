// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    /// <summary>
    /// Base enumerator which accesses all particles in a <see cref="ParticlePool"/> in a sorted manner
    /// </summary>
    public abstract class ParticleSorter : IEnumerable
    {
        /// <summary>
        /// Target <see cref="ParticlePool"/> to iterate and sort
        /// </summary>
        protected ParticlePool ParticlePool;

        protected ParticleSorter(ParticlePool pool)
        {
            ParticlePool = pool;
        }

        /// <summary>
        /// Returns a particle field accessor for the contained <see cref="ParticlePool"/>
        /// </summary>
        /// <typeparam name="T">Type data for the field</typeparam>
        /// <param name="fieldDesc">The field description</param>
        /// <returns></returns>
        public ParticleFieldAccessor<T> GetField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        {
            return ParticlePool.GetField<T>(fieldDesc);
        }

        /// <summary>
        /// Sorts the particles. Should be called once per frame, after the particles have been updated and before they are drawn
        /// </summary>
        public abstract void Sort();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<Particle> GetEnumerator();
    }
}
