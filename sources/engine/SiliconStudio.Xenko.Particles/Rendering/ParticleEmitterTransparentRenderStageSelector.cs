// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
