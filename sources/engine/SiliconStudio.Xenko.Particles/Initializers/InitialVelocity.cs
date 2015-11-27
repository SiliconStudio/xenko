// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialVelocity")]
    public class InitialVelocity : InitializerBase
    {
        // TODO Change the RNG to a deterministic generator
        readonly Random randomNumberGenerator = new Random();

        public InitialVelocity()
        {
            RequiredFields.Add(ParticleFields.Velocity);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Velocity))
                return;

            var velField = pool.GetField(ParticleFields.Velocity);

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

            leftCorner += WorldPosition;


            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);

                var particleRandVel = leftCorner;
                particleRandVel += xAxis * (float)randomNumberGenerator.NextDouble();
                particleRandVel += yAxis * (float)randomNumberGenerator.NextDouble();
                particleRandVel += zAxis * (float)randomNumberGenerator.NextDouble();

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

        [DataMember(30)]
        [Display("Velocity min")]
        public Vector3 VelocityMin = new Vector3(-1, 1, -1);

        [DataMember(40)]
        [Display("Velocity max")]
        public Vector3 VelocityMax = new Vector3(1, 1, 1);

        [DataMemberIgnore]
        public Vector3 WorldPosition { get; private set; } = new Vector3(0, 0, 0);
        [DataMemberIgnore]
        public Quaternion WorldRotation { get; private set; } = new Quaternion(0, 0, 0, 1);
        [DataMemberIgnore]
        public float WorldScale { get; private set; } = 1f;

        /// <summary>
        /// The rotation relative to the parent transformation.
        /// </summary>
        /// <userdoc>The rotation of the entity with regard to its parent</userdoc>
        [DataMember(12)]
        public Quaternion Rotation = new Quaternion(0, 0, 0, 1);

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasPos = InheritLocation.HasFlag(Particles.InheritLocation.Position);
            var hasRot = InheritLocation.HasFlag(Particles.InheritLocation.Rotation);
            var hasScl = InheritLocation.HasFlag(Particles.InheritLocation.Scale);

            WorldScale = (hasScl) ? Scale : 1f;

            WorldRotation = (hasRot) ? this.Rotation * Rotation : this.Rotation;

            WorldPosition = (hasPos) ? Translation : new Vector3(0, 0, 0);
        }
    }
}
