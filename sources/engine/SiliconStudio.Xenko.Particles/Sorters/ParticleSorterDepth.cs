// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    /// <summary>
    /// Sorts the particles by ascending order of their Depth (position on the camera's Z axis)
    /// </summary>
    public class ParticleSorterDepth : ParticleSorterCustom<Vector3>, IParticleSorter
    {
        public ParticleSorterDepth(ParticlePool pool) : base(pool, ParticleFields.Position) { }

        public ParticleList GetSortedList(Vector3 depth)
        {
            var livingParticles = ParticlePool.LivingParticles;

            var sortField = ParticlePool.GetField(fieldDesc);

            if (!sortField.IsValid())
            {
                // Field is not valid - return an unsorted list
                return new ParticleList(ParticlePool, livingParticles);
            }

            SortedParticle[] particleList = ArrayPool.Allocate(ParticlePool.ParticleCapacity);

            var i = 0;
            foreach (var particle in ParticlePool)
            {
                particleList[i] = new SortedParticle(particle, Vector3.Dot(depth, particle.Get(sortField)));
                i++;
            }

            Array.Sort(particleList, 0, livingParticles);

            return new ParticleList(ParticlePool, livingParticles, particleList);
        }

        /// <summary>
        /// In case an array was used it must be freed back to the pool
        /// </summary>
        /// <param name="sortedList">Reference to the <see cref="ParticleList"/> to be freed</param>
        public void FreeSortedList(ref ParticleList sortedList)
        {
            sortedList.Free(ArrayPool);
        }
    }
}
