// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Initializers;
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

            Initializers = new TrackingCollection<InitializerBase>();
            Initializers.CollectionChanged += UpdatersChanged;

            Updaters = new TrackingCollection<UpdaterBase>();
            Updaters.CollectionChanged += UpdatersChanged;

            // The standard spawner requires a lifetime field

            // NOTE If the member is initialized in the constructor, trying to change it later results in an exception:
            // Exception: InvalidCastException: Unable to cast object of type 'SiliconStudio.Xenko.Particles.Spawner.SpawnPerSecond' to type 'SiliconStudio.Xenko.Particles.Spawner.SpawnPerFrame'.
            //ParticleSpawner = new SpawnPerSecond();
            AddRequiredField(ParticleFields.RemainingLife);

            //AddRequiredField(ParticleFields.Position);
            //AddRequiredField(ParticleFields.Velocity);

        }

        #region Updaters

        [DataMember(40)]
        [Display("Initializers", Expand = ExpandRule.Always)]
        [NotNullItems]
        [MemberCollection(CanReorderItems = true)]
        public readonly TrackingCollection<InitializerBase> Initializers;

        [DataMember(50)]
        [Display("Updaters", "Description", Expand = ExpandRule.Always)]
        [NotNullItems]
        [MemberCollection(CanReorderItems = true)]
        public readonly TrackingCollection<UpdaterBase> Updaters;


        private void UpdatersChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var module = e.Item as ParticleModuleBase;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    module?.RequiredFields.ForEach(AddRequiredField);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    module?.RequiredFields.ForEach(RemoveRequiredField);
                    break;
            }
        }

        /*
        public void AddModule(ParticleModule module)
        {
            var allFieldsAdded = true;
            module.RequiredFields.ForEach(desc => allFieldsAdded &= AddRequiredField(desc));

            if (!allFieldsAdded)
            {
                module.RequiredFields.ForEach(RemoveRequiredField);
                return;
            }

            Updaters.Add(module);
        }

        public void RemoveModule(ParticleModule module)
        {
            if (!Updaters.Contains(module))
                return;

            module.RequiredFields.ForEach(RemoveRequiredField);

            Updaters.Remove(module);
        }
        //*/

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
                var pos = ((Vector3*)particle[posField]);
                var vel = ((Vector3*)particle[velField]);

                *pos += *vel * dt;
            }
        }

        /// <summary>
        /// Should be called before <see cref="SpawnNewParticles"/> to ensure new particles are not moved the frame they spawn
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private void ApplyParticleUpdaters(float dt)
        {
            foreach (var updater in Updaters)
            {
                updater.Update(dt, pool);
            }
        }

        /// <summary>
        /// Spawns new particles and in general should be one of the last methods to call from the <see cref="Update"/> method
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private void SpawnNewParticles(float dt)
        {
            var capacity = pool.ParticleCapacity;
            var startIndex = pool.NextFreeIndex % capacity;

            ParticleSpawner.SpawnNew(dt, pool);

            var endIndex = pool.NextFreeIndex % capacity;

            foreach (var initializer in Initializers)
            {
                initializer.Initialize(pool, startIndex, endIndex, capacity);
            }
        }

        #endregion

        #region Fields

        private readonly Dictionary<ParticleFieldDescription, int> requiredFields;

        private void AddRequiredField(ParticleFieldDescription description)
        {
            int fieldReferences;
            if (requiredFields.TryGetValue(description, out fieldReferences))
            {
                // Field already exists. Increase the reference by 1
                requiredFields[description] = fieldReferences + 1;
                return;
            }

            // Check if the pool doesn't already have too many fields
            if (requiredFields.Count >= ParticlePool.DefaultMaxFielsPerPool)
                return;

            if (!pool.FieldExists(description, forceCreate: true))
                return;

            requiredFields.Add(description, 1);
        }

        private void RemoveRequiredField(ParticleFieldDescription description)
        {
            int fieldReferences;
            if (requiredFields.TryGetValue(description, out fieldReferences))
            {
                requiredFields[description] = fieldReferences - 1;

                // If this was not the last field, other Updaters are still using it so don't remove it from the pool
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
