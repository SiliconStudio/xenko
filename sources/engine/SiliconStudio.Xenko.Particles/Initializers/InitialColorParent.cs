// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Spawners;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    /// <summary>
    /// The <see cref="InitialColorParent"/> is an initializer which sets the particle's initial velocity at the time of spawning
    /// </summary>
    [DataContract("InitialColorParent")]
    [Display("Color from parent")]
    public class InitialColorParent : ParticleChildInitializer
    {
        /// <summary>
        /// Default constructor which also registers the fields required by this updater
        /// </summary>
        public InitialColorParent()
        {
            RequiredFields.Add(ParticleFields.Color);
            RequiredFields.Add(ParticleFields.RandomSeed);

            // DisplayPosition = true; // Always inherit the position and don't allow to opt out
            DisplayParticleRotation = true;
            DisplayParticleScaleUniform = true;
        }

        /// <inheritdoc />
        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            var parentPool = Parent?.Pool;
            var parentParticlesCount = parentPool?.LivingParticles ?? 0;
            var colorFieldParent = parentPool?.GetField(ParticleFields.Color) ?? ParticleFieldAccessor<Color4>.Invalid();
            if (!colorFieldParent.IsValid())
            {
                parentParticlesCount = 0;
            }

            if (!pool.FieldExists(ParticleFields.Color) || !pool.FieldExists(ParticleFields.RandomSeed))
                return;

            var spawnControlField = GetSpawnControlField();

            var colorField = pool.GetField(ParticleFields.Color4);
            var rndField = pool.GetField(ParticleFields.RandomSeed);

            var sequentialParentIndex = 0;
            var sequentialParentParticles = 0;
            var parentIndex = 0;

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);
                var randSeed = particle.Get(rndField);

                var color = Color4.Lerp(ColorMin, ColorMax, randSeed.GetFloat(RandomOffset.Offset1A + SeedOffset));

                
                if (parentParticlesCount > 0)
                {
                    var parentParticleColor = new Color4(1, 1, 1, 1);

                    // It changes here
                    if (spawnControlField.IsValid())
                    {
                        while (sequentialParentParticles == 0)
                        {
                            if (sequentialParentIndex >= parentParticlesCount)
                                return; // Early out - or should we continue; ?

                            parentIndex = sequentialParentIndex;
                            var tempParentParticle = parentPool.FromIndex(parentIndex);
                            sequentialParentIndex++;

                            var childrenAttribute = (*((ParticleChildrenAttribute*)tempParentParticle[spawnControlField]));

                            sequentialParentParticles = (int)childrenAttribute.ParticlesToEmit;
                        }

                        sequentialParentParticles--;

                        var parentParticle = parentPool.FromIndex(parentIndex);
                        parentParticleColor = (*((Color4*)parentParticle[colorFieldParent]));
                    }
                    else
                    {
                        parentIndex = (int)(parentParticlesCount * randSeed.GetFloat(RandomOffset.Offset1A + ParentSeedOffset));
                        var parentParticle = parentPool.FromIndex(parentIndex);

                        parentParticleColor = (*((Color4*)parentParticle[colorFieldParent]));
                    }

                    color *= parentParticleColor;
                }

                // Premultiply alpha
                // This can't be done in advance for ColorMin and ColorMax because it will change the math
                color.R *= color.A;
                color.G *= color.A;
                color.B *= color.A;

                (*((Color4*)particle[colorField])) = color;

                i = (i + 1) % maxCapacity;
            }
        }

        /// <summary>
        /// The seed offset used to match or separate random values
        /// </summary>
        /// <userdoc>
        /// The seed offset used to match or separate random values
        /// </userdoc>
        [DataMember(20)]
        [Display("Random Seed")]
        public uint SeedOffset { get; set; } = 0;

        /// <summary>
        /// The first color to interpolate from
        /// </summary>
        /// <userdoc>
        /// The first color to interpolate from
        /// </userdoc>
        [DataMember(30)]
        [Display("Color A")]
        public Color4 ColorMin { get; set; } = new Color4(1, 1, 1, 1);

        /// <summary>
        /// The second color to interpolate to
        /// </summary>
        /// <userdoc>
        /// The second color to interpolate to
        /// </userdoc>
        [DataMember(40)]
        [Display("Color B")]
        public Color4 ColorMax { get; set; } = new Color4(1, 1, 1, 1);
    }
}
