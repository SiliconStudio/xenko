// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    public struct ParticleSortedListCustom<T> : IParticleSortedList where T : struct
    {
        private readonly ParticlePool pool;
        private readonly int currentLivingParticles;
        private readonly SortedParticle[] particleList;

        public ParticleSortedListCustom(ParticlePool particlePool)
        {
            pool = particlePool;

            currentLivingParticles = pool.LivingParticles;
            particleList = new SortedParticle[currentLivingParticles];

            var i = 0;


            foreach (var particle in pool)
            {
                particleList[i] = new SortedParticle(particle, 0);
                i++;
            }
        }

        public ParticleSortedListCustom(ParticlePool particlePool, ParticleFieldDescription<T> fieldDesc, GetSortIndex<T> getIndex)
        {
            pool = particlePool;

            currentLivingParticles = pool.LivingParticles;
            var i = 0;

            particleList = new SortedParticle[currentLivingParticles];

            var sortField = pool.GetField(fieldDesc);

            foreach (var particle in pool)
            {
                particleList[i] = new SortedParticle(particle, getIndex(particle.Get(sortField)));
                i++;
            }

            // Sort the list
            Array.Sort(particleList, 0, currentLivingParticles); // GC problem? Switch to another solution if needed
        }

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

    /// <summary>
    /// The custom sorter uses a user-defined method for generating sort index from a user-defined field
    /// </summary>
    public class ParticleSorterCustom<T> : ParticleSorter where T : struct
    {
        private readonly ParticleFieldDescription<T> fieldDesc;
        private readonly GetSortIndex<T> getIndex;

        public ParticleSorterCustom(ParticlePool pool, ParticleFieldDescription<T> fieldDesc, GetSortIndex<T> getIndex) : base(pool)
        {
            this.fieldDesc = fieldDesc;
            this.getIndex = getIndex;
        }

        public override IParticleSortedList GetSortedList()
        {
            var sortField = ParticlePool.GetField(fieldDesc);

            if (!sortField.IsValid())
                return new ParticleSortedListCustom<T>(ParticlePool);

            return new ParticleSortedListCustom<T>(ParticlePool, fieldDesc, getIndex);
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

}
