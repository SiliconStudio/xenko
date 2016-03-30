// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Particles.Updaters;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    /// <summary>
    /// A particle spawner which continuously spawns particles. Number of particles to be spawned is given in seconds.
    /// </summary>
    [DataContract("SpawnerFromParent")]
    [Display("From parent")]
    public sealed class SpawnerFromParent : ParticleSpawner
    {
        /// <summary>
        /// Referenced parent emitter
        /// </summary>
        [DataMemberIgnore]
        protected ParticleEmitter Parent;

        private string parentName;

        /// <summary>
        /// <c>true</c> is the parent's name has changed or the particle system has been invalidated
        /// </summary>
        private bool isParentNameDirty = true;

        [DataMember(12)]
        [Display("Parent emitter")]
        public string ParentName
        {
            get { return parentName; }
            set
            {
                parentName = value;
                isParentNameDirty = true;
            }
        }


        /// <summary>
        /// Some initializers require fine control between parent and child emitters. Use the control group to assign such meta-fields.
        /// </summary>
        [DataMember(13)]
        [Display("Spawn Control Group")]
        public ParentControlFlag ParentControlFlag
        {
            get { return parentControlFlag; }
            set
            {
                RemoveControlGroup();
                parentControlFlag = value;
                AddControlGroup();
            }
        }
        private ParentControlFlag parentControlFlag = ParentControlFlag.Group00;

        /// <summary>
        /// Removes the old required control group field from the parent emitter's pool
        /// </summary>
        private void RemoveControlGroup()
        {
            var groupIndex = (int)parentControlFlag;
            if (groupIndex >= ParticleFields.ChildrenFlags.Length)
                return;

            Parent?.RemoveRequiredField(ParticleFields.ChildrenFlags[groupIndex]);
        }

        /// <summary>
        /// Adds the required control group field to the parent emitter's pool
        /// </summary>
        private void AddControlGroup()
        {
            var groupIndex = (int)parentControlFlag;
            if (groupIndex >= ParticleFields.ChildrenFlags.Length)
                return;

            Parent?.AddRequiredField(ParticleFields.ChildrenFlags[groupIndex]);
        }

        /// <summary>
        /// Gets a field accessor to the parent emitter's spawn control field, if it exists
        /// </summary>
        /// <returns></returns>
        protected ParticleFieldAccessor<ParticleChildrenAttribute> GetSpawnControlField()
        {
            var groupIndex = (int)parentControlFlag;
            if (groupIndex >= ParticleFields.ChildrenFlags.Length)
                return ParticleFieldAccessor<ParticleChildrenAttribute>.Invalid();

            return Parent?.Pool?.GetField(ParticleFields.ChildrenFlags[groupIndex]) ?? ParticleFieldAccessor<ParticleChildrenAttribute>.Invalid();
        }

        [DataMemberIgnore]
        private float carryOver;

        [DataMemberIgnore]
        private float spawnCount;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SpawnerFromParent()
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
        public unsafe override void SpawnNew(float dt, ParticleEmitter emitter)
        {
            if (isParentNameDirty)
            {
                RemoveControlGroup();

                Parent = emitter.CahcedParticleSystem?.GetEmitterByName(ParentName);

                AddControlGroup();

                isParentNameDirty = false;
            }

            var spawnerState = GetUpdatedState(dt, emitter);
            if (spawnerState != SpawnerState.Active)
                return;

            // Get parent pool
            if (Parent == null) return;

            var parentPool = Parent.Pool;
            var parentParticlesCount = parentPool.LivingParticles;
            if (parentParticlesCount == 0) return;

            var spawnControlGroup = GetSpawnControlField();
            if (!spawnControlGroup.IsValid())
                return;

            var collisionControlFieldParent = parentPool.GetField(ParticleFields.CollisionControl);

            int totalParticlesToEmit = 0;

            foreach (var parentParticle in parentPool)
            {
                var parentEventTriggered = false;
                ParticleChildrenAttribute childrenAttribute = ParticleChildrenAttribute.Empty;

                // Trigger event by parent's surface collision
                if (collisionControlFieldParent.IsValid())
                {
                    var collisionAttribute = (*((ParticleCollisionAttribute*)parentParticle[collisionControlFieldParent]));
                    parentEventTriggered |= collisionAttribute.HasColided;
                }


                uint particlesToEmit = 0;
                if (parentEventTriggered)
                {
                    // TODO Not-hardcoded
                    particlesToEmit = 5;
                }

                childrenAttribute.ParticlesToEmit = particlesToEmit;
                totalParticlesToEmit += (int)particlesToEmit;


                (*((ParticleChildrenAttribute*)parentParticle[spawnControlGroup])) = childrenAttribute;
            }

            emitter.EmitParticles(totalParticlesToEmit);
        }

        /// <inheritdoc />
        public override void InvalidateRelations()
        {
            base.InvalidateRelations();

            RemoveControlGroup();
            
            Parent = null;
            isParentNameDirty = true;
        }
    }
}
