// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    /// <summary>
    /// The <see cref="ShapeBuilder"/> base class is responsible for generating shapes (procedural mesh) ready for rendering from the particle data
    /// </summary>
    [DataContract("ShapeBuilder")]
    public abstract class ShapeBuilder
    {
        /// <summary>
        /// Returns the number of quads required per particle to draw all particles. Assuming 1 Quad = 4 Vertices = 6 Indices
        /// </summary>
        [DataMemberIgnore]
        public abstract int QuadsPerParticle { get; protected set; }

        /// <summary>
        /// Indicates that the required vertex layout has changed and <see cref="UpdateVertexBuilder"/> should be called
        /// </summary>
        [DataMemberIgnore]
        public bool VertexLayoutHasChanged { get; protected set; } = true;

        /// <summary>
        /// Builds the actual vertex buffer for the current frame using the particle data
        /// </summary>
        /// <param name="vtxBuilder">Target vertex buffer builder</param>
        /// <param name="invViewX">Unit vector X (right) in camera space, extracted from the inverse view matrix</param>
        /// <param name="invViewY">Unit vector Y (up) in camera space, extracted from the inverse view matrix</param>
        /// <param name="spaceTranslation">Translation of the target draw space in regard to the particle data (world or local)</param>
        /// <param name="spaceRotation">Rotation of the target draw space in regard to the particle data (world or local)</param>
        /// <param name="spaceScale">Uniform scale of the target draw space in regard to the particle data (world or local)</param>
        /// <param name="sorter">Particle enumerator which can be iterated and returns sported particles</param>
        /// <returns></returns>
        public abstract int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY, 
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter);

        /// <summary>
        /// Check if ParticleVertexElements should be changed and set HasVertexLayoutChanged = true; if they do
        /// </summary>
        /// <param name="vertexBuilder">Target vertex buffer stream builder which will be used for the current frame</param>
        /// <param name="sorter">Enumerator which accesses all particles in a sorted manner</param>
        public virtual void PrepareForDraw(ParticleVertexBuilder vertexBuilder, ParticleSorter sorter)
        {
            // Check if ParticleVertexElements should be changed and set HasVertexLayoutChanged = true; if they do
        }

        /// <summary>
        /// Should be invoked if the <see cref="VertexLayoutHasChanged"/> was <c>true</c> so that new layout fields can be added to the buffer builder
        /// </summary>
        /// <param name="vertexBuilder">Target vertex buffer stream builder which will be used for the current frame</param>
        public virtual void UpdateVertexBuilder(ParticleVertexBuilder vertexBuilder)
        {
            // You can add ParticleVertexElements here

            VertexLayoutHasChanged = false;
        }

        /// <summary>
        /// Sets the required quads per particle and number of particles so that a sufficiently big buffer can be allocated
        /// </summary>
        /// <param name="quadsPerParticle">Required quads per particle, assuming 1 quad = 4 vertices = 6 indices</param>
        /// <param name="livingParticles">Number of living particles this frame</param>
        /// <param name="totalParticles">Number of total number of possible particles for the parent emitter</param>
        public virtual void SetRequiredQuads(int quadsPerParticle, int livingParticles, int totalParticles) { }

        /// <summary>
        /// Finds the circumcenter coordinates for triangle ABC
        /// </summary>
        public static Vector3 Circumcenter(ref Vector3 A, ref Vector3 B, ref Vector3 C)
        {
            var a = A - C;
            var b = B - C;

            var crossAB = Vector3.Cross(a, b);
            var lenAB = crossAB.LengthSquared();
            if (lenAB < MathUtil.ZeroTolerance)
                return C;

            return C + Vector3.Cross(a.LengthSquared() * b - b.LengthSquared() * a, crossAB) / (2 * lenAB);
        }
    }
}
