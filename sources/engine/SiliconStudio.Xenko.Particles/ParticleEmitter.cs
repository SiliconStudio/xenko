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
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Initializers;
using SiliconStudio.Xenko.Particles.Materials;
using SiliconStudio.Xenko.Particles.Modules;
using SiliconStudio.Xenko.Particles.ShapeBuilders;
using SiliconStudio.Xenko.Particles.Spawner;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("ParticleEmitter")]
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
            pool = new ParticlePool(0, 0);
            requiredFields = new Dictionary<ParticleFieldDescription, int>();
            AddRequiredField(ParticleFields.RemainingLife); // TODO Maybe not add the Life field always? Could depend on the Spawner?

            Initializers = new TrackingCollection<InitializerBase>();
            Initializers.CollectionChanged += ModulesChanged;

            Updaters = new TrackingCollection<UpdaterBase>();
            Updaters.CollectionChanged += ModulesChanged;
        }

        #region Modules

        [DataMember(200)]
        [Display("Initializers", Expand = ExpandRule.Always)]
        [NotNullItems]
        [MemberCollection(CanReorderItems = true)]
        public readonly TrackingCollection<InitializerBase> Initializers;

        [DataMember(300)]
        [Display("Updaters", Expand = ExpandRule.Always)]
        [NotNullItems]
        [MemberCollection(CanReorderItems = true)]
        public readonly TrackingCollection<UpdaterBase> Updaters;

        private void ModulesChanged(object sender, TrackingCollectionChangedEventArgs e)
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

        #endregion

        #region Update

        /// <summary>
        /// Updates the emitter and all its particles, and applies all updaters and spawners.
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        /// <param name="parentSystem">The parent <see cref="ParticleSystem"/> hosting this emitter</param>
        public void Update(float dt, ParticleSystem parentSystem)
        {
            // Update sub-systems
            foreach (var initializer in Initializers)
            {
                initializer.SetParentTRS(ref parentSystem.Translation, ref parentSystem.Rotation, parentSystem.UniformScale);
            }

            foreach (var updater in Updaters)
            {
                updater.SetParentTRS(ref parentSystem.Translation, ref parentSystem.Rotation, parentSystem.UniformScale);
            }

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
            // Serializer workaround. ParticleSpawner = new SpawnerPerSecond(); cannot be initialized in the constructor because it causes problems with the serialization
            if (ParticleSpawner == null)
                ParticleSpawner = new SpawnerPerSecond();

            pool.SetCapacity(ParticleSpawner.GetMaxParticles());
        }

        /// <summary>
        /// Should be called before <see cref="ApplyParticleUpdaters"/> to ensure dead particles are removed before they are updated
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        private unsafe void MoveAndDeleteParticles(float dt)
        {
            ParticleSpawner.RemoveOld(dt, pool);

            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.Velocity))
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

        /// <summary>
        /// Add a particle field required by some dependent module. If the module already exists in the pool, only its reference counter is increased.
        /// </summary>
        /// <param name="description"></param>
        private void AddRequiredField(ParticleFieldDescription description)
        {
            int fieldReferences;
            if (requiredFields.TryGetValue(description, out fieldReferences))
            {
                // Field already exists. Increase the reference counter by 1
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

        /// <summary>
        /// Remove a particle field no longer required by a dependent module. It only gets removed from the pool if it reaches 0 reference counters.
        /// </summary>
        /// <param name="description"></param>
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

        #region Rendering

        [DataMember(40)]
        [Display("Shape")]
        [NotNull]
        public ShapeBuilderBase ShapeBuilder;

        [DataMember(50)]
        [Display("Material")]
        [NotNull]
        public ParticleMaterialBase Material;

        public int BuildVertexBuffer(IntPtr vertexBuffer, Vector3 invViewX, Vector3 invViewY, ref int remainingCapacity)
        {
            if (ShapeBuilder == null)
                ShapeBuilder = new BillboardBuilder();

            return ShapeBuilder.BuildVertexBuffer(vertexBuffer, invViewX, invViewY, ref remainingCapacity, pool);
        }

        #endregion

    }
}
