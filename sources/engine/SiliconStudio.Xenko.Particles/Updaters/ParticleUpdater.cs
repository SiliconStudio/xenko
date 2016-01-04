// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("ParticleUpdater")]
    public abstract class ParticleUpdater : ParticleModule
    {
//        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);

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
