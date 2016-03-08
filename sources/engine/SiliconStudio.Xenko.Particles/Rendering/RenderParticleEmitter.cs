using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    [DefaultPipelinePlugin(typeof(ParticleEmitterPipelinePlugin))]
    public class RenderParticleEmitter : RenderObject
    {
        public RenderParticleSystem RenderParticleSystem;

        public ParticleEmitter ParticleEmitter;
        internal ParticleEmitterRenderFeature.ParticleMaterialInfo ParticleMaterialInfo;
    }
}