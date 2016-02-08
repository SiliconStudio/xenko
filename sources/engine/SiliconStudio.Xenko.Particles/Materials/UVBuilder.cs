// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("UVBuilder")]
    public abstract class UVBuilder
    {
        public abstract void BuildUVCoordinates(ParticleVertexBuilder vertexBuilder, ParticleSorter sorter, AttributeDescription texCoordsDescription);
    }
}
