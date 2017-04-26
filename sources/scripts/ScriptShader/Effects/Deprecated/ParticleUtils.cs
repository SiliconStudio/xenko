// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Rendering
{
    static class ParticleUtils
    {
        public static int CalculateMaximumPowerOf2Count(int value)
        {
            return (int)Math.Pow(2.0, Math.Ceiling(Math.Log(value, 2)));
        }
    }
}
