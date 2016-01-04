// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    /// <summary>
    /// Initializer which sets the initial velocity for particles based on RandomSeed information
    /// </summary>
    [DataContract("InitialVelocitySeed")]
    [Display("Initial Velocity by seed")]
    public class InitialVelocitySeed : ParticleInitializer
    {
        public InitialVelocitySeed()
        {
            RequiredFields.Add(ParticleFields.Velocity);
            RequiredFields.Add(ParticleFields.RandomSeed);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Velocity) || !pool.FieldExists(ParticleFields.RandomSeed))
                return;

            var velField = pool.GetField(ParticleFields.Velocity);
            var rndField = pool.GetField(ParticleFields.RandomSeed);

            var leftCorner = VelocityMin * WorldScale;
            var xAxis = new Vector3(VelocityMax.X * WorldScale - leftCorner.X, 0, 0);
            var yAxis = new Vector3(0, VelocityMax.Y * WorldScale - leftCorner.Y, 0);
            var zAxis = new Vector3(0, 0, VelocityMax.Z * WorldScale - leftCorner.Z);

            if (!WorldRotation.IsIdentity)
            {
                WorldRotation.Rotate(ref leftCorner);
                WorldRotation.Rotate(ref xAxis);
                WorldRotation.Rotate(ref yAxis);
                WorldRotation.Rotate(ref zAxis);
            }

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);
                var randSeed = particle.Get(rndField);

                var particleRandVel = leftCorner;
                particleRandVel += xAxis * randSeed.GetFloat(RandomOffset.Offset3A + SeedOffset);
                particleRandVel += yAxis * randSeed.GetFloat(RandomOffset.Offset3B + SeedOffset);
                particleRandVel += zAxis * randSeed.GetFloat(RandomOffset.Offset3C + SeedOffset);

                (*((Vector3*)particle[velField])) = particleRandVel;

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
        [Display("Velocity min")]
        public Vector3 VelocityMin { get; set; } = new Vector3(-1, 1, -1);

        [DataMember(40)]
        [Display("Velocity max")]
        public Vector3 VelocityMax { get; set; } = new Vector3(1, 1, 1);

        [DataMemberIgnore]
        public Quaternion WorldRotation { get; private set; } = new Quaternion(0, 0, 0, 1);
        [DataMemberIgnore]
        public float WorldScale { get; private set; } = 1f;

        [DataMemberIgnore]
        private Vector3 WorldPosition;

        /// <summary>
        /// The rotation relative to the parent transformation.
        /// </summary>
        /// <userdoc>The rotation of the entity with regard to its parent</userdoc>
        [DataMember(12)]
        public Quaternion Rotation { get; set; } = new Quaternion(0, 0, 0, 1);

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasRot = InheritLocation.HasFlag(InheritLocation.Rotation);
            var hasScl = InheritLocation.HasFlag(InheritLocation.Scale);

            WorldPosition = Translation;

            WorldScale = (hasScl) ? Scale : 1f;

            WorldRotation = (hasRot) ? this.Rotation * Rotation : this.Rotation;
        }

        public override bool TryGetDebugDrawShape(ref DebugDrawShape debugDrawShape, ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            debugDrawShape = DebugDrawShape.Cube;

            rotation = WorldRotation;

            scale = (VelocityMax - VelocityMin);
            translation = (VelocityMax + VelocityMin) * 0.5f * WorldScale;

            scale *= WorldScale;
            rotation.Rotate(ref translation);
            translation += WorldPosition;

            return true;
        }

    }
}
