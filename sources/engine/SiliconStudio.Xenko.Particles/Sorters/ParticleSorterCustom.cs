// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    public struct ParticleSortedListCustom<T> : IParticleSortedList where T : struct
    {
        private readonly ParticlePool pool;
        private readonly int currentLivingParticles;
        private readonly SortedParticle[] particleList;

        /// <summary>
        /// Will construct an unsorted list of the living particles in the specified pool
        /// </summary>
        /// <param name="particlePool">The <see cref="ParticlePool"/></param>
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

        /// <summary>
        /// Will construct a sorted list of the living particles in the specified pool
        /// </summary>
        /// <param name="particlePool">The <see cref="ParticlePool"/></param>
        /// <param name="fieldDesc">The particle attribute field to use as a base sorting value</param>
        /// <param name="sorter">The converter for the particle attribute to a sorting key</param>
        public ParticleSortedListCustom(ParticlePool particlePool, ParticleFieldDescription<T> fieldDesc, ParticleSorterCustom<T> sorter)
        {
            pool = particlePool;

            currentLivingParticles = pool.LivingParticles;
            var i = 0;

            particleList = new SortedParticle[currentLivingParticles];

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
    public abstract class ParticleSorterCustom<T> : ParticleSorter where T : struct
    {
        private readonly ParticleFieldDescription<T> fieldDesc;

        protected ParticleSorterCustom(ParticlePool pool, ParticleFieldDescription<T> fieldDesc) : base(pool)
        {
            this.fieldDesc = fieldDesc;
        }

        public abstract float GetSortValue(T value);

        public override IParticleSortedList GetSortedList()
        {
            var sortField = ParticlePool.GetField(fieldDesc);

            if (!sortField.IsValid())
                return new ParticleSortedListCustom<T>(ParticlePool);

            return new ParticleSortedListCustom<T>(ParticlePool, fieldDesc, this);
        }
    }

    /// <summary>
    /// Sorts the particles by ascending order of their Depth (position on the camera's Z axis)
    /// </summary>
    public class ParticleSorterDepth : ParticleSorterCustom<Vector3>
    {
        public Vector3 DepthVector { get; set; }

        public ParticleSorterDepth(ParticlePool pool) : base(pool, ParticleFields.Position) { }

        public override float GetSortValue(Vector3 position) => Vector3.Dot(DepthVector, position);
    }

    /// <summary>
    /// Sorts the particles by descending order of their remaining Life
    /// </summary>
    public class ParticleSorterAge : ParticleSorterCustom<float>
    {
        public ParticleSorterAge(ParticlePool pool) : base(pool, ParticleFields.Life) { }

        public override float GetSortValue(float life) => -life;
    }

    /// <summary>
    /// Sorts the particles by ascending order of their Order attribute
    /// </summary>
    public class ParticleSorterOrder : ParticleSorterCustom<uint>
    {
        public ParticleSorterOrder(ParticlePool pool) : base(pool, ParticleFields.Order) { }

        public override float GetSortValue(uint order) => BitConverter.ToSingle(BitConverter.GetBytes(order), 0) * -1f;
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
