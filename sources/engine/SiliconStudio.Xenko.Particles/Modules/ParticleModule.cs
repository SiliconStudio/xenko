// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("ParticleModule")]
    public abstract class ParticleModule
    {
        [DataMemberIgnore]
        public ParticleModuleType Type { get; protected set; }

//        public Texture Texture { get; set; }

        internal List<ParticleFieldDescription> RequiredFields;

        protected ParticleModule()
        {
            Type = ParticleModuleType.Invalid;
            RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);
        } 
         
        /// <summary>
        /// Updates the module instance in case it has properties which change with time
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        /// <param name="parentSystem">The parent <see cref="ParticleSystem"/> hosting this module</param>
        public void Update(float dt, ParticleSystem parentSystem)
        {
            
        }

        /// <summary>
        /// Override Apply(...) if your module acts as an Updater and change its type to Updater
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        /// <param name="pool">Particle pool to target</param>
        public unsafe virtual void Apply(float dt, ParticlePool pool)
        {
            /*
            // Example - nullify the position's Y coordinate
            if (!pool.FieldExists(ParticleFields.Position))
                return;

            var posField = pool.GetField(ParticleFields.Position);

            foreach (var particle in pool)
            {
                (*((Vector3*)particle[posField])).Y = 0;
            }
            //*/
        }

        /// <summary>
        /// Override Initialize if your module acts as an Initializer and change its type to Initializer
        /// </summary>
        /// <param name="pool">Particle pool to target</param>
        /// <param name="startIdx">Starting index (included from the array)</param>
        /// <param name="endIdx">End index (excluded from the array)</param>
        /// <param name="maxCapacity">Max pool capacity (loops after this point) so that it's possible for (endIdx < startIdx)</param>
        public unsafe virtual void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            /*
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
            //*/
        }
    }

    public enum ParticleModuleType
    {
        /// <summary>
        /// The type of this module is not properly set
        /// </summary>
        Invalid,

        /// <summary>
        /// This module acts as an Updater - it must apply to the pool each frame
        /// </summary>
        Updater,

        /// <summary>
        /// This module acts as an Initializer - it only applies once to each newly spawend particle
        /// </summary>
        Initializer            
    }
}
