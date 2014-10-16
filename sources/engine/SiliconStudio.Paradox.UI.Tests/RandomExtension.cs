// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.UI.Tests
{
    internal static class RandomExtension
    {
        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        public static Vector3 NextVector3(this Random random)
        {
            return new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat());
        }

        public static Thickness NextThickness(this Random random, float leftFactor, float topFactor, float backFactor, float rightFactor, float bottomFactor, float frontFactor)
        {
            return new Thickness(
                random.NextFloat() * leftFactor, random.NextFloat() * topFactor, random.NextFloat() * backFactor,
                random.NextFloat() * rightFactor, random.NextFloat() * bottomFactor, random.NextFloat() * frontFactor);
        }
    }
}
