// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialSpawnOrder")]
    [Display("Spawn Order")]
    public class InitialSpawnOrder : ParticleInitializer
    {
        // Will loop every so often, but the loop condition should be almost unreachable for normal games (~800 hours for spawning rate of 100 particles/second)
        private UInt32 spawnOrder = 0;

        public InitialSpawnOrder()
        {
            spawnOrder = 0;

            RequiredFields.Add(ParticleFields.Order);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Order))
                return;

            var orderField = pool.GetField(ParticleFields.Order);

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);


                (*((UInt32*)particle[orderField])) = spawnOrder++;

                i = (i + 1) % maxCapacity;
            }
        }
    }
}
