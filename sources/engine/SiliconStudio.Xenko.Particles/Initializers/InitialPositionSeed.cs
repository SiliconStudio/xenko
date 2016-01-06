// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialPositionSeed")]
    [Display("Initial Position by seed")]
    public class InitialPositionSeed : ParticleInitializer
    {
        public InitialPositionSeed()
        {
            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.RandomSeed);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.RandomSeed))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var rndField = pool.GetField(ParticleFields.RandomSeed);

            var leftCorner = PositionMin * WorldScale;
            var xAxis = new Vector3(PositionMax.X * WorldScale.X - leftCorner.X, 0, 0);
            var yAxis = new Vector3(0, PositionMax.Y * WorldScale.Y - leftCorner.Y, 0);
            var zAxis = new Vector3(0, 0, PositionMax.Z * WorldScale.Z - leftCorner.Z);

            if (!WorldRotation.IsIdentity)
            {
                WorldRotation.Rotate(ref leftCorner);
                WorldRotation.Rotate(ref xAxis);
                WorldRotation.Rotate(ref yAxis);
                WorldRotation.Rotate(ref zAxis);
            }

            leftCorner += WorldPosition;


            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);
                var randSeed = particle.Get(rndField);

                var particleRandPos = leftCorner;

                particleRandPos += xAxis * randSeed.GetFloat(RandomOffset.Offset3A + SeedOffset);
                particleRandPos += yAxis * randSeed.GetFloat(RandomOffset.Offset3B + SeedOffset);
                particleRandPos += zAxis * randSeed.GetFloat(RandomOffset.Offset3C + SeedOffset);

                (*((Vector3*)particle[posField])) = particleRandPos;

                i = (i + 1) % maxCapacity;
            }
        }

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTRS is called. 
        /// This is intentional and reduces overhead, because SetParentTRS is called exactly once/turn.
        /// </summary>
        [DataMember(5)]
        [Display("Inheritance")]
        public InheritLocation InheritLocation { get; set; } = InheritLocation.Position | InheritLocation.Rotation | InheritLocation.Scale;

        [DataMember(8)]
        [Display("Seed offset")]
        public UInt32 SeedOffset { get; set; } = 0;

        [DataMember(30)]
        [Display("Position min")]
        public Vector3 PositionMin { get; set; } = new Vector3(-1, 1, -1);

        [DataMember(40)]
        [Display("Position max")]
        public Vector3 PositionMax { get; set; } = new Vector3(1, 1, 1);
 
        public override bool TryGetDebugDrawShape(ref DebugDrawShape debugDrawShape, ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            debugDrawShape = DebugDrawShape.Cube;

            rotation = WorldRotation;

            scale = (PositionMax - PositionMin);
            translation = (PositionMax + PositionMin) * 0.5f * WorldScale;

            scale *= WorldScale;
            rotation.Rotate(ref translation);
            translation += WorldPosition;

            return true;
        }
    }
}
