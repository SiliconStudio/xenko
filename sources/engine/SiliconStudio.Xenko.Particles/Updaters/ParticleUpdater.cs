// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("ParticleUpdater")]
    public abstract class ParticleUpdater : ParticleModule
    {
        /// <summary>
        /// All updaters are called exactly once during each <see cref="ParticleEmitter"/>'s update.
        /// Most updaters are called before spawning the new particles for the frame, but post updaters are called after that.
        /// </summary>
        /// <userdoc>
        /// Most updaters are called before spawning the new particles for the frame, but post updaters are called after that.
        /// This is important when the updater needs to verify a value even after it was initialized for the first time.
        /// </userdoc>
        [DataMemberIgnore]
        public virtual bool IsPostUpdater => false;

        /// <summary>
        /// Updates all particles in the <see cref="ParticlePool"/> using this updater
        /// </summary>
        /// <param name="dt">Delta time in seconds which has passed since the last update call</param>
        /// <param name="pool">The target <see cref="ParticlePool"/> which needs to be updated</param>
        public abstract void Update(float dt, ParticlePool pool);
        /*
        {
            // Example - nullify the position's Y coordinate
            if (!pool.FieldExists(ParticleFields.Position))
                return;

            var posField = pool.GetField(ParticleFields.Position);

            foreach (var particle in pool)
            {
                (*((Vector3*)particle[posField])).Y = 0;
            }
        }
        //*/
    }
}
