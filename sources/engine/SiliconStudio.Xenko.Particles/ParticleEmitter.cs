// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using SiliconStudio.Core.Collections;
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
            
        }

        /// <summary>
        /// Should be called before <see cref="ApplyParticleUpdaters"/> to ensure dead particles are removed before they are updated
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private void MoveAndDeleteParticles(float dt)
        {
            
        }

        /// <summary>
        /// Should be called before <see cref="SpawnNewParticles"/> to ensure new particles are not moved the frame they spawn
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private void ApplyParticleUpdaters(float dt)
        {
            modules.ForEach(module => module.Apply(dt, pool));
        }

        /// <summary>
        /// Spawns new particles and in general should be one of the last methods to call from the <see cref="Update"/> method
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private void SpawnNewParticles(float dt)
        {
            
        }

        #endregion

        #region Fields

        private readonly Dictionary<ParticleFieldDescription, int> requiredFields;

        private bool AddRequiredField(ParticleFieldDescription description)
        {
            var fieldReferences = 0;
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

                // TODO Remove field from the pool

                requiredFields.Remove(description);
                return;
            }

            Debug.Assert(false, "Module is trying to remove a field which doesn't exist!");
        }

        #endregion

    }
}
