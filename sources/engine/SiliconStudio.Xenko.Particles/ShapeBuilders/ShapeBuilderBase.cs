// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    [DataContract("ShapeBuilderBase")]
    public abstract class ShapeBuilderBase
    {
        public abstract int BuildVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, ref int remainingCapacity,
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticlePool pool);

        public abstract int QuadsPerParticle { get; protected set; }
    }
}
