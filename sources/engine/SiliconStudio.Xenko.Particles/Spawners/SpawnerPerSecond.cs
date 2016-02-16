// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    /// <summary>
    /// A particle spawner which continuously spawns particles. Number of particles to be spawned is given in seconds.
    /// </summary>
    [DataContract("SpawnerPerSecond")]
    [Display("Per second")]
    public sealed class SpawnerPerSecond : ParticleSpawner
    {
        [DataMemberIgnore]
        private float carryOver;

        [DataMemberIgnore]
        private float spawnCount;

        public SpawnerPerSecond()
        {
            spawnCount = 100f;
            carryOver = 0;
        }


        /// <summary>
        /// The amount of particles this spawner will emit over one second, every second
        /// </summary>
        /// <userdoc>
        /// The amount of particles this spawner will emit over one second, every second
        /// </userdoc>
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

        /// <inheritdoc />
        public override int GetMaxParticlesPerSecond()
        {
            return (int)Math.Ceiling(SpawnCount);
        }
        
        /// <inheritdoc />
        public override void SpawnNew(float dt, ParticleEmitter emitter)
        {
            var spawnerState = GetUpdatedState(dt, emitter);
            if (spawnerState != SpawnerState.Active)
                return;

            var toSpawn = spawnCount * dt + carryOver;

            var integerPart = (int)Math.Floor(toSpawn);
            carryOver = toSpawn - integerPart;

            emitter.EmitParticles(integerPart);
        }
    }
}

