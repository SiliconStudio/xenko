// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Modules;
using SiliconStudio.Xenko.Particles.Spawner;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("ParticleEmitter")]
    [Category]
    public class ParticleEmitter
    {
        [DataMember(30)]
        [NotNull]
        public SpawnerBase ParticleSpawner;

        // Exposing for debug drawing
        [DataMemberIgnore]
        public readonly ParticlePool pool;
         
        public ParticleEmitter()
        {
            // Create an empty pool. We only need one and are going to reuse it
            pool = new ParticlePool(0, 0);
            requiredFields = new Dictionary<ParticleFieldDescription, int>();

            modules = new SafeList<ParticleModule>();

            // The standard spawner requires a lifetime field

            // NOTE If the member is initialized in the constructor, trying to change it later results in an exception:
            // Exception: InvalidCastException: Unable to cast object of type 'SiliconStudio.Xenko.Particles.Spawner.SpawnPerSecond' to type 'SiliconStudio.Xenko.Particles.Spawner.SpawnPerFrame'.
            //ParticleSpawner = new SpawnPerSecond();
            AddRequiredField(ParticleFields.RemainingLife);

            AddRequiredField(ParticleFields.Position);
            AddRequiredField(ParticleFields.Velocity);

        }

        #region Modules

        [DataMember(50)]
        [Category]
        [Display("Modules", Expand = ExpandRule.Always)]
        [NotNullItems] // Can't create non-derived classes if this attribute is set
        [MemberCollection(CanReorderItems = true)]
        public readonly SafeList<ParticleModule> modules;

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
            // TODO Maybe add/remove fields here

            if (ParticleSpawner == null)
                ParticleSpawner = new SpawnPerSecond();

            pool.SetCapacity(ParticleSpawner.GetMaxParticles());
        }

        /// <summary>
        /// Should be called before <see cref="ApplyParticleUpdaters"/> to ensure dead particles are removed before they are updated
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private unsafe void MoveAndDeleteParticles(float dt)
        {
            ParticleSpawner.RemoveOld(dt, pool);

            if (!pool.FieldExists(ParticleFields.Position) && pool.FieldExists(ParticleFields.Velocity))
                return;

            // should this be a separate module?
            // Position and velocity update only
            var posField = pool.GetField(ParticleFields.Position);
            var velField = pool.GetField(ParticleFields.Velocity);

            foreach (var particle in pool)
            {
            //    var pos = ((Vector3*)particle[posField]);
            //    var vel = ((Vector3*)particle[velField]);

            //    *pos += *vel * dt;

                var position = particle.Get(posField);
                var velocity = particle.Get(velField);

                position += velocity*dt;

                particle.Set(posField, position);
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
                if (module.Type == ParticleModuleType.Updater)
                    module.Apply(dt, pool);
            }
        }

        /// <summary>
        /// Spawns new particles and in general should be one of the last methods to call from the <see cref="Update"/> method
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private unsafe void SpawnNewParticles(float dt)
        {
            var capacity = pool.ParticleCapacity;
            var startIndex = pool.NextFreeIndex % capacity;

            ParticleSpawner.SpawnNew(dt, pool);

            var endIndex = pool.NextFreeIndex % capacity;

            foreach (var module in modules)
            {
                if (module.Type == ParticleModuleType.Initializer)
                    module.Initialize(pool, startIndex, endIndex, capacity);
            }

            Random randomNumberGenerator = new Random();

            // TEST
            var posField = pool.GetField(ParticleFields.Position);
            var velField = pool.GetField(ParticleFields.Velocity);

            var startPos = new Vector3(0, 0, 0);
            var startVel = new Vector3(0, 0, 0);

            var i = startIndex;
            while (i != endIndex)
            {
                var particle = pool.FromIndex(i);

                (*((Vector3*)particle[posField])) = startPos;

                startVel.X = ((float)randomNumberGenerator.NextDouble() * 4 - 2);
                startVel.Y = ((float)randomNumberGenerator.NextDouble() * 2 + 2);
                startVel.Z = ((float)randomNumberGenerator.NextDouble() * 4 - 2);
                (*((Vector3*)particle[velField])) = startVel;

                i = (i + 1) % capacity;
            }
            //*/
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
