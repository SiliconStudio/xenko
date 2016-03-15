// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.Materials
{
    /// <summary>
    /// Base class for building and animating the texture coordinates in a particle vertex buffer stream
    /// </summary>
    [DataContract("UVBuilder")]
    public abstract class UVBuilder
    {
        /// <summary>
        /// Enhances or animates the texture coordinates using already existing base coordinates of (0, 0, 1, 1) or similar
        /// (base texture coordinates may differ depending on the actual shape)
        /// </summary>
        /// <param name="vertexBuilder">Target vertex buffer builder to use</param>
        /// <param name="sorter"><see cref="ParticleSorter"/> to use to iterate over all particles drawn this frame</param>
        /// <param name="texCoordsDescription">Attribute description of the texture coordinates in the current vertex layout</param>
        public abstract void BuildUVCoordinates(ParticleVertexBuilder vertexBuilder, ParticleSorter sorter, AttributeDescription texCoordsDescription);
    }
}
