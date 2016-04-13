// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    /// <summary>
    /// A particle spawner which continuously spawns particles. Number of particles to be spawned is given in seconds.
    /// </summary>
    [DataContract("SpawnerFromDistance")]
    [Display("Distance")]
    public sealed class SpawnerFromDistance : ParticleSpawner
    {
        [DataMemberIgnore]
        private float carryOver;

        [DataMemberIgnore]
        private float spawnCount;

        [DataMemberIgnore]
        private bool hasBegun;

        [DataMemberIgnore]
        private Vector3 oldPosition;

        public SpawnerFromDistance()
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
        [Display("Particles/unit")]
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
            {
                hasBegun = false;
                return;
            }

            if (!hasBegun)
            {
                hasBegun = true;
                oldPosition = emitter.CachedParticleSystem.Translation;
                return;
            }

            var totalDistance = (emitter.CachedParticleSystem.Translation - oldPosition).Length();
            oldPosition = emitter.CachedParticleSystem.Translation;

            var toSpawn = spawnCount * totalDistance + carryOver;

            var integerPart = (int)Math.Floor(toSpawn);
            carryOver = toSpawn - integerPart;

            emitter.EmitParticles(integerPart);
        }
    }
}

