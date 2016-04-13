// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles.Spawners
{
    /// <summary>
    /// <see cref="ParticleSpawnTrigger"/> governs the condition under which particle emission is triggered for spawners like <see cref="SpawnerFromParent"/>
    /// </summary>
    public abstract class ParticleSpawnTrigger
    {
        /// <summary>
        /// Prepares fields accessors before the 
        /// </summary>
        /// <param name="pool"></param>
        public abstract void PrepareFromPool(ParticlePool pool);

        /// <summary>
        /// For positive values, shows if the condition was met and how much is the magnitude
        /// </summary>
        /// <param name="parentParticle">Parent particle which attributes are used for testing</param>
        /// <returns></returns>
        public abstract float HasTriggered(Particle parentParticle);
    }

    /// <inheritdoc/>
    public abstract class ParticleSpawnTrigger<T> : ParticleSpawnTrigger where T : struct
    {
        protected ParticleFieldAccessor<T> FieldAccessor;         
    }
}
