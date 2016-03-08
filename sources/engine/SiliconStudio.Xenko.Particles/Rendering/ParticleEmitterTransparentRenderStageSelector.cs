using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    public class ParticleEmitterTransparentRenderStageSelector : TransparentRenderStageSelector
    {
        public override void Process(RenderObject renderObject)
        {
            var renderParticleEmitter = (RenderParticleEmitter)renderObject;
            var effectName = renderParticleEmitter.ParticleEmitter.Material.EffectName;

            var renderStage = TransparentRenderStage;
            renderObject.ActiveRenderStages[renderStage.Index] = new ActiveRenderStage(effectName);
        }
    }
}