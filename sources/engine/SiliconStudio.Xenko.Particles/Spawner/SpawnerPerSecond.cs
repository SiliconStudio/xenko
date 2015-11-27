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
        private float carryOver;

        private float spawnCount;
        [DataMember(40)]
        [Display("Particles/second")]
        public float SpawnCount
        {
            get { return spawnCount; }
            set
            {
                MarkAsDirty();
                spawnCount = value;
            }
        }

        public SpawnerPerSecond()
        {
            spawnCount = 100f;
            carryOver = 0;
        }

        public override int GetMaxParticlesPerSecond()
        {
            return (int)Math.Ceiling(SpawnCount);
        }
        
        public override void SpawnNew(float dt, ParticleEmitter emitter)
        {
            base.SpawnNew(dt, emitter);

            var toSpawn = spawnCount * dt + carryOver;

            var integerPart = (int)Math.Floor(toSpawn);
            carryOver = toSpawn - integerPart;

            emitter.EmitParticles(integerPart);
        }
    }
}

