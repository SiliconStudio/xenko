// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("ParticleInitializer")]
    public abstract class ParticleInitializer : ParticleModule
    {
//        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);

        /// <summary>
        /// Override Initialize if your module acts as an Initializer and change its type to Initializer
        /// </summary>
        /// <param name="pool">Particle pool to target</param>
        /// <param name="startIdx">Starting index (included from the array)</param>
        /// <param name="endIdx">End index (excluded from the array)</param>
        /// <param name="maxCapacity">Max pool capacity (loops after this point) so that it's possible for (endIdx < startIdx)</param>
        public abstract void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity);
        /*
        {
            // Example - nullify the position's Y coordinate
            if (!pool.FieldExists(ParticleFields.Position))
                return;

            var posField = pool.GetField(ParticleFields.Position);

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);

                (*((Vector3*)particle[posField])).Y = 0;

                i = (i + 1) % maxCapacity;
            }
        }
        //*/
    }
}

