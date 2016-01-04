// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    /// <summary>
    /// A particle spawner which continuously spawns particles. Number of particles to be spawned is given in seconds.
    /// </summary>
    [DataContract("SpawnerPerFrame")]
    [Display("Per frame")]
    public sealed class SpawnerPerFrame : ParticleSpawner
    {
        private float carryOver;

        private float spawnCount;
        [DataMember(40)]
        [Display("Particles/frame")]
        public float SpawnCount
        {
            get { return spawnCount; }
            set
            {
                MarkAsDirty();
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
                MarkAsDirty();
                defaultFramerate = value;
            }
        }

        public override int GetMaxParticlesPerSecond()
        {
            return (int)Math.Ceiling(SpawnCount * defaultFramerate);
        }


        public SpawnerPerFrame()
        {
            spawnCount = 1f;
            carryOver = 0;
        }

        public override void SpawnNew(float dt, ParticleEmitter emitter)
        {
            var spawnerState = GetUpdatedState(dt, emitter);
            if (spawnerState != SpawnerState.Active)
                return;

            var toSpawn = spawnCount + carryOver;

            var integerPart = (int)Math.Floor(toSpawn);
            carryOver = toSpawn - integerPart;

            emitter.EmitParticles(integerPart);
        }

    }
}

