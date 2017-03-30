// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    public class ParticleEmitterTransparentRenderStageSelector : TransparentRenderStageSelector
    {
        public override void Process(RenderObject renderObject)
        {
            if (TransparentRenderStage != null && ((RenderGroupMask)(1U << (int)renderObject.RenderGroup) & RenderGroup) != 0)
            {
                var renderParticleEmitter = (RenderParticleEmitter)renderObject;
                var effectName = renderParticleEmitter.ParticleEmitter.Material.EffectName;

                renderObject.ActiveRenderStages[TransparentRenderStage.Index] = new ActiveRenderStage(effectName);
            }
        }
    }
}