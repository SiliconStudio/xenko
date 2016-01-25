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
                        particleList[i].Particle = particle;
                        i++;
                    }
                }
            }
            else
            {
                foreach (var particle in ParticlePool)
                {
                    particleList[i].Particle = particle;
                    i++;
                }
            }

            currentLivingParticles = i;
        }

        public override IEnumerator<Particle> GetEnumerator()
        {
            return new Enumerator(particleList, currentLivingParticles);
        }

    }

}
