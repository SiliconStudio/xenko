using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;

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
