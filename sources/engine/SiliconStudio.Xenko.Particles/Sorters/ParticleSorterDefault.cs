// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    /// <summary>
    /// The default sorter doesn not sort the particles, but only passes them directly to the renderer
    /// </summary>
    public class ParticleSorterDefault : IParticleSorter
    {
        /// <summary>
        /// Target <see cref="ParticlePool"/> to iterate and sort
        /// </summary>
        protected readonly ParticlePool ParticlePool;

        public ParticleSorterDefault(ParticlePool pool)
        {
            ParticlePool = pool;
        }

        public ParticleList GetSortedList(Vector3 depth)
        {
            return new ParticleList(ParticlePool, ParticlePool.LivingParticles);
        }

        /// <summary>
        /// The default sorter does not allocate any resources so there is nothing to free
        /// </summary>
        /// <param name="sortedList">Reference to the <see cref="ParticleList"/> to be freed</param>
        public void FreeSortedList(ref ParticleList sortedList) { }
    }
}
