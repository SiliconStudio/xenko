// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialRotationSeed")]
    [Display("Initial Rotation by seed")]
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

        [DataMember(8)]
        [Display("Seed offset")]
        public UInt32 SeedOffset { get; set; } = 0;

        // Positive value is a clockwise rotation
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

        // Positive value is a clockwise rotation
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

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale) { }
    }
}
