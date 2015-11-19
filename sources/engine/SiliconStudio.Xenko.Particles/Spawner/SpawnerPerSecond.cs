// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Spawner
{
    /// <summary>
    /// A particle spawner which continuously spawns particles. Number of particles to be spawned is given in seconds.
    /// </summary>
    [DataContract("SpawnerPerSecond")]
    [Display("Per second")]
    public sealed class SpawnerPerSecond : SpawnerBase
    {
        [DataMemberIgnore]
        public bool Dirty { get; private set; }

        private float carryOver;

        private float spawnCount;
        [DataMember(40)]
        [Display("Particles/second")]
        public float SpawnCount
        {
            get { return spawnCount; }
            set
            {
                Dirty = true;
                spawnCount = value;
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

        public SpawnerPerSecond()
        {
            Dirty = true;

            spawnCount = 100f;
            carryOver = 0;

            particleMinLifetime = 1f;
            particleMaxLifetime = 1f;

            maxParticlesOverride = 0;
            MaxParticles = 0;
        }

        public override int GetMaxParticles()
        {
            if (!Dirty)
                return MaxParticles;

            Dirty = false;

            if (MaxParticlesOverride > 0)
            {
                MaxParticles = MaxParticlesOverride;
                return MaxParticles;
            }

            var maxCount = particleMaxLifetime * spawnCount;

            // TODO Emitter lifetime, bursts, etc.

            MaxParticles = Math.Max(1, (int) Math.Ceiling(maxCount));

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

                if (*life > particleMaxLifetime)
                    *life = particleMaxLifetime;

                if (*life <= 0 || (*life -= dt) <= 0)
                {
                    particleEnumerator.RemoveCurrent(ref particle);
                }
            }
        }

        public override unsafe void SpawnNew(float dt, ParticlePool pool)
        {
            var lifeField = pool.GetField(ParticleFields.RemainingLife);

            var toSpawn = spawnCount * dt + carryOver;

            toSpawn = Math.Min(pool.AvailableParticles, toSpawn);

            var integerPart = (int)Math.Floor(toSpawn);
            carryOver = toSpawn - integerPart;

            for (var i = 0; i < integerPart; i++)
            {
                var particle = pool.AddParticle();

                *((float*)particle[lifeField]) = particleMaxLifetime; // TODO Random
            }

        }
    }
}

