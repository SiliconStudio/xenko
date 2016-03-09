// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Particles.Components;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    /// <summary>
    /// Defines a particle system to render.
    /// </summary>
    public class RenderParticleSystem
    {
        public ParticleSystemComponent ParticleSystemComponent;

        public TransformComponent TransformComponent;

        public RenderParticleEmitter[] Emitters;
    }
}