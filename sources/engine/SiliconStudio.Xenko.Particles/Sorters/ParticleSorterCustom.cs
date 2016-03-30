// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Particles.Sorters
{

    /// <summary>
    /// The custom sorter uses a user-defined method for generating sort index from a user-defined field
    /// </summary>
    public class ParticleSorterCustom<T> : ParticleSorter where T : struct
    {
        SortedParticle[] particleList;
        private int currentLivingParticles;

        private readonly ParticleFieldDescription<T> fieldDesc;
        private readonly GetSortIndex<T> getIndex;

        public ParticleSorterCustom(ParticlePool pool, ParticleFieldDescription<T> fieldDesc, GetSortIndex<T> getIndex) : base(pool)
        {
            particleList = new SortedParticle[pool.ParticleCapacity];
            currentLivingParticles = 0;

            this.fieldDesc = fieldDesc;
            this.getIndex = getIndex;
        }

        public override void Sort() 
        {
            currentLivingParticles = ParticlePool.LivingParticles;
            var i = 0;

            var posField = ParticlePool.GetField(fieldDesc);

            if (posField.IsValid())
            {
                unsafe
                {
                    foreach (var particle in ParticlePool)
                    {
                        particleList[i] = new SortedParticle(particle, getIndex(particle.Get(posField)));
                        i++;
                    }
                }
            }
            else
            {
                foreach (var particle in ParticlePool)
                {
                    particleList[i] = new SortedParticle(particle, 0);
                    i++;
                }
            }

            // Sort the list
            Array.Sort(particleList, 0, currentLivingParticles); // GC problem? Switch to another solution if needed
        }

        public override IEnumerator<Particle> GetEnumerator()
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

}
