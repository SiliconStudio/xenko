// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    /// <summary>
    /// The <see cref="InitialSpawnOrder"/> is an initializer which assigns all particles an increasing number based on the order of their spawning
    /// </summary>
    [DataContract("InitialSpawnOrder")]
    [Display("Spawn Order")]
    public class InitialSpawnOrder : ParticleInitializer
    {
        // Will loop every so often, but the loop condition should be unreachable for normal games (~800 hours for spawning rate of 100 particles/second)
        private uint spawnOrder = 0;

        /// <inheritdoc />
        internal override void ResetSimulation()
        {
            spawnOrder = 0;
        }

        /// <summary>
        /// Default constructor which also registers the fields required by this updater
        /// </summary>
        public InitialSpawnOrder()
        {
            spawnOrder = 0;

            RequiredFields.Add(ParticleFields.Order);
        }

        /// <inheritdoc />
        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Order))
                return;

            var orderField = pool.GetField(ParticleFields.Order);
            var childOrderField = pool.GetField(ParticleFields.ChildOrder);

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);

                (*((uint*)particle[orderField])) = spawnOrder++; // Will loop every so often, but the loop condition should be unreachable for normal games

                if (childOrderField.IsValid())
                    (*((uint*)particle[childOrderField])) = 0;

                i = (i + 1) % maxCapacity;
            }
        }
    }
}
