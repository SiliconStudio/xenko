using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Particles.Sorters
{

    /// <summary>
    /// The default sorter doesn not sort the particles, but only passes them directly to the renderer
    /// </summary>
    public class ParticleSorterCustom : ParticleSorter
    {
        SortedParticle[] particleList;
        private int currentLivingParticles;

        public ParticleSorterCustom(ParticlePool pool) : base(pool)
        {
            particleList = new SortedParticle[pool.ParticleCapacity];
            currentLivingParticles = 0;
        }

        public override void Sort<T>(ParticleFieldDescription<T> fieldDesc, GetSortIndex<T> getIndex) 
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
                        var position = particle.Get(posField);

                        particleList[i].Particle = particle;
                        particleList[i].SortIndex = getIndex(particle.Get(posField));
                        i++;
                    }
                }
            }
            else
            {
                foreach (var particle in ParticlePool)
                {
                    particleList[i].Particle = particle;
                    particleList[i].SortIndex = 0;
                    i++;
                }
            }

            // Sort the list
            Array.Sort(particleList, 0, currentLivingParticles);
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
