// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    /// <summary>
    /// The <see cref="InitialPositionSeed"/> is an initializer which sets the particle's initial position at the time of spawning
    /// </summary>
    [DataContract("InitialPositionSeed")]
    [Display("Initial Position")]
    public class InitialPositionSeed : ParticleInitializer
    {
        /// <summary>
        /// Default constructor which also registers the fields required by this updater
        /// </summary>
        public InitialPositionSeed()
        {
            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.RandomSeed);

            // DisplayPosition = true; // Always inherit the position and don't allow to opt out
            DisplayParticleRotation = true;
            DisplayParticleScaleUniform = true;
        }

        /// <inheritdoc />
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
        /// The seed offset used to match or separate random values
        /// </summary>
        /// <userdoc>
        /// The seed offset used to match or separate random values
        /// </userdoc>
        [DataMember(8)]
        [Display("Random Seed")]
        public uint SeedOffset { get; set; } = 0;

        /// <summary>
        /// The left bottom back corner of the box
        /// </summary>
        /// <userdoc>
        /// The left bottom back corner of the box
        /// </userdoc>
        [DataMember(30)]
        [Display("Position min")]
        public Vector3 PositionMin { get; set; } = new Vector3(-1, 1, -1);

        /// <summary>
        /// The right upper front corner of the box
        /// </summary>
        /// <userdoc>
        /// The right upper front corner of the box
        /// </userdoc>
        [DataMember(40)]
        [Display("Position max")]
        public Vector3 PositionMax { get; set; } = new Vector3(1, 1, 1);

        /// <summary>
        /// Should this Particle Module's bounds be displayed as a debug draw
        /// </summary>
        /// <userdoc>
        /// Display the Particle Module's bounds as a wireframe debug shape. Temporary feature (will be removed later)!
        /// </userdoc>
        [DataMember(-1)]
        [DefaultValue(false)]
        public bool DebugDraw { get; set; } = false;

        /// <inheritdoc />
        public override bool TryGetDebugDrawShape(out DebugDrawShape debugDrawShape, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            if (!DebugDraw)
                return base.TryGetDebugDrawShape(out debugDrawShape, out translation, out rotation, out scale);

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
