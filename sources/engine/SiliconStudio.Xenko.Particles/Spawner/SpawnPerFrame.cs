// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Spawner
{
    /// <summary>
    /// A particle spawner which continuously spawns particles. Number of particles to be spawned is given in seconds.
    /// </summary>
    [DataContract("SpawnPerFrame")]
    [Display("Per frame")]
    public sealed class SpawnPerFrame : SpawnerBase
    {
        [DataMemberIgnore]
        public bool Dirty { get; private set; }

        private float spawnCount;
        [DataMember(40)]
        [Display("Particles/frame")]
        public float SpawnCount
        {
            get { return spawnCount; }
            set
            {
                Dirty = true;
                spawnCount = value;
            }
        }


        private float defaultFramerate = 60;
        [DataMember(45)]
        [Display("Framerate")]
        public float Framerate
        {
            get { return defaultFramerate; }
            set
            {
                Dirty = true;
                defaultFramerate = value;
            }
        }

        private int maxParticlesOverride;
        [DataMember(50)]
        [Display("Maximum particles")]
        public int MaxParticlesOverride
        {
            get { return maxParticlesOverride; }
            set
            {
                Dirty = true;
                maxParticlesOverride = value;
            }
        }

        [DataMemberIgnore]
        public int MaxParticles { get; private set; }

        /// <summary>
        /// Minimum particle lifetime, in seconds. Should be positive and no bigger than <see cref="ParticleMaxLifetime"/>
        /// </summary>
        private float particleMinLifetime;

        [DataMember(60)]
        [Display("Particle's min lifetime")]
        public float ParticleMinLifetime
        {
            get { return particleMinLifetime; }
            set
            {
                if (value <= 0 || value > particleMaxLifetime)
                    return;

                Dirty = true;
                particleMinLifetime = value;
            }
        }

        /// <summary>
        /// Maximum particle lifetime, in seconds. Should be positive and no smaller than <see cref="ParticleMinLifetime"/>
        /// </summary>
        private float particleMaxLifetime;

        [DataMember(65)]
        [Display("Particle's max lifetime")]
        public float ParticleMaxLifetime
        {
            get { return particleMaxLifetime; }
            set
            {
                if (value < particleMinLifetime)
                    return;

                Dirty = true;
                particleMaxLifetime = value;
            }
        }

        public SpawnPerFrame()
        {
            Dirty = true;

            spawnCount = 1f;

            particleMinLifetime = 1f;
            particleMaxLifetime = 1f;

            maxParticlesOverride = 0;
            MaxParticles = 0;
        }

        public override int GetMaxParticles()
        {
            if (!Dirty)
                return MaxParticles;

            if (MaxParticlesOverride > 0)
            {
                MaxParticles = MaxParticlesOverride;
                return MaxParticles;
            }

            var maxCount = particleMaxLifetime * spawnCount * defaultFramerate;
            // TODO Emitter lifetime, bursts, etc.
            MaxParticles = Math.Max(1, (int)maxCount);

            return MaxParticles;
        }


        public override unsafe void RemoveOld(float dt, ParticlePool pool)
        {
            if (!pool.FieldExists(ParticleFields.RemainingLife))
                return;

            var lifeField = pool.GetField(ParticleFields.RemainingLife);

            var particleEnumerator = pool.GetEnumerator();
            while (particleEnumerator.MoveNext())
            {
                var particle = particleEnumerator.Current;
                var life = (float*)particle[lifeField];

                if ((*life > 0) && ((*life -= dt) <= 0))
                {
                    particleEnumerator.RemoveCurrent(ref particle);
                }
            }
        }

        public override unsafe void SpawnNew(float dt, ParticlePool pool)
        {
            var lifeField = pool.GetField(ParticleFields.RemainingLife);

            var toSpawn = spawnCount;

            toSpawn = Math.Min(pool.AvailableParticles, toSpawn);

            for (var i = 0; i < toSpawn; i++)
            {
                var particle = pool.AddParticle();

                *((float*)particle[lifeField]) = particleMaxLifetime; // TODO Random
            }

        }


   //     public static implicit operator SpawnPerSecond(SpawnPerFrame src)
 //       {
//
//        }

    }
}

