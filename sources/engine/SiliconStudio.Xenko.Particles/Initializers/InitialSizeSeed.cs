// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialSizeSeed")]
    [Display("Initial Size by seed")]
    public class InitialSizeSeed : ParticleInitializer
    {
        public InitialSizeSeed()
        {
            RequiredFields.Add(ParticleFields.Size);
            RequiredFields.Add(ParticleFields.RandomSeed);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Size) || !pool.FieldExists(ParticleFields.RandomSeed))
                return;

            var sizeField = pool.GetField(ParticleFields.Size);
            var rndField = pool.GetField(ParticleFields.RandomSeed);
            
            var minSize = WorldScale * RandomSize.X;
            var sizeGap = WorldScale * RandomSize.Y - minSize;

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);
                var randSeed = particle.Get(rndField);

                (*((float*)particle[sizeField])) = minSize + sizeGap * randSeed.GetFloat(RandomOffset.Offset1A + SeedOffset);

                i = (i + 1) % maxCapacity;
            }
        }

        [DataMemberIgnore]
        public float WorldScale { get; private set; } = 1f;

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTRS is called. 
        /// This is intentional and reduces overhead, because SetParentTRS is called exactly once/turn.
        /// </summary>
        [DataMember(5)]
        [Display("Inheritance")]
        public InheritLocation InheritLocation { get; set; } = InheritLocation.Scale;

        [DataMember(8)]
        [Display("Seed offset")]
        public UInt32 SeedOffset { get; set; } = 0;

        [DataMember(30)]
        [Display("Random size")]
        public Vector2 RandomSize { get; set; } = new Vector2(0.5f, 1);

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasScl = InheritLocation.HasFlag(InheritLocation.Scale);

            WorldScale = (hasScl) ? Scale : 1f;
        }
    }
}
