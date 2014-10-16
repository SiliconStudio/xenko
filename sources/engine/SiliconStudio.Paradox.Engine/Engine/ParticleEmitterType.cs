// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Engine
{
    public enum ParticleEmitterType
    {
        /// <summary>
        /// Particles data are uploaded/updated by the CPU at each frame. But count is static.
        /// </summary>
        CpuStatic,

        /// <summary>
        /// Particles data are uploaded/updated by the CPU at each frame. Count can changed at each frame.
        /// </summary>
        CpuDynamic,

        /// <summary>
        /// Particles are managed by the GPU, with a static set of particles (no dynamic emitters).
        /// </summary>
        /// <remarks>
        /// <see cref="ParticleEmitterComponent.Shader"/> must be non null for GPU particles.
        /// </remarks>
        GpuStatic,

        /// <summary>
        /// Particles are managed by the GPU and is a dynamic emitter.
        /// </summary>
        /// <remarks>
        /// <see cref="ParticleEmitterComponent.Shader"/> must be non null for GPU particles.
        /// </remarks>
        GpuDynamic
    }
}