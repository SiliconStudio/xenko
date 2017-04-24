// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles;

namespace ParticlesSample.Particles
{
    public static class CustomParticleFields
    {
        /// <summary>
        /// Custom field for our particle, which defines non-uniform dimensions in 2D space
        /// </summary>
        public static readonly ParticleFieldDescription<Vector2> RectangleXY = new ParticleFieldDescription<Vector2>("RectangleXY", new Vector2(1, 1));
    }
}
