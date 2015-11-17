// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Particles.Spawner
{
    public class ParticleSpawner
    {
        public ParticleSpawner()
        {
            Dirty = true;

            spawnCondition = SpawnConditionEnum.PerSecond;
            spawnCount = 100f;

            particleMinLifetime = 1f;
            particleMaxLifetime = 1f;

            maxParticlesOverride = 0;
            MaxParticles = 0;
        }

        public bool Dirty { get; private set; }

        public enum SpawnConditionEnum
        {
            /// <summary>
            /// 
            /// </summary>
            PerSecond,

            PerMeter,

            Once,

            OnHit,

            PerFrame

        }
        private SpawnConditionEnum spawnCondition;
        public SpawnConditionEnum SpawnCondition
        {
            get { return spawnCondition; }
            set
            {
                Dirty = true;
                spawnCondition = value;
            }
        }

        private float spawnCount;
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
        public int MaxParticlesOverride
        {
            get { return maxParticlesOverride; }
            set
            {
                Dirty = true;
                maxParticlesOverride = value;
            }
        }

        public int MaxParticles { get; private set; }

        /// <summary>
        /// Minimum particle lifetime, in seconds. Should be positive and no bigger than <see cref="ParticleMaxLifetime"/>
        /// </summary>
        private float particleMinLifetime;
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

        public int CalculateMaxParticles()
        {
            if (!Dirty)
                return MaxParticles;

            if (MaxParticlesOverride > 0)
            {
                MaxParticles = MaxParticlesOverride;
                return MaxParticles;
            }

            switch (spawnCondition)
            {
                case SpawnConditionEnum.PerSecond:
                {
                    var maxCount = particleMaxLifetime*spawnCount;
                    // TODO Emitter lifetime, bursts, etc.
                    MaxParticles = Math.Max(1, (int)maxCount);
                }
                break;

                case SpawnConditionEnum.PerMeter:
                {
                    // We can't really know the emitter's speed in advance, so we assume it to be 1m/s. If you need, you can override it.
                    var maxCount = particleMaxLifetime*spawnCount;
                    MaxParticles = Math.Max(1, (int)maxCount);
                }
                break;

                case SpawnConditionEnum.Once:
                {
                    // TODO Emitter lifetime should be taken into account
                    var maxCount = spawnCount;
                    MaxParticles = Math.Max(1, (int)maxCount);
                }
                break;

                case SpawnConditionEnum.OnHit:
                {
                    // TODO Emitter lifetime should be taken into account
                    var maxCount = spawnCount;
                    MaxParticles = Math.Max(1, (int)maxCount);                    
                }
                break;

                case SpawnConditionEnum.PerFrame:
                {
                    var assumedFrameRate = 60;
                    var maxCount = particleMaxLifetime * spawnCount * assumedFrameRate;
                    // TODO Emitter lifetime, bursts, etc.
                    MaxParticles = Math.Max(1, (int)maxCount);                    
                }
                break;
            }

            return MaxParticles;
        }

        public unsafe void UpdateAndRemoveDead(float dt, ParticlePool pool)
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

        public unsafe void SpawnNew(float dt, ParticlePool pool)
        {
            var lifeField = pool.GetField(ParticleFields.RemainingLife);

            var toSpawn = 0f;

            switch (spawnCondition)
            {
                case SpawnConditionEnum.PerFrame:
                    toSpawn = spawnCount;
                    break;

                case SpawnConditionEnum.PerSecond:
                    toSpawn = spawnCount * dt;
                    break;

                case SpawnConditionEnum.OnHit:
                    toSpawn = 0; // TODO Handle collisions
                    break;

                case SpawnConditionEnum.Once:
                    toSpawn = 0; // TODO Handle lifetime cycles
                    break;

                case SpawnConditionEnum.PerMeter:
                    toSpawn = 0; // TODO Handle distances
                    break;
            }

            toSpawn = Math.Min(pool.ParticleCapacity - pool.ParticleCount, toSpawn);

            for (var i = 0; i < toSpawn; i++)
            {
                var particle = pool.AddParticle();

                *((float*)particle[lifeField]) = particleMaxLifetime; // TODO Random
            }

        }
    }
}
