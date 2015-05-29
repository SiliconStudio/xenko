using System;

namespace SiliconStudio.Paradox.Rendering
{
    static class ParticleUtils
    {
        public static int CalculateMaximumPowerOf2Count(int value)
        {
            return (int)Math.Pow(2.0, Math.Ceiling(Math.Log(value, 2)));
        }
    }
}