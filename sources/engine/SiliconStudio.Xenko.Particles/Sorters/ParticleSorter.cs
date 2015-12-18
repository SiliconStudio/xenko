// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Particles.Sorters
{
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

        public abstract void Sort();

IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<Particle> GetEnumerator();
    }
}
