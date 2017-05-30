// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
