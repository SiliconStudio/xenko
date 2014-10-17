// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Particles
{
    /// <summary>
    /// Common particle fields.
    /// </summary>
    public static class ParticleFields
    {
        /// <summary>
        /// A particle field description for the particle position.
        /// </summary>
        public static readonly ParticleFieldDescription<Vector3> Position = new ParticleFieldDescription<Vector3>("Position");

        /// <summary>
        /// A particle field description for the particle acceleration.
        /// </summary>
        public static readonly ParticleFieldDescription<Vector3> Acceleration = new ParticleFieldDescription<Vector3>("Acceleration");

        /// <summary>
        /// A particle field description for the particle velocity.
        /// </summary>
        public static readonly ParticleFieldDescription<Vector3> Velocity = new ParticleFieldDescription<Vector3>("Velocity");

        /// <summary>
        /// A particle field description for the particle Orientation (for billboard only).
        /// </summary>
        public static readonly ParticleFieldDescription<float> Angle = new ParticleFieldDescription<float>("Angle", 0.0f);

        /// <summary>
        /// A particle field description for the particle angular velocity.
        /// </summary>
        public static readonly ParticleFieldDescription<float> AngularVelocity = new ParticleFieldDescription<float>("AngularVelocity", 0.0f);

        /// <summary>
        /// A particle field description for the particle angular acceleration.
        /// </summary>
        public static readonly ParticleFieldDescription<float> AngularAcceleration = new ParticleFieldDescription<float>("AngularAcceleration", 0.0f);

        /// <summary>
        /// A particle field description for the particle size.
        /// </summary>
        public static readonly ParticleFieldDescription<Vector2> Size = new ParticleFieldDescription<Vector2>("Size", Vector2.One);

        /// <summary>
        /// A particle field description for the particle color.
        /// </summary>
        public static readonly ParticleFieldDescription<Color4> Color = new ParticleFieldDescription<Color4>("Color", SiliconStudio.Core.Mathematics.Color4.White);

        /// <summary>
        /// A particle field description for the particle Orientation (Euler angles).
        /// </summary>
        public static readonly ParticleFieldDescription<Vector3> Rotation = new ParticleFieldDescription<Vector3>("Rotation", SiliconStudio.Core.Mathematics.Vector3.Zero);

        /// <summary>
        /// Particle field storing the particle current lifetime (age).
        /// </summary>
        public static readonly ParticleFieldDescription<float> Lifetime = new ParticleFieldDescription<float>("Lifetime");
    }
}