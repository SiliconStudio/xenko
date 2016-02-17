// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Particles.Spawners;

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
