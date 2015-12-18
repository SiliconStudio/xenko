// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    [DataContract("ShapeBuilderBase")]
    public abstract class ShapeBuilderBase
    {
        public abstract int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY, 
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter);

        public abstract int QuadsPerParticle { get; protected set; }

        [DataMemberIgnore]
        public bool VertexLayoutHasChanged { get; protected set; } = true;

        public virtual void PrepareForDraw(ParticleVertexBuilder vertexBuilder, ParticleSorter sorter)
        {
            // Check if ParticleVertexElements should be changed and set VertexLayoutHasChanged = true; if they do
        }

        public virtual void UpdateVertexBuilder(ParticleVertexBuilder vertexBuilder)
        {
            // You can add ParticleVertexElements here

            VertexLayoutHasChanged = false;
        }
    }
}
