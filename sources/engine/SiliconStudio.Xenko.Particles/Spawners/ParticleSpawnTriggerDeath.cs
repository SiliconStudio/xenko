// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    /// <summary>
    /// <see cref="ParticleSpawnTriggerDeath"/> triggers when the parent particle dies
    /// </summary>
    [DataContract("ParticleSpawnTriggerDeath")]
    [Display("On Death")]
    public class ParticleSpawnTriggerDeath : ParticleSpawnTrigger<float>
    {
        public override void PrepareFromPool(ParticlePool pool)
        {
            if (pool == null)
            {
                FieldAccessor = ParticleFieldAccessor<float>.Invalid();
                return;
            }

            FieldAccessor = pool.GetField(ParticleFields.RemainingLife);
        }

        public unsafe override float HasTriggered(Particle parentParticle)
        {
            if (!FieldAccessor.IsValid())
                return 0f;

            var remainingLifetime = (*((float*)parentParticle[FieldAccessor]));

            return (remainingLifetime <= MathUtil.ZeroTolerance) ? 1f : 0f;
        }
    }
}
