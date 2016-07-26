// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    public struct ParticleSortedListLiving : IParticleSortedList 
    {
        private readonly ParticlePool pool;
        private readonly int currentTotalParticles;
        private readonly int currentLivingParticles;
        private readonly SortedParticle[] particleList;

        /// <summary>
        /// Will construct an unsorted list of the living particles in the specified pool
        /// </summary>
        /// <param name="particlePool">The <see cref="ParticlePool"/></param>
        public ParticleSortedListLiving(ParticlePool particlePool)
        {
            pool = particlePool;

            currentTotalParticles = pool.LivingParticles;
            particleList = new SortedParticle[currentTotalParticles];

            var lifeField = particlePool.GetField(ParticleFields.Life);

            var i = 0;

            if (lifeField.IsValid())
            {
                foreach (var particle in particlePool)
                {
                    if (particle.Get(lifeField) > 0)
                    {
                        particleList[i] = new SortedParticle(particle, 0);
                        i++;
                    }
                }
            }
            else
            {
                foreach (var particle in particlePool)
                {
                    particleList[i] = new SortedParticle(particle, 0);
                    i++;
                }
            }

            currentLivingParticles = i;
        }

        public void Free() { }

        /// <inheritdoc />
        public ParticleFieldAccessor<T> GetField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct => pool.GetField<T>(fieldDesc);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<Particle> GetEnumerator()
        {
            return new Enumerator(particleList, currentLivingParticles);
        }
    }

    public struct Enumerator : IEnumerator<Particle>
    {
        private readonly SortedParticle[] sortedList;
        private readonly int listCapacity;

        private int index;

        internal Enumerator(SortedParticle[] list, int capacity)
        {
            sortedList = list;
            listCapacity = capacity;
            index = -1;
        }

        /// <inheritdoc />
        public void Reset()
        {
            index = -1;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            return (++index < listCapacity);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        public Particle Current => sortedList[index].Particle;

        object IEnumerator.Current => sortedList[index].Particle;
    }

    /// <summary>
    /// The <see cref="ParticleSorterLiving"/> collects all living particles, rather than sorting them
    /// It is useful for some pool policies, like Ring, which iterate over all particles, not only living ones
    /// </summary>
    public class ParticleSorterLiving : IParticleSorter
    {
        private readonly ParticlePool pool;

        public ParticleSorterLiving(ParticlePool pool)
        {
            this.pool = pool;
        }

        IParticleSortedList IParticleSorter.GetSortedList(Vector3 depth) => GetSortedList(depth);

        public ParticleSortedListLiving GetSortedList(Vector3 depth) => new ParticleSortedListLiving(pool);

    }

}
