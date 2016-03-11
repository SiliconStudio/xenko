// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Particles.Sorters
{

    /// <summary>
    /// The <see cref="ParticleSorterLiving"/> collects all living particles, rather than sorting them
    /// It is useful for some pool policies, like Ring, which iterate over all particles, not only living ones
    /// </summary>
    public class ParticleSorterLiving : ParticleSorter 
    {
        readonly SortedParticle[] particleList;
        private int currentLivingParticles;

        public ParticleSorterLiving(ParticlePool pool) : base(pool)
        {
            particleList = new SortedParticle[pool.ParticleCapacity];
            currentLivingParticles = 0;
        }

        /// <inheritdoc />
        public override void Sort()
        {
            var i = 0;

            var lifeField = ParticlePool.GetField(ParticleFields.Life);

            if (lifeField.IsValid())
            {
                foreach (var particle in ParticlePool)
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
                foreach (var particle in ParticlePool)
                {
                    particleList[i] = new SortedParticle(particle, 0);
                    i++;
                }
            }

            currentLivingParticles = i;
        }

        /// <inheritdoc />
        public override IEnumerator<Particle> GetEnumerator()
        {
            return new Enumerator(particleList, currentLivingParticles);
        }

    }

}
