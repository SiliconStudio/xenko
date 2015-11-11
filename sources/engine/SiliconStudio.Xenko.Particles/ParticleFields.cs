// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles
{
    public static class ParticleFields
    {
        /// <summary>
        /// Particle position in 3D space
        /// </summary>
        public static readonly ParticleFieldDescription<Vector3> Position = new ParticleFieldDescription<Vector3>("Position", new Vector3(0, 0, 0));

        /// <summary>
        /// Particle velocity in 3D space
        /// </summary>
        public static readonly ParticleFieldDescription<Vector3> Velocity = new ParticleFieldDescription<Vector3>("Velocity", new Vector3(0, 0, 0));

        /// <summary>
        /// Particle uniform size
        /// </summary>
        public static readonly ParticleFieldDescription<float> Size = new ParticleFieldDescription<float>("Size", 1);

        /// <summary>
        /// Particle remaining lifetime. When it reaches 0, the particle dies
        /// </summary>
        public static readonly ParticleFieldDescription<float> RemainingLife = new ParticleFieldDescription<float>("RemainingLife", 1);

    }
}
