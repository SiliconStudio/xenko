// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles
{
    public static class RandomOffset
    {
        /// <summary>
        /// Lifetime offset should always be 0 so that it can easily be retrieved from the random seed.
        /// </summary>
        public const uint Lifetime = 0;

        /// <summary>
        /// Random seed offset used for coupling 1-dimensional random values
        /// </summary>
        public const uint Offset1A = 1112;

        /// <summary>
        /// Random seed offset used for coupling 2-dimensional random values
        /// </summary>
        public const uint Offset2A = 2223;
        public const uint Offset2B = 3334;

        /// <summary>
        /// Random seed offset used for coupling 3-dimensional random values
        /// </summary>
        public const uint Offset3A = 4445;
        public const uint Offset3B = 5556;
        public const uint Offset3C = 6667;

    }

    public class ParticleRandomSeedGenerator
    {
        private uint rngSeed;

        public ParticleRandomSeedGenerator(uint seed)
        {
            rngSeed = seed;
        }

        public double GetNextDouble() => GetNextSeed().GetDouble(0);

        public RandomSeed GetNextSeed()
        {
            return new RandomSeed(unchecked(rngSeed++)); // We want it to overflow
        }
    }
}
