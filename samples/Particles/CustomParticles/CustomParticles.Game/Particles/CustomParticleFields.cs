// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles;

namespace CustomParticles.Particles
{
    public static class CustomParticleFields
    {
        /// <summary>
        /// Custom field for our particle, which defines non-uniform dimensions in 2D space
        /// </summary>
        public static readonly ParticleFieldDescription<Vector2> RectangleXY = new ParticleFieldDescription<Vector2>("RectangleXY", new Vector2(1, 1));
    }
}
