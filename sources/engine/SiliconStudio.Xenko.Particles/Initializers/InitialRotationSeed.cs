// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialRotationSeed")]
    [Display("Initial Rotation")]
    public class InitialRotationSeed : ParticleInitializer
    {
        public InitialRotationSeed()
        {
            RequiredFields.Add(ParticleFields.Angle);
            RequiredFields.Add(ParticleFields.RandomSeed);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Angle) || !pool.FieldExists(ParticleFields.RandomSeed))
                return;

            var rotField = pool.GetField(ParticleFields.Angle);
            var rndField = pool.GetField(ParticleFields.RandomSeed);

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);
                var randSeed = particle.Get(rndField);

                (*((float*)particle[rotField])) = angularRotationStart + angularRotationStep * randSeed.GetFloat(RandomOffset.Offset1A + SeedOffset);

                i = (i + 1) % maxCapacity;
            }
        }

        /// <summary>
        /// The seed offset used to match or separate random values
        /// </summary>
        /// <userdoc>
        /// The seed offset used to match or separate random values
        /// </userdoc>
        [DataMember(8)]
        [Display("Seed offset")]
        public UInt32 SeedOffset { get; set; } = 0;

        /// <summary>
        /// Angular rotation in degrees, positive value means clockwise
        /// </summary>
        /// <userdoc>
        /// Angular rotation in degrees, positive value means clockwise
        /// </userdoc>
        [DataMember(30)]
        [Display("Angle (degrees) min")]
        public float AngularRotationMin
        {
            get { return angularRotationMin; }
            set
            {
                angularRotationMin = value;
                angularRotationStart = MathUtil.DegreesToRadians(angularRotationMin);
                angularRotationStep  = MathUtil.DegreesToRadians(angularRotationMax - angularRotationMin);
            }
        }

        /// <summary>
        /// Angular rotation in degrees, positive value means clockwise
        /// </summary>
        /// <userdoc>
        /// Angular rotation in degrees, positive value means clockwise
        /// </userdoc>
        [DataMember(40)]
        [Display("Angle (degrees) max")]
        public float AngularRotationMax
        {
            get { return angularRotationMax; }
            set
            {
                angularRotationMax = value;
                angularRotationStart = MathUtil.DegreesToRadians(angularRotationMin);
                angularRotationStep  = MathUtil.DegreesToRadians(angularRotationMax - angularRotationMin);
            }
        }

        private float angularRotationMin = -60f;
        private float angularRotationMax = 60f;
        private float angularRotationStart = MathUtil.DegreesToRadians(-60f);
        private float angularRotationStep  = MathUtil.DegreesToRadians(120);
    }
}
