// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    /// <summary>
    /// Defines a particle emitter to render.
    /// </summary>
    public class RenderParticleEmitter : RenderObject
    {
        public RenderParticleSystem RenderParticleSystem;

        public ParticleEmitter ParticleEmitter;
        internal ParticleEmitterRenderFeature.ParticleMaterialInfo ParticleMaterialInfo;

        public bool HasVertexBufferChanged;
        public int VertexSize;
        public int VertexCount;
    }
}