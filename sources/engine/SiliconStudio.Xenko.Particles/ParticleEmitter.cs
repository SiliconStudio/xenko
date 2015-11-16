// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Modules;

namespace SiliconStudio.Xenko.Particles
{
    public class ParticleEmitter
    {
        private readonly ParticlePool pool;
         
        public ParticleEmitter()
        {
            // Create an empty pool. We only need one and are going to reuse it
            pool = new ParticlePool(0, 0);
            requiredFields = new Dictionary<ParticleFieldDescription, int>();

            modules = new List<ParticleModule>();

            // This is always required. Maybe later other cases will be added.
            AddRequiredField(ParticleFields.RemainingLife);
        }

        #region Modules

        private readonly List<ParticleModule> modules;

        public void AddModule(ParticleModule module)
        {
            var allFieldsAdded = true;
            module.RequiredFields.ForEach(desc => allFieldsAdded &= AddRequiredField(desc));

            if (!allFieldsAdded)
            {
                module.RequiredFields.ForEach(RemoveRequiredField);
                return;
            }

            modules.Add(module);
        }

        public void RemoveModule(ParticleModule module)
        {
            if (!modules.Contains(module))
                return;

            module.RequiredFields.ForEach(RemoveRequiredField);

            modules.Remove(module);
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the emitter and all its particles, and applies all updaters and spawners.
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        /// <param name="parentSystem">The parent <see cref="ParticleSystem"/> hosting this emitter</param>
        public void Update(float dt, ParticleSystem parentSystem)
        {
            EnsurePoolCapacity();

            MoveAndDeleteParticles(dt);

            ApplyParticleUpdaters(dt);

            SpawnNewParticles(dt);
        }

        /// <summary>
        /// Should be called before the other methods from <see cref="Update"/> to ensure the pool has sufficient capacity to handle all particles.
        /// </summary>
        private void EnsurePoolCapacity()
        {
            // TODO Resize pool and add/remove fields
            if (pool.ParticleCapacity < 100)
                pool.SetCapacity(100);
        }

        /// <summary>
        /// Should be called before <see cref="ApplyParticleUpdaters"/> to ensure dead particles are removed before they are updated
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private unsafe void MoveAndDeleteParticles(float dt)
        {
            // This module is pretty much fixed. It updates particles' lifetime and position
            var lifeFieldExists = pool.FieldExists(ParticleFields.RemainingLife);
            var moveFieldsExist = pool.FieldExists(ParticleFields.Position) && pool.FieldExists(ParticleFields.Velocity);

            // Neither combination is available - early out
            // Note! Since we always add Life field, we can't return here, but the check exists for future updates.
            if (!lifeFieldExists && !moveFieldsExist)
                return;

            if (!lifeFieldExists)
            {
                // Position and velocity update only
                var posField = pool.GetField(ParticleFields.Position);
                var velField = pool.GetField(ParticleFields.Velocity);

                foreach (var particle in pool)
                {
                    var pos = ((Vector3*)particle[posField]);
                    var vel = ((Vector3*)particle[velField]);

                    *pos += *vel * dt;
                }
            }
            else if (!moveFieldsExist)
            {
                // Lifetime update only
                var lifeField = pool.GetField(ParticleFields.RemainingLife);

                var particleEnumerator = pool.GetEnumerator();
                while (particleEnumerator.MoveNext())
                {
                    var particle = particleEnumerator.Current;
                    var life = (float*)particle[lifeField];

                    if ((*life > 0) && ((*life -= dt) <= 0))
                    {
                        particleEnumerator.RemoveCurrent(ref particle);
                        continue;
                    }
                }
            }
            else
            {
                // Both lifetime and movement updates
                var lifeField = pool.GetField(ParticleFields.RemainingLife);
                var posField = pool.GetField(ParticleFields.Position);
                var velField = pool.GetField(ParticleFields.Velocity);

                var particleEnumerator = pool.GetEnumerator();
                while (particleEnumerator.MoveNext())
                {
                    var particle = particleEnumerator.Current;
                    var life = (float*)particle[lifeField];

                    if ((*life > 0) && ((*life -= dt) <= 0))
                    {
                        particleEnumerator.RemoveCurrent(ref particle);
                        continue;
                    }

                    var pos = ((Vector3*)particle[posField]);
                    var vel = ((Vector3*)particle[velField]);

                    *pos += *vel * dt;
                }
            }
        }

        /// <summary>
        /// Should be called before <see cref="SpawnNewParticles"/> to ensure new particles are not moved the frame they spawn
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private void ApplyParticleUpdaters(float dt)
        {
            foreach (var module in modules)
            {
                if (module.Type == ParticleModule.ModuleType.Updater)
                    module.Apply(dt, pool);
            }
        }

        /// <summary>
        /// Spawns new particles and in general should be one of the last methods to call from the <see cref="Update"/> method
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private unsafe void SpawnNewParticles(float dt)
        {
            var lifeField = pool.GetField(ParticleFields.RemainingLife);

            var spawnCount = pool.ParticleCapacity - pool.ParticleCount;
            spawnCount = Math.Min((int) (100*dt), spawnCount);

            var capacity = pool.ParticleCapacity;
            var startIndex = pool.NextFreeIndex % capacity;
            for (var i = 0; i < spawnCount; i++)
            {
                var particle = pool.AddParticle();

                *((float*)particle[lifeField]) = 1;
            }
            var endIndex = pool.NextFreeIndex % capacity;

            foreach (var module in modules)
            {
                if (module.Type == ParticleModule.ModuleType.Initializer)
                    module.Initialize(pool, startIndex, endIndex, capacity);
            }
        }

        #endregion

        #region Fields

        private readonly Dictionary<ParticleFieldDescription, int> requiredFields;

        private bool AddRequiredField(ParticleFieldDescription description)
        {
            int fieldReferences;
            if (requiredFields.TryGetValue(description, out fieldReferences))
            {
                // Field already exists. Increase the reference by 1
                requiredFields[description] = fieldReferences + 1;
                return true;
            }

            // Check if the pool doesn't already have too many fields
            if (requiredFields.Count >= ParticlePool.DefaultMaxFielsPerPool)
                return false;

            if (!pool.FieldExists(description, forceCreate: true))
                return false;

            requiredFields.Add(description, 1);
            return true;
        }

        private void RemoveRequiredField(ParticleFieldDescription description)
        {
            int fieldReferences;
            if (requiredFields.TryGetValue(description, out fieldReferences))
            {
                requiredFields[description] = fieldReferences - 1;

                // If this was not the last field, other modules are still using it so don't remove it from the pool
                if (fieldReferences > 1)
                    return;

                pool.RemoveField(description);

                requiredFields.Remove(description);
                return;
            }

            // This line can be reached when a AddModule was unsuccessful and the required fields should be cleaned up
        }

        #endregion

    }
}
