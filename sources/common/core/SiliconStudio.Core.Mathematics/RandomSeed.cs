// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

// Random numbers which also allow creation of random values in the shaders and are deterministic
// Based on this article:
// http://martindevans.me/game-development/2015/02/22/Random-Gibberish/

using System;

namespace SiliconStudio.Core.Mathematics
{
    /// <summary>
    /// The <see cref="RandomSeed"/> is a structure for deterministically acquiring random values.
    /// One <see cref="RandomSeed"/> should be able to reproduce the same pseudo-random value for a fixed offset, but
    /// provide enough random distribution for different offsets or different random seeds
    /// Although other methods exist, the current implementation can easily be replicated in the shaders in required
    /// </summary>
    public struct RandomSeed
    {
        private const double GelfondConst = 23.1406926327792690;            // e to the power of Pi = (-1) to the power of -i
        private const double GelfondSchneiderConst = 2.6651441426902251;    // 2 to the power of sqrt(2)
        private const double Numerator = 123456789;

        private readonly UInt32 seed;

        /// <summary>
        /// Create a random seed from a target uint32
        /// </summary>
        /// <param name="seed"></param>
        public RandomSeed(UInt32 seed)
        {
            this.seed = seed;
        }

        /// <summary>
        /// Get a deterministic double value between 0 and 1 based on the seed
        /// </summary>
        /// <returns>Deterministic pseudo-random value between 0 and 1</returns>
        public double GetDouble(UInt32 offset)
        {
            var dRand = (double)(seed + offset);

            var dotProduct = Math.Cos(dRand) * GelfondConst + Math.Sin(dRand) * GelfondSchneiderConst;
            var denominator = 1e-7 + 256 * dotProduct;
            var remainder = Numerator % denominator;

            return (remainder - Math.Floor(remainder));
        }

        /// <summary>
        /// Get a deterministic float value between 0 and 1 based on the seed
        /// The calculations are still made as doubles to prevent underflow errors.
        /// </summary>
        /// <returns>Deterministic pseudo-random value between 0 and 1</returns>
        public double GetFloat(UInt32 offset) => (float)GetDouble(offset);

        /// <summary>
        /// Run some tests to check random distribution, etc.
        /// </summary>
        public static void RunTests()
        {
            const UInt32 maxValue = 0x00FFFFFF;

            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"Random distribution (step of 0.01) for the first {maxValue:D} seeds");
            var distribution = new int[100];
            var seed = new RandomSeed(0);
            for (UInt32 i = 0; i <= maxValue; i++)
            {
                var random = seed.GetDouble(i);
                var bucket = (int)(random * 100);
                distribution[bucket]++;
            }

            foreach (var i in distribution)
            {
                System.Console.Out.WriteLine($"{((double)i/(double)maxValue):##.00000} %");
            }

            var randVals = new double[100];

            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"Random values for seeds 0 - 99, sorted");
            for (UInt32 i = 0; i < 100; i++)
            {
                randVals[i] = seed.GetDouble(i);
            }

            Array.Sort(randVals);
            foreach (var randVal in randVals)
            {
                System.Console.Out.WriteLine($"{randVal}");
            }


            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"Random values for seeds 10000 - 10099, sorted");
            for (UInt32 i = 0; i < 100; i++)
            {
                randVals[i] = seed.GetDouble(i + 10000);
            }

            Array.Sort(randVals);
            foreach (var randVal in randVals)
            {
                System.Console.Out.WriteLine($"{randVal}");
            }

        }
    }
}
