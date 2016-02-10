// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialColorSeed")]
    [Display("Initial Color by seed")]
    public class InitialColorSeed : ParticleInitializer
    {
        public InitialColorSeed()
        {
            RequiredFields.Add(ParticleFields.Color4);
            RequiredFields.Add(ParticleFields.RandomSeed);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Color4) || !pool.FieldExists(ParticleFields.RandomSeed))
                return;

            var colField = pool.GetField(ParticleFields.Color4);
            var rndField = pool.GetField(ParticleFields.RandomSeed);
            
            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);
                var randSeed = particle.Get(rndField);

                var color = Color4.Lerp(ColorMin, ColorMax, randSeed.GetFloat(RandomOffset.Offset1A + SeedOffset));

                // Premultiply alpha
                // This can't be done in advance for ColorMin and ColorMax because it will change the math
                color.R *= color.A;
                color.G *= color.A;
                color.B *= color.A;

                (*((Color4*)particle[colField])) = color;

                i = (i + 1) % maxCapacity;
            }
        }

        [DataMember(8)]
        [Display("Seed offset")]
        public UInt32 SeedOffset { get; set; } = 0;

        [DataMember(30)]
        [Display("Color min")]
        public Color4 ColorMin { get; set; } = new Color4(1, 1, 1, 1);

        [DataMember(40)]
        [Display("Color max")]
        public Color4 ColorMax { get; set; } = new Color4(1, 1, 1, 1);    
    }
}
