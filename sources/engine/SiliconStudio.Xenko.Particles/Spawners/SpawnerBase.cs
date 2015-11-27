// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    [DataContract("SpawnerBase")]
    public abstract class SpawnerBase 
    {
        [DataMemberIgnore]
        protected ParticleEmitter emitter = null;

        protected void MarkAsDirty()
        {
            if (emitter != null)
            {
                emitter.Dirty = true;
            }
        }

        public virtual void SpawnNew(float dt, ParticleEmitter emitter)
        {
            // emitter.EmitParticles(0);

            if (this.emitter != null)
                return;

            this.emitter = emitter;
            emitter.Dirty = true;            
        }

        public abstract int GetMaxParticlesPerSecond();
    }
}
