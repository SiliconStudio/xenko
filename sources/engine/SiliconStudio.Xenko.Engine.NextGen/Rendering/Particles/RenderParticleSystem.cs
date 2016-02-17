using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Particles;
using SiliconStudio.Xenko.Particles.Components;

namespace SiliconStudio.Xenko.Rendering.Particles
{
    public class RenderParticleSystem : RenderObject
    {
        public ParticleSystemComponent ParticleSystemComponent;

        public ParticleEmitter ParticleEmitter;

        public TransformComponent TransformComponent;
    }
}