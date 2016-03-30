// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

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
        /// Order of the particle, which can be based on spawn order or something else
        /// </summary>
        public static readonly ParticleFieldDescription<uint> ChildrenFlags = new ParticleFieldDescription<uint>("ChildrenFlags0001", 0);   // TODO User should be able to set this


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

        [DataMember(2)]
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
                InvalidateRelations();

                Parent = emitter.CahcedParticleSystem?.GetEmitterByName(ParentName);
                if (Parent != null)
                    Parent.AddRequiredField(ChildrenFlags);

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

            var childrenFlagsFieldParent = parentPool.GetField(ChildrenFlags);
            if (!childrenFlagsFieldParent.IsValid()) return;

            var collisionControlFieldParent = parentPool.GetField(ParticleFields.CollisionControl);

            int totalParticlesToEmit = 0;

            foreach (var parentParticle in parentPool)
            {
                uint childrenFlag = 0x0;

                uint particlesToEmit = 5;   // TODO Not-hardcoded
                if (collisionControlFieldParent.IsValid())
                {
                    var collisionControlFlag = (*((uint*)parentParticle[collisionControlFieldParent]));
                    if ((collisionControlFlag & 0x0001) == 0)
                        particlesToEmit = 0;
                }

                particlesToEmit &= 0xFFFF;  // TODO Not-hardcoded limit

                childrenFlag = particlesToEmit;


                (*((uint*)parentParticle[childrenFlagsFieldParent])) = childrenFlag;

                totalParticlesToEmit += (int) particlesToEmit;
            }

            emitter.EmitParticles(totalParticlesToEmit);
        }

        /// <inheritdoc />
        public /*override*/ void InvalidateRelations()
        {
//            base.InvalidateRelations();

            if (Parent != null)
                Parent.RemoveRequiredField(ChildrenFlags);
            
            Parent = null;
            isParentNameDirty = true;
        }
    }
}
