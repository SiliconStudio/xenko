// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Particles.Updaters;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    /// <summary>
    /// <see cref="ParticleSpawnTriggerLifetime"/> triggers when the parent particle's remaining lifetime is within the specified limit
    /// </summary>
    [DataContract("ParticleSpawnTriggerLifetime")]
    [Display("Lifetime")]
    public class ParticleSpawnTriggerLifetime : ParticleSpawnTrigger<float>
    {
        private bool limitsAreInOrder;

        /// <summary>
        /// If the parent particle is younger than the lower limit, it won't spawn children. When the lower limit is higher than the upper limit the condition is reversed.
        /// </summary>
        [DataMember(10)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Lower Limit")]
        public float LifetimeLowerLimit { get; set; } = 0f;

        /// <summary>
        /// If the parent particle is older than the upper limit, it won't spawn children. When the upper limit is smaller than the lower limit the condition is reversed.
        /// </summary>
        [DataMember(20)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Upper Limit")]
        public float LifetimeUpperLimit { get; set; } = 1f;

        public override void PrepareFromPool(ParticlePool pool)
        {
            limitsAreInOrder = (LifetimeLowerLimit <= LifetimeUpperLimit);

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

            // We store remaining lifetime in the particle field, so for progress [0..1) we need to take (1 - remaining)
            var currentLifetime = 1f - (*((float*)parentParticle[FieldAccessor]));

            // TODO - Time difference ?
            return ((currentLifetime >= LifetimeLowerLimit) ^ (currentLifetime <= LifetimeUpperLimit) ^ limitsAreInOrder) ? 1f : 0f;
        }
    }
}
