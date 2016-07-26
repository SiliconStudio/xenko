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
        private readonly ConcurrentArrayPool<SortedParticle> arrayPool;
        private readonly int currentLivingParticles;
        private readonly SortedParticle[] particleList;

        /// <summary>
        /// Will construct an unsorted list of the living particles in the specified pool
        /// </summary>
        /// <param name="particlePool">The <see cref="ParticlePool"/></param>
        public ParticleSortedListCustom(ParticlePool particlePool, ConcurrentArrayPool<SortedParticle> sortedArrayPool)
        {
            pool = particlePool;
            arrayPool = sortedArrayPool;

            currentLivingParticles = pool.LivingParticles;
            particleList = arrayPool.Allocate(pool.ParticleCapacity);

            var i = 0;

            foreach (var particle in pool)
            {
                particleList[i] = new SortedParticle(particle, 0);
                i++;
            }
        }

        public void Free()
        {
            arrayPool.Free(particleList);
        }

        /// <summary>
        /// Will construct a sorted list of the living particles in the specified pool
        /// </summary>
        /// <param name="particlePool">The <see cref="ParticlePool"/></param>
        /// <param name="fieldDesc">The particle attribute field to use as a base sorting value</param>
        /// <param name="sorter">The converter for the particle attribute to a sorting key</param>
        public ParticleSortedListCustom(ParticlePool particlePool, ConcurrentArrayPool<SortedParticle> sortedArrayPool, ParticleFieldDescription<T> fieldDesc, ISortValueCalculator<T> sorter)
        {
            pool = particlePool;
            arrayPool = sortedArrayPool;

            currentLivingParticles = pool.LivingParticles;
            var i = 0;

            particleList = arrayPool.Allocate(pool.ParticleCapacity);

            var sortField = pool.GetField(fieldDesc);

            foreach (var particle in pool)
            {
                particleList[i] = new SortedParticle(particle, sorter.GetSortValue(particle.Get(sortField)));
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
    public abstract class ParticleSorterCustom<T> where T : struct
    {
        protected readonly ParticleFieldDescription<T> fieldDesc;

        protected readonly ConcurrentArrayPool<SortedParticle> ArrayPool = new ConcurrentArrayPool<SortedParticle>();

        protected readonly ParticlePool ParticlePool;

        protected ParticleSorterCustom(ParticlePool pool, ParticleFieldDescription<T> fieldDesc)
        {
            ParticlePool = pool;
            this.fieldDesc = fieldDesc;
        }
    }

    public interface ISortValueCalculator<T> where T : struct
    {
        float GetSortValue(T value);
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
